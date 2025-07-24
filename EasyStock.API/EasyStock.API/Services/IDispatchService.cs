using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface IDispatchService
    {
        Task<IEnumerable<DispatchOverview>> GetAllAsync();
        Task<PaginationResult<DispatchOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination);
        Task AddAsync(Dispatch entity, string userName, bool useTransaction = true);
        Task DeleteAsync(int id, string userName, bool useTransaction = true);
        Task BlockAsync(int id, string userName, bool useTransaction = true);
        Task UnblockAsync(int id, string userName, bool useTransaction = true);
        Task<int> GetNextLineNumberAsync(int id);
        Task<Dispatch?> AddFromSalesOrder(int salesOrderId, string userName, bool useTransaction = true);
    }
}
