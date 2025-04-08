using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;

namespace Backend.Models.Communication
{
    public class Cannal
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Nom { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public List<User> Admins { get; set; }
        [Required]
        public List<User> Participants { get; set; }

        public Small ToSmall()
        {
            return new Small
            {
                Id = Id,
                Nom = Nom,
                Description = Description,
                Admins = Admins.ConvertAll(u => u.Id),
                Participants = Participants.ConvertAll(u => u.Id)
            };
        }

        public class Small
        {
            public Guid Id { get; init; }
            public string Nom { get; init; }
            public string Description { get; init; }
            public List<Guid> Admins { get; init; }
            public List<Guid> Participants { get; init; }

            public Cannal ToFull(AppDbContext db)
            {
                return db.Channels.FirstOrDefault(c => c.Id == Id) ?? new Cannal
                {
                    Id = Id,
                    Nom = Nom,
                    Description = Description,
                    Admins = db.Users.Where(u => Admins.Contains(u.Id)).ToList(),
                    Participants = db.Users.Where(u => Participants.Contains(u.Id)).ToList()
                };
            }
        }
    }

    
}
