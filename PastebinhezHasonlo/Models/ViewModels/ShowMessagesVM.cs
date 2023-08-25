namespace PastebinhezHasonlo.Models.ViewModels
{
    public class ShowMessagesVM
    {
        // Az összes üzenet, amit a user írt
        public IEnumerable<Message> MessageList { get; set; }

        // A user neve
        public string UserName { get; set; }

        // Az admin nézi valaki más üzeneteit. (false = user nézi a saját üzeneteit)
        public bool IsAdminSupervision { get; set; }
    }
}
