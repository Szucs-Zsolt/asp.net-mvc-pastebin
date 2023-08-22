using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PastebinhezHasonlo.Models
{
    // ViewModel a CreateMessage view számára
    public class CreateMessageVM
    {
        // ebbe kerül a tényleges üzenet
        public Message Message { get; set; }

        // A controllerben ez alapján fogjuk beállítani, hogy mikor kell törölni az adatot
        [ValidateNever]
        public int DiscardTime { get; set; }

        [ValidateNever]
        public List<SelectListItem> DiscardTimeList { get; set; }
    }
}
