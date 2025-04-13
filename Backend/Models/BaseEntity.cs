using Microsoft.EntityFrameworkCore;

namespace Backend.Models
{
    [PrimaryKey(nameof(ChantierId), nameof(Id))]
    public abstract class BaseEntity
    {
        public int ChantierId { get; set; }
        public Chantier Chantier { get; set; }
        public int Id { get; set; }
    }
}
