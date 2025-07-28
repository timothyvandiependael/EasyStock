using EasyStock.API.Models;

namespace EasyStock.API.Dtos
{
    public class OutputClientDetailDto : OutputPersonDto
    {
        public ICollection<OutputSalesOrderOverviewDto> SalesOrders { get; set; } = new List<OutputSalesOrderOverviewDto>();
    }
}
