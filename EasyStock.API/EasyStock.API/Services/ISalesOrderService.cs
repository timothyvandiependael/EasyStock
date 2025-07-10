using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface ISalesOrderService
    {
        Task<IEnumerable<SalesOrderOverview>> GetAllAsync();
        Task<PaginationResult<SalesOrderOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination);
        Task AddAsync(SalesOrder entity, string userName);
        Task DeleteAsync(int id, string userName);
        Task BlockAsync(int id, string userName);
        Task UnblockAsync(int id, string userName);
        Task<List<Product>> GetProductsWithSuppliersForOrderAsync(int id);
        Task<int> GetNextLineNumberAsync(int id);
        Task<bool> IsComplete(int id);
    }
}
