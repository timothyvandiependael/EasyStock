using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Repositories
{
    public interface ISalesOrderLineRepository
    {
        Task<IEnumerable<SalesOrderLineOverview>> GetAllAsync();
        Task<PaginationResult<SalesOrderLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination);
    }
}
