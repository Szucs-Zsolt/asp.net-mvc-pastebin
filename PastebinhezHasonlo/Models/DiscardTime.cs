namespace PastebinhezHasonlo.Models
{
    public class DiscardTime : List<String>
    {
        public DiscardTime()
        {
            // Mennyi idő után töröljük?
            this.Add("Első olvasáskor");
            this.Add("1 óra");
            this.Add("4 óra");
            this.Add("8 óra");
            this.Add("Egy nap");
            this.Add("Egy hét");
            this.Add("Egy hónap");
        }
    }
}