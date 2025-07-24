using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface IPurchaseOrderLineService
    {
        Task<IEnumerable<PurchaseOrderLineOverview>> GetAllAsync();
        Task<PaginationResult<PurchaseOrderLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination);

        Task AddAsync(PurchaseOrderLine entity, string userName);
        Task UpdateAsync(PurchaseOrderLine entity, string userName, bool useTransaction = true);
        Task DeleteAsync(int id, string userName);
        Task BlockAsync(int id, string userName);
        Task UnblockAsync(int id, string userName);
    }
}
