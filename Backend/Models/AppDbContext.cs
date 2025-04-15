using Backend.Models.Management;
using Microsoft.EntityFrameworkCore;
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
            modelBuilder.Entity<Utilisateur>().Property(user => user.RoleUtilisateur).HasConversion<string>();
            modelBuilder.Entity<Mouvement>().Property(mouvement => mouvement.Type).HasConversion<string>();
        }

        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Chantier> Chantiers { get; set; }
        #region Management
        public DbSet<Ressource> Ressources { get; set; }
        public DbSet<Mouvement> Mouvements { get; set; }
        public DbSet<Rapport> Rapports { get; set; }
        #endregion Management
        #region Communication
        //
        #endregion Communication
    }
}
