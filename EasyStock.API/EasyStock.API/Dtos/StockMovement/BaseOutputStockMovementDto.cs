using EasyStock.API.Models;
using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public class BaseOutputStockMovementDto : OutputDtoBase
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int QuantityChange { get; set; }
        public string Reason { get; set; }
        public int? PurchaseOrderId { get; set; }
        public int? SalesOrderId { get; set; }
    }
}
