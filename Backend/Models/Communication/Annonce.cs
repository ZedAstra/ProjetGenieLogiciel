using System;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Communication
{
    public class Annonce
    {
        [Key]
        public Guid Id { get; set; }
        [Required] public string Titre { get; set; }
        [Required] public DateTime Publication { get; set; }
        [Required] public string Contenu { get; set; }
    }
}
