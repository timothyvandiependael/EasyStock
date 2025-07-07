using EasyStock.API.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyStock.API.Models
{
    public class OrderNumberCounter
    {
        public int Id { get; set; }

        [Column(TypeName = "varchar(2)")]
        public OrderType OrderType { get; set; }
        public DateOnly Date { get; set; }
        public int LastNumber { get; set; }
    }
}
