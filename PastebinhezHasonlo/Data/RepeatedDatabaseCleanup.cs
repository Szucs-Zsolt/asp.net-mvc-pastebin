namespace PastebinhezHasonlo.Data
{
    public class RepeatedDatabaseCleanup : BackgroundService
    {
        // private readonly PeriodicTimer _timer = new PeriodicTimer(TimeSpan.FromHours(1));
        private readonly PeriodicTimer _timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        protected override async Task ExecuteAsync(CancellationToken stopToken)
        {
            while (await _timer.WaitForNextTickAsync(stopToken)
                && !stopToken.IsCancellationRequested) 
            {
                Console.WriteLine(DateTime.Now.ToString());
            }
        }
    }
}
