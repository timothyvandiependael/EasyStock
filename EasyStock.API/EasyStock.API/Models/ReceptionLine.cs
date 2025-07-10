using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Models
{
    public class ReceptionLine : ModelBase
    {
        public int Id { get; set; }
        public int ReceptionId { get; set; }
        public int LineNumber { get; set; }

        [MaxLength(1000)]
        public string? Comments { get; set; }

        public required Reception Reception { get; set; }
        public int ProductId { get; set; }
        public required Product Product { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public int PurchaseOrderLineId { get; set; }
        public required PurchaseOrderLine PurchaseOrderLine { get; set; }
    }
}
