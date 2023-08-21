using Microsoft.EntityFrameworkCore;
using PastebinhezHasonlo.Models;

namespace PastebinhezHasonlo.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            :base(options)
        {
                
        }

        // Táblák
        DbSet<Message> Messages { get; set; }
    }
}
