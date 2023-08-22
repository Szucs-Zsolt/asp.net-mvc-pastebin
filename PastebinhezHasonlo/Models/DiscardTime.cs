using Microsoft.AspNetCore.Routing.Constraints;

namespace PastebinhezHasonlo.Models
{
    // CreateModelVM használja az action-ben. Ebből készít SelectListItem-et
    // és az enum Time-ot adja vissza, hogy mit választottak ki a View-ban
    public class DiscardTime
    {
        public enum Time : int
        {
            AfterRead, 
            Hour1, Hour4, Hour8,
            Day1, Week1, Month1
        }
        public Dictionary<int, string> Names { get; set; }

        public DiscardTime()
        {
            Names = new Dictionary<int, string>
            {
                { (int)Time.AfterRead, "Elolvasása után azonnal" },
                { (int)Time.Hour1, "1 óra" },
                { (int)Time.Hour4, "4 óra" },
                { (int)Time.Hour8, "8 óra" },
                { (int)Time.Day1, "Egy nap" },
                { (int)Time.Week1, "Egy hét" },
                { (int)Time.Month1, "Egy hónap" }
            };
        }
    }
}