using System.ComponentModel.DataAnnotations;

namespace PastebinhezHasonlo.Models.ViewModels
{
    public class DeleteUserVM
    {
        [Display(Name ="Név")]
        public string UserName { get; set; }

        [Display(Name = "Email")]
        public  string  Email { get; set; }
    }
}
