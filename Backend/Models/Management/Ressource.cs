using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;

namespace Backend.Models.Management
{
    public class Ressource : BaseEntity
    {
        [Required]
        public string Nom { get; set; }
        [Required]
        public string Unite { get; set; }

        public decimal Quantite(AppDbContext db)
        {
            // Get all mouvements for this ressource and sort them by date
            var mouvements = db.Mouvements
                .Include(m => m.Ressource)
                .Include(m => m.Chantier)
                .Where(m => m.Ressource.Id == Id && m.Chantier.Id == Chantier.Id)
                .OrderBy(m => m.Date)
                .ToList();
            // Calculate the total quantity
            decimal total = 0;
            foreach (var mouvement in mouvements)
            {
                if (mouvement.Type == Mouvement.TypeMouvement.Entrée)
                {
                    total += mouvement.Quantite;
                }
                else if (mouvement.Type == Mouvement.TypeMouvement.Sortie)
                {
                    total -= mouvement.Quantite;
                }
            }
            return total;
        }

        public CompactRessource CompactEntity(AppDbContext db) => new()
        {
            Chantier = Chantier.Id,
            Id = Id,
            Nom = Nom,
            Unite = Unite,
            Quantite = Quantite(db)
        };

        public class CompactRessource
        {
            public int Chantier { get; init; }
            public int Id { get; init; }
            public string Nom { get; init; }
            public string Unite { get; init; }
            public decimal Quantite { get; init; }

            public Ressource Expand(DbContext db)
            {
                return db.Set<Ressource>().Find(Chantier, Id) ?? throw new Exception($"Ressource with id {Id} not found");
            }
        }
    }
}
