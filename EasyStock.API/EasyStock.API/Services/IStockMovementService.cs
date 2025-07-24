using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface IStockMovementService
    {
        Task<IEnumerable<StockMovementOverview>> GetAllAsync();
        Task<PaginationResult<StockMovementOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination);
    }
}
