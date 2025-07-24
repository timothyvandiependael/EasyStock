using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Repositories
{
    public interface IDispatchLineRepository
    {
        Task<IEnumerable<DispatchLineOverview>> GetAllAsync();
        Task<PaginationResult<DispatchLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination);
    }
}
