using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Models
{
    public class Reception : ModelBase
    {
        public int Id { get; set; }
        [Required]
        public required string ReceptionNumber { get; set; }

        [MaxLength(1000)]
        public string? Comments { get; set; }

        public int SupplierId { get; set; }
        public required Supplier Supplier { get; set; }
        public ICollection<ReceptionLine> Lines { get; set; } = new List<ReceptionLine>();
    }
}
