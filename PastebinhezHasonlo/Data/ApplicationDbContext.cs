using Microsoft.EntityFrameworkCore;
using PastebinhezHasonlo.Models;

using Microsoft.AspNetCore.Identity;                    
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace PastebinhezHasonlo.Data
{
    
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        
        }

        // Táblák
        public DbSet<Message> Messages { get; set; }

        // Alapértékkel feltöltjük, hogy legyen min tesztelni.

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Message>().HasData(
                new Message()
                {
                    MessageId = "1",
                    Msg = "Példaüzenet, hogy az adatbázis létrehozásakor már legyen benne valami.",
                    DiscardFirstRead = false,
                    DiscardDate = DateTime.Now.AddMonths(1)
                }
            );
        }
    }
}
