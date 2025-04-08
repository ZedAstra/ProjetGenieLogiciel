using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Backend.Models.Planning
{
    public class Tache
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public DateTime Début { get; set; }
        [Required]
        public DateTime Fin { get; set; }
        [Required]
        public string Nom { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public Statut Etat { get; set; } = Statut.EnAttente;
        public List<User> Assignés { get; set; } = new List<User>();

        [IgnoreDataMember, JsonIgnore]
        public bool Terminé => DateTime.Now > Fin;
        [IgnoreDataMember, JsonIgnore]
        public bool Debuté => DateTime.Now > Début && DateTime.Now < Fin;

        public enum Statut
        {
            EnAttente,
            EnCours,
            Terminé,
            Annulé
        }

        public Small ToSmall()
        {
            return new Small
            {
                Id = Id,
                Début = Début,
                Fin = Fin,
                Nom = Nom,
                Description = Description,
                Etat = Etat,
                Assignés = Assignés.ConvertAll(u => u.Id),
                Terminé = Terminé,
                Debuté = Debuté
            };
        }

        public class Small
        {
            public Guid Id { get; init; }
            public DateTime Début { get; init; }
            public DateTime Fin { get; init; }
            public string Nom { get; init; }
            public string Description { get; init; }
            public Statut Etat { get; init; }
            public List<Guid> Assignés { get; init; }
            public bool Terminé { get; init; }
            public bool Debuté { get; init; }

            public Tache ToFull(AppDbContext db)
            {
                return db.Taches.Find(Id) ?? new Tache
                {
                    Id = Id,
                    Début = Début,
                    Fin = Fin,
                    Nom = Nom,
                    Description = Description,
                    Etat = Etat,
                    Assignés = db.Users.Where(u => Assignés.Contains(u.Id)).ToList()
                };
            }
        }
    }
}
