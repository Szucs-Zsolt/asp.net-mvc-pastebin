using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace PastebinhezHasonlo.Models.ViewModels
{
    public class ChangePasswordVM
    {
        [Required(ErrorMessage = "{0} megadása szükséges.")]
        [DataType(DataType.Password)]
        [Display(Name = "Jelenlegi jelszó")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "{0} megadása szükséges.")]
        [DataType(DataType.Password)]
        [Display(Name = "Új jelszó")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "{0} megadása szükséges.")]
        [DataType(DataType.Password)]
        [Display(Name = "Új jelszó ismét")]
        [Compare("NewPassword", ErrorMessage =
            "A két jelszó nem egyezik meg.")]
        public string ConfirmPassword { get; set; }
    }
}
