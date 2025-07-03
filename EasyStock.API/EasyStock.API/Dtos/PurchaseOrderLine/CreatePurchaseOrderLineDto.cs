using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public class CreatePurchaseOrderLineDto
    {
        public int PurchaseOrderId { get; set; }
        public int LineNumber { get; set; }
        public int ProductId { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }
    }
}
