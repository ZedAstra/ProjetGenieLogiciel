using Backend.Models.Communication;
using Backend.Models.Management;
using Backend.Models.Planning;
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
            /*modelBuilder.Entity<Utilisateur>()
                .HasMany(u => u.Chantiers)
                .WithMany(c => c.Membres)
                .UsingEntity(j => j.ToTable("ChantierMembre"));*/
            modelBuilder.Entity<Mouvement>().Property(mouvement => mouvement.Type).HasConversion<string>();
            modelBuilder.Entity<Tache>().Property(tache => tache.Status).HasConversion<string>();

            modelBuilder.Entity<Chantier>()
                .HasMany(c => c.Membres)
                .WithMany(u => u.Chantiers)
                .UsingEntity(j => j.ToTable("ChantierMembre"));
            // Annonce
            /*modelBuilder.Entity<Chantier>()
                .HasMany(c => c.Annonces)
                .WithOne(a => a.Chantier)
                .HasForeignKey(a => a.ChantierId)
                .OnDelete(DeleteBehavior.Cascade);
            // Tache
            modelBuilder.Entity<Chantier>()
                .HasMany(c => c.Taches)
                .WithOne(t => t.Chantier)
                .HasForeignKey(t => t.ChantierId)
                .OnDelete(DeleteBehavior.Cascade);
            // Rapport
            modelBuilder.Entity<Chantier>()
                .HasMany(c => c.Rapports)
                .WithOne(r => r.Chantier)
                .HasForeignKey(r => r.ChantierId)
                .OnDelete(DeleteBehavior.Cascade);
            // Ressource
            modelBuilder.Entity<Chantier>()
                .HasMany(c => c.Ressources)
                .WithOne(r => r.Chantier)
                .HasForeignKey(r => r.ChantierId)
                .OnDelete(DeleteBehavior.Cascade);
            // Mouvement
            modelBuilder.Entity<Chantier>()
                .HasMany(c => c.Mouvements)
                .WithOne(m => m.Chantier)
                .HasForeignKey(m => m.ChantierId)
                .OnDelete(DeleteBehavior.Cascade);

            // AutoIncrement

            modelBuilder.Entity<Utilisateur>()
                .Property(u => u.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<Chantier>()
                .Property(c => c.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<Ressource>()
                .Property(r => r.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<Mouvement>()
                .Property(m => m.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<Annonce>()
                .Property(a => a.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<Tache>()
                .Property(t => t.Id)
                .ValueGeneratedOnAdd();*/
        }

        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Chantier> Chantiers { get; set; }
        #region Management
        public DbSet<Ressource> Ressources { get; set; }
        public DbSet<Mouvement> Mouvements { get; set; }
        public DbSet<Rapport> Rapports { get; set; }
        #endregion Management
        #region Communication
        public DbSet<Annonce> Annonces { get; set; }
        #endregion Communication
        #region Planning
        public DbSet<Tache> Taches { get; set; }
        #endregion Planning
    }
}
