using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Repositories
{
    public interface ISalesOrderRepository
    {
        Task<IEnumerable<SalesOrderOverview>> GetAllAsync();
        Task<PaginationResult<SalesOrderOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination);
    }
}
