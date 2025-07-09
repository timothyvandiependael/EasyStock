using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface ISalesOrderLineService
    {
        Task<IEnumerable<SalesOrderLineOverview>> GetAllAsync();
        Task<PaginationResult<SalesOrderLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination);
        Task AddAsync(SalesOrderLine entity, string userName);
        Task UpdateAsync(SalesOrderLine entity, string userName);
        Task DeleteAsync(int id, string userName, bool manageTransaction = true);
        Task BlockAsync(int id, string userName, bool manageTransaction = true);
        Task UnblockAsync(int id, string userName, bool manageTransaction = true);
    }
}
