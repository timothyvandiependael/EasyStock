using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Models
{
    public class DispatchLine : ModelBase
    {
        public int Id { get; set; }
        public int DispatchId { get; set; }
        public int LineNumber { get; set; }
        public required Dispatch Dispatch { get; set; }


        [MaxLength(1000)]
        public string? Comments { get; set; }
        public int ProductId { get; set; }
        public required Product Product { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public int SalesOrderLineId { get; set; }
        public required SalesOrderLine SalesOrderLine { get; set; }
    }
}
