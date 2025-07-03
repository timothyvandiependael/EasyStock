using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface ISalesOrderService
    {
        Task<IEnumerable<SalesOrderOverview>> GetAllAsync();
        Task<PaginationResult<SalesOrderOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination);
    }
}
