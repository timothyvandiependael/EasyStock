using EasyStock.API.Common;
using EasyStock.API.Dtos;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface ISalesOrderService
    {
        Task<IEnumerable<SalesOrderOverview>> GetAllAsync();
        Task<PaginationResult<SalesOrderOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination);
        Task<List<AutoRestockDto>> AddAsync(SalesOrder entity, string userName, bool useTransaction = true);
        Task DeleteAsync(int id, string userName, bool useTransaction = true);
        Task BlockAsync(int id, string userName, bool useTransaction = true);
        Task UnblockAsync(int id, string userName, bool useTransaction = true);
        Task<List<Product>> GetProductsWithSuppliersForOrderAsync(int id);
        Task<int> GetNextLineNumberAsync(int id);
        Task<bool> IsComplete(int id);
    }
}
