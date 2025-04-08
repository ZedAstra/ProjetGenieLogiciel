using Backend.Models.Communication;
using Backend.Models.Management;
using Backend.Models.Planning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.IO;

namespace Backend.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={Path.Combine(Program.ExeDirectory.FullName, "app.db")}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().Property(user => user.UserRole).HasConversion<string>();
            modelBuilder.Entity<Tache>().Property(tache => tache.Etat).HasConversion<string>();
        }

        public DbSet<User> Users { get; set; }

        // Planning
        public DbSet<Tache> Taches { get; set; }

        // Management
        public DbSet<Document> Documents { get; set; }
        public DbSet<Ressource> Ressources { get; set; }

        // Communication
        public DbSet<Cannal> Channels { get; set; }
        public DbSet<Annonce> Annonces { get; set; }
        public DbSet<Plainte> Plaintes { get; set; }
        public DbSet<Message> Messages { get; set; }
    }
}
