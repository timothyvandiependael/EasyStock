using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Models
{
    public class Dispatch : ModelBase, IEntity
    {
        public int Id { get; set; }
        public required string DispatchNumber { get; set; }

        [MaxLength(1000)]
        public string? Comments { get; set; }
        public int ClientId { get; set; }
        public required virtual Client Client { get; set; }
        public virtual ICollection<DispatchLine> Lines { get; set; } = new List<DispatchLine>();
    }
}
