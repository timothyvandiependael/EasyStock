using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Repositories
{
    public interface IPurchaseOrderRepository
    {
        Task<IEnumerable<PurchaseOrderOverview>> GetAllAsync();
        Task<PaginationResult<PurchaseOrderOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination);
    }
}
