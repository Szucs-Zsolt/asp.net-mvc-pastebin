﻿using Microsoft.EntityFrameworkCore;
using PastebinhezHasonlo.Models;

using Microsoft.AspNetCore.Identity;                    // KELL
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace PastebinhezHasonlo.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            :base(options)
        {
                
        }

        // Táblák
        public DbSet<Message> Messages { get; set; }

        // Alapértékkel feltöltjük, hogy legyen min tesztelni.

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Message>().HasData(
                new Message() { Id = 1, MessageId = "1", Msg = "Példaüzenet." },
                new Message() { Id = 2, MessageId = "2", Msg = "Második üzenet."}
            );
        }
    }
}
