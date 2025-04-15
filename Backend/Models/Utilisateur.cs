using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class Utilisateur
    {
        [Key]
        [Description("Ignorer lors de la création d'un utilisateur")]
        public int Id { get; set; }
        [IgnoreDataMember, JsonIgnore]
        public string Name => $"{Prenom} {Nom}";
        [Required]
        public string Prenom { get; set; }
        [Required] 
        public string Nom { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string MotDePasse { get; set; }
        [Required]
        public Role RoleUtilisateur { get; set; }
        public List<Chantier> Chantiers { get; internal set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum Role
        {
            None,
            Admin,
            Chef,
            Partenaire,
            Ouvrier,
        }

        public SafeUtilisateur CompactEntity() => new()
        {
            Id = Id,
            Name = Name,
            FirstName = Prenom,
            LastName = Nom,
            UserRole = RoleUtilisateur
        };

        public class SafeUtilisateur
        {
            public int Id { get; init; }
            public string Name { get; init; }
            public string FirstName { get; init; }
            public string LastName { get; init; }
            public Role UserRole { get; init; }

            public Utilisateur Expand(DbContext db)
            {
                return db.Set<Utilisateur>().Find(Id) ?? throw new Exception($"User with id {Id} not found");
            }
        }
    }
}
