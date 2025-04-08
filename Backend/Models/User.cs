using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class User
    {
        [Key]
        [Description("Ignorer lors de la création d'un utilisateur")]
        public Guid Id { get; set; }
        [IgnoreDataMember, JsonIgnore]
        public string Name => $"{FirstName} {LastName}";
        [Required]
        public string FirstName { get; set; }
        [Required] 
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public Role UserRole { get; set; }

        public enum Role
        {
            None,
            Admin,
            Chef,
            Ouvrier,
            Partenaire,
        }

        [JsonDerivedType(typeof(Safe))]
        public class Safe : User
        {
            public Guid Id => base.Id;
            public string Name => base.Name;
            public string FirstName => base.FirstName;
            public string LastName => base.LastName;
            public string Email => base.Email;
            public Role UserRole => base.UserRole;
        }
    }
}
