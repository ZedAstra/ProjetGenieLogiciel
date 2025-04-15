using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Backend.Models.Management
{
    public class Rapport : BaseEntity
    {
        public int Annee { get; set; }
        public int Mois { get; set; }
        public byte[]? Fichier { get; set; }
    }
}
