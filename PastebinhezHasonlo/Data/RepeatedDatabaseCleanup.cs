using PastebinhezHasonlo.Models;

namespace PastebinhezHasonlo.Data
{
    public class RepeatedDatabaseCleanup : BackgroundService
    {
        // Adatbázis elérése
        private readonly ApplicationDbContext _db;
        public RepeatedDatabaseCleanup(IServiceScopeFactory factory)
        {
            _db = factory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();       
        }

        // Általános karbantartás: Óránként ellenőrzi és törli a lejárt üzeneteket.
        // Mivel ez nem teljesen pontos, ezért megjelenítés előtt is ellenőrizni fogjuk,
        // hogy nem járt-e le egy üzenet ideje.
        private readonly PeriodicTimer _timer = new PeriodicTimer(TimeSpan.FromHours(1));
        protected override async Task ExecuteAsync(CancellationToken stopToken)
        {
            while (await _timer.WaitForNextTickAsync(stopToken)
                && !stopToken.IsCancellationRequested)
            {
                // Adatbázis tisztítása (ha már túl vagyunk a lejárati idején)
                DateTime currentDateTime = DateTime.Now;

                _db.Messages.RemoveRange(_db.Messages.Where(x => x.DiscardDate < currentDateTime));
                _db.SaveChanges();
            }
        }
    }
}
