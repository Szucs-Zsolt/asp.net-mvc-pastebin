using System.ComponentModel.DataAnnotations;

namespace PastebinhezHasonlo.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage="{0} megadása szükséges.")]
        [Display(Name ="Üzenet azonosító")]
        public string MessageId { get; set; }

        [Required(ErrorMessage ="{0} megadása szükséges.")]
        [MaxLength(8000, ErrorMessage ="{0} max. {1} karakter hosszú lehet.")]
        [Display(Name ="Üzenet")]
        public string Msg { get; set; }
    }
}
