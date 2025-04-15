using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public int ChantierId { get; set; }
        [JsonIgnore]
        public Chantier Chantier { get; set; }
        
    }
}
