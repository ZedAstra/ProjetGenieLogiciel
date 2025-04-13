using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models.Management
{
    public class Ressource : BaseEntity
    {
        [Required]
        public string Nom { get; set; }
        [Required]
        public string Unite { get; set; }
        [Required]
        public decimal Quantite { get; set; }

        public CompactRessource CompactEntity() => new()
        {
            Chantier = Chantier.Id,
            Id = Id,
            Nom = Nom,
            Unite = Unite,
            Quantite = Quantite
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
