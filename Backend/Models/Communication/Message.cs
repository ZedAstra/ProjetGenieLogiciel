using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;

namespace Backend.Models.Communication
{
    public class Message
    {
        [Key]
        public Guid Id { get; set; }
        [Required] public Cannal Cannal { get; set; }
        [Required] public User Auteur { get; set; }
        [Required] public string Contenu { get; set; }

        public Small ToSmall()
        {
            return new Small
            {
                Id = Id,
                Cannal = Cannal.Id,
                Auteur = Auteur.Id,
                Contenu = Contenu
            };
        }

        public class Small
        {
            public Guid Id { get; init; }
            public Guid Cannal { get; init; }
            public Guid Auteur { get; init; }
            public string Contenu { get; init; }

            public Message ToFull(AppDbContext db)
            {
                return db.Messages.FirstOrDefault(m => m.Id == Id) ?? new Message
                {
                    Id = Id,
                    Cannal = db.Channels.FirstOrDefault(c => c.Id == Cannal) ?? new Cannal(),
                    Auteur = db.Users.FirstOrDefault(u => u.Id == Auteur) ?? new User(),
                    Contenu = Contenu
                };
            }
        }
    }

    
}
