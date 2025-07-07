using EasyStock.API.Models;
using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public class BaseOutputSalesOrderLineDto : OutputDtoBase
    {
        public int Id { get; set; }
        public int SalesOrderId { get; set; }
        public int LineNumber { get; set; }
        public string? Comments { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public required string Status { get; set; }
    }
}
