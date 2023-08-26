using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace PastebinhezHasonlo.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        
        [Display(Name ="Üzenetazonosító")]
        public string? MessageId { get; set; }

        [Required(ErrorMessage = "{0} megadása szükséges.")]
        [MaxLength(8000, ErrorMessage = "{0} max. {1} karakter hosszú lehet.")]
        [Display(Name = "Üzenet")]
        public string Msg { get; set; }

        [ValidateNever]            // Controllerben írjuk bele, nem formból jövő adat
        public string? UserId { get; set; }         // Ki írta az üzenetet

        [ValidateNever]
        [Display(Name ="Első elolvasás után töröljük")]
        public bool DiscardFirstRead { get; set; }      // első olvasáskor töröljük?

        [ValidateNever]
        public DateTime DiscardDate { get; set; }		// ekkor mindenkép

    }
}
