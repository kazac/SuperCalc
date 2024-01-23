using System.ComponentModel.DataAnnotations;

namespace SuperCalc
{
    public class Inputs
    {
        [Required]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Super must be a number of the form 0000000")]
        public string Super { get; set; } = "0";

        [RegularExpression(@"^\d{5,6}$", ErrorMessage = "Annuity must be a number of the form 00000 to 0000000")]
        public string Annuity { get; set; } = "";

        [RegularExpression(@"^\d{1}$", ErrorMessage = "Annuity payment must be a single digit number")]
        public string Annuity_Percent { get; set; } = "5";

        [Required]
        [RegularExpression(@"^\d{1,6}$", ErrorMessage = "Assets must be a number of the form 0 to 0000000")]
        public string Assets { get; set; } = "40000";

        [Required]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Target must be a number of the form 000000")]
        public string Target { get; set; } = "35000";

        public string Name { get; set; } = "";

    };
}
