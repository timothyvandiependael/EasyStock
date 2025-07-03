using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface IPurchaseOrderService
    {
        Task<IEnumerable<PurchaseOrderOverview>> GetAllAsync();
        Task<PaginationResult<PurchaseOrderOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination);
    }
}
