using System;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Management
{
    public class Ressource
    {
        [Key]
        public string Nom { get; set; }
        [Required]
        public decimal Quantité { get; set; } = -1;
        [Required]
        public string Unité { get; set; } = "?";
    }
}
