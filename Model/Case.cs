using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace SuperCalc.Model
{
    public class Case
    {
        [Required]
        [RegularExpression(@"^\d{5,7}$", ErrorMessage = "Super must be a number of the form 00000 to 0000000")]
        public string Super { get; set; } = "0";

        [RegularExpression(@"^\d{5,6}$", ErrorMessage = "Annuity (if entered) must be a number of the form 00000 to 0000000")]
        public string Annuity { get; set; } = "";

        [RegularExpression(@"^\d{1}$", ErrorMessage = "Annuity payment (if entered) must be a single digit number")]
        public string Annuity_Percent { get; set; } = "5";

        [Required]
        [RegularExpression(@"^\d{1,6}$", ErrorMessage = "Assets must be a number of the form 0 to 0000000")]
        public string Assets { get; set; } = "40000";
        public Strategy Strategy { get; set; } = Strategy.Residual;
        [Required]
        [RegularExpression(@"^\d{1,5}$", ErrorMessage = "Target must be a number of the form 0 to 000000")]
        public string Target { get; set; } = "35000";

        public string Name { get; set; } = "default";
    }

    public enum Strategy
    {
        Residual,
        Income
    }
}
