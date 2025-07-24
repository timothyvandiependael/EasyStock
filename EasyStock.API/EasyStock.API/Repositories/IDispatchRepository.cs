using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Repositories
{
    public interface IDispatchRepository
    {
        Task<IEnumerable<DispatchOverview>> GetAllAsync();
        Task<PaginationResult<DispatchOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination);
    }
}
