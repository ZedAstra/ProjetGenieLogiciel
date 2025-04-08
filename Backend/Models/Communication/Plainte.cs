using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;

namespace Backend.Models.Communication
{
    public class Plainte
    {
        [Key]
        public Guid Id { get; set; }
        [Required] public string Titre { get; set; }
        [Required] public User Auteur { get; set; }
        [Required] public string Contenu { get; set; }

        public Small ToSmall()
        {
            return new Small
            {
                Id = Id,
                Titre = Titre,
                Auteur = Auteur.Id,
                Contenu = Contenu
            };
        }

        public class Small
        {
            public Guid Id { get; init; }
            public string Titre { get; init; }
            public Guid Auteur { get; init; }
            public string Contenu { get; init; }

            public Plainte ToFull(AppDbContext db)
            {
                return db.Plaintes.FirstOrDefault(m => m.Id == Id) ?? new Plainte
                {
                    Id = Id,
                    Titre = Titre,
                    Auteur = db.Users.FirstOrDefault(u => u.Id == Auteur) ?? new User(),
                    Contenu = Contenu
                };
            }
        }
    }
}
