using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Json.Serialization;

namespace Backend.Models.Management
{
    public class Mouvement : BaseEntity
    {
        public int RessourceId { get; set; }
        public Ressource Ressource { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Quantite { get; set; }
        public TypeMouvement Type { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum TypeMouvement
        {
            Entrée,
            Sortie
        }

        public CompactMouvement CompactEntity() => new()
        {
            Resource = RessourceId,
            Description = Description,
            Id = Id,
            Date = Date,
            Quantite = Quantite,
            Type = Type
        };

        public class CompactMouvement
        {
            public int Resource { get; init; }
            public string Description { get; init; } = string.Empty;
            public int Id { get; init; }
            public DateTime Date { get; init; }
            public decimal Quantite { get; init; }
            public TypeMouvement Type { get; init; }

            public Mouvement Expand(DbContext db)
            {
                return db.Set<Mouvement>().Find(Resource, Id) ?? throw new Exception($"Mouvement with id {Id} not found");
            }
        }
    }
}
