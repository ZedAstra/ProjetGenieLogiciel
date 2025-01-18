using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Backend.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        [IgnoreDataMember]
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
        public UserType Type { get; set; }
        [Required]
        public Role UserRole { get; set; }


        public enum UserType
        {
            Admin,
            User
        }
        public enum Role
        {
            Chef,
            Ouvrier,
            Partenaire,
        }


    }
}
