using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Repositories
{
    public interface IPurchaseOrderLineRepository
    {
        Task<IEnumerable<PurchaseOrderLineOverview>> GetAllAsync();
        Task<PaginationResult<PurchaseOrderLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination);
    }
}
