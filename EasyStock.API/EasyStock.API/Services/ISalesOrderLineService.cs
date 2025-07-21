using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface ISalesOrderLineService
    {
        Task<IEnumerable<SalesOrderLineOverview>> GetAllAsync();
        Task<PaginationResult<SalesOrderLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination);
        Task AddAsync(SalesOrderLine entity, string userName);
        Task UpdateAsync(SalesOrderLine entity, string userName, bool useTransaction = true);
        Task DeleteAsync(int id, string userName);
        Task BlockAsync(int id, string userName);
        Task UnblockAsync(int id, string userName);
    }
}
