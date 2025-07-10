using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface IPurchaseOrderLineService
    {
        Task<IEnumerable<PurchaseOrderLineOverview>> GetAllAsync();
        Task<PaginationResult<PurchaseOrderLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination);

        Task AddAsync(PurchaseOrderLine entity, string userName, bool fromParent = false);
        Task UpdateAsync(PurchaseOrderLine entity, string userName);
        Task DeleteAsync(int id, string userName, bool manageTransaction = true);
        Task BlockAsync(int id, string userName, bool manageTransaction = true);
        Task UnblockAsync(int id, string userName, bool manageTransaction = true);
    }
}
