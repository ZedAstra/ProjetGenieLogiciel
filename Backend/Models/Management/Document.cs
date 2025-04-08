using System;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Management
{
    public class Document
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Nom { get; set; }
        [Required]
        public string Contenu { get; set; }
    }
}
