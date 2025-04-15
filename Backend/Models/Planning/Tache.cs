using System;
using System.Text.Json.Serialization;

namespace Backend.Models.Planning
{
    public class Tache : BaseEntity
    {
        public string Titre { get; set; }
        public string Description { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public EtatTache Status { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum EtatTache
        {
            NonCommencee,
            EnCours,
            Terminee,
            Annulee
        }
    }
}
