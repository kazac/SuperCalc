using SuperCalc.Model;
using SuperCalc.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using static SuperCalc.Components.Pages.Home;
using static SuperCalc.Services.State;

namespace SuperCalc.Services;
    public class State
    {
    public State(IConfiguration configuration)
    {
        Configuration = configuration;
        //var data = await ProtectedSessionStore.GetAsync<Inputs>("data");
        //if (data != null)
        //    Inputs = data;
    }
    private IConfiguration Configuration { get; set; }
    public bool ShowingNewCaseDialog { get; private set; }
    public Case CurrentCase { get; private set; } = new();
    public Dictionary<string, Case> Cases { get; set; } = new();
    public record Line(int year, decimal super, decimal assets, decimal asset_test, decimal income_test, decimal part_pension, decimal annuity, decimal subtotal, decimal drawdown, decimal total);
    public IQueryable<Line> Lines { get; set; }  = Enumerable.Empty<Line>().AsQueryable();


    public void ShowNewCaseDialog()
    {
        CurrentCase = new Case()
        {
            Super = "",
            Annuity = "",
            Annuity_Percent = "",
            Assets = "",
            Target = "",
            Name = ""
            //Strategy = Strategy.Residual
        };

        ShowingNewCaseDialog = true;
    }

    public void CancelNewCaseDialog()
    {
        CurrentCase = new();
        ShowingNewCaseDialog = false;
    }

    public void ConfirmNewCaseDialog()
    {
        if (Cases.ContainsKey(CurrentCase.Name))
        {
            Cases[CurrentCase.Name] = CurrentCase;
        }
        else
            Cases.Add(CurrentCase.Name, CurrentCase);

        //    ConfiguringCase = null;
        DoCalc();
        ShowingNewCaseDialog = false;
    }

    public void RemoveCase(Case _case)
    {
        if (Cases.ContainsKey(CurrentCase.Name))
            Cases.Remove(_case.Name);
    }

    private void DoCalc()
    {
        Case c = CurrentCase;
        List<Line> items = new();
        if (c != null)
        {
            string name = c.Name.Trim();
            decimal superStart = decimal.Parse(c.Super); // 200000;
            decimal assets = decimal.Parse(c.Assets); // 25000;
            decimal annuity_cost = c.Annuity == "" ? 0M : decimal.Parse(c.Annuity); //200000;
            decimal annuity_pct = c.Annuity_Percent == "" ? 0M : decimal.Parse(c.Annuity_Percent); //200000;
            decimal annuity = annuity_cost * annuity_pct / 100M;
            decimal target = decimal.Parse(c.Target); //40000;
            decimal pension = Configuration.GetValue<decimal>("pension_single"); 

            decimal drawdown = 0M;
            decimal total_assets = 0M;
            if (c.Strategy == Strategy.Income)
            {
                var super = superStart - annuity_cost;
                for (int n = 0; n != 30; n++)
                {
                    var annuity_factor = n < 20 ? 0.6M : 0.3M;
                    total_assets = n == 0 ? super + assets + (annuity_factor * annuity_cost) : total_assets - drawdown;
                    super = n == 0 ? super : Math.Max(0, super - drawdown);
                    var reductionA_pct = Math.Max((total_assets - 301750M) / (667500M - 301750M), 0M);
                    var reductionA = reductionA_pct * pension;
                    var deeming = Math.Max((0.0225M * Math.Max((total_assets - 60400M), 0) + (0.0025M * Math.Min(total_assets, 60400M)) + (annuity * annuity_factor)), 0);
                    var part_pension = Math.Max(0, pension - Math.Max(reductionA, deeming));
                    var subtotal = part_pension + annuity;
                    drawdown = Math.Min(target - subtotal, super);  // should be super residual
                    var total = subtotal + drawdown;
                    var line = new Line(n, super, total_assets, reductionA, deeming, part_pension, annuity, subtotal, drawdown, total);
                    items.Add(line);
                }
            }
            else if (c.Strategy == Strategy.Residual)
            {
                decimal residual = 0M;
                var residual_target = target;
                target = 1000*Math.Round(pension/1000) ;
                do
                {
                    items.Clear();
                    var super = superStart - annuity_cost;
                    target = Math.Min(target, super);
                    for (int n = 0; n != 30; n++)
                    {
                        var annuity_factor = n < 20 ? 0.6M : 0.3M;
                        total_assets = n == 0 ? super + assets + (annuity_factor * annuity_cost) : total_assets - drawdown;
                        super = n == 0 ? super : Math.Max(super - drawdown, 0);
                        var reductionA_pct = Math.Max((total_assets - 301750M) / (667500M - 301750M), 0M);
                        var reductionA = reductionA_pct * pension;
                        var deeming = Math.Max((0.0225M * Math.Max((super + assets - 60400M), 0) + (0.0025M * Math.Min(super + assets, 60400M)) + (annuity * annuity_factor)), 0);
                        var part_pension = Math.Max(0,pension - Math.Max(reductionA, deeming));
                        var subtotal = part_pension + annuity;
                        drawdown = Math.Min(target - subtotal, super);  // should be super residual
                        var total = subtotal + drawdown;
                        var line = new Line(n, super, total_assets, reductionA, deeming, part_pension, annuity, subtotal, drawdown, total);
                        items.Add(line);
                    }
                    residual = items[29].super;
                    target += 100;
                } while (residual > residual_target);
            }
        }
        Lines = items.AsQueryable<Line>();
        //await ProtectedSessionStore.SetAsync("data", data);
        //Inputs = new Inputs();
    }
}
