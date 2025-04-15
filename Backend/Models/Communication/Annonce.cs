using System;

namespace Backend.Models.Communication
{
    public class Annonce : BaseEntity
    {
        public string Titre { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string Contenu { get; set; }
    }
}
