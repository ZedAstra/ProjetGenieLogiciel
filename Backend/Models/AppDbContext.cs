using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace Backend.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source=./app.db");
        }

        public DbSet<User> Users { get; set; }
    }
}
