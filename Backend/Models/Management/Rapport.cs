using Microsoft.EntityFrameworkCore;

namespace Backend.Models.Management
{
    [PrimaryKey(nameof(ChantierId), nameof(Annee), nameof(Mois))]
    public class Rapport
    {
        public int ChantierId { get; set; }
        public int Annee { get; set; }
        public int Mois { get; set; }
        public byte[]? Fichier { get; set; }
    }
}
