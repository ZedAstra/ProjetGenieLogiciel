using Backend.Models.Communication;
using Backend.Models.Management;
using Backend.Models.Planning;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class Chantier
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nom { get; set; } = string.Empty;
        [Required]
        public string Details { get; set; } = string.Empty;
        [Required]
        public DateOnly DateDebut { get; set; }
        [Required]
        public StatusChantier Status { get; set; }

        public List<Utilisateur> Membres { get; set; } = [];
        public List<Annonce> Annonces { get; set; } = [];
        public List<Tache> Taches { get; set; } = [];
        public List<Rapport> Rapports { get; set; } = [];
        public List<Ressource> Ressources { get; set; } = [];
        public List<Mouvement> Mouvements { get; set; } = [];

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum StatusChantier
        {
            EnAttente,
            EnCours,
            Terminé,
            Annulé
        }

        public CompactChantier CompactEntity() => new()
        {
            Id = Id,
            Nom = Nom,
            Details = Details,
            DateDebut = DateDebut,
            Status = Status,
            Membres = Membres.ConvertAll(m => m.Id)
        };

        public class CompactChantier
        {
            public int Id { get; set; }
            public string Nom { get; set; } = string.Empty;
            public string Details { get; set; } = string.Empty;
            public DateOnly DateDebut { get; set; }
            public StatusChantier Status { get; set; }
            public List<int> Membres { get; set; } = [];

            public Chantier Expand(DbContext db)
            {
                return db.Set<Chantier>().Find(Id) ?? throw new Exception($"Chantier with id {Id} not found");
            }
        }
    }
}
