using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Models
{
    public class Reception : ModelBase, IEntity
    {
        public int Id { get; set; }
        [Required]
        public required string ReceptionNumber { get; set; }

        [MaxLength(1000)]
        public string? Comments { get; set; }

        public int SupplierId { get; set; }
        public required virtual Supplier Supplier { get; set; }
        public virtual ICollection<ReceptionLine> Lines { get; set; } = new List<ReceptionLine>();
    }
}
