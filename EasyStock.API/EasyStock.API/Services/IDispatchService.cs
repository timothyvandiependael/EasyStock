using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface IDispatchService
    {
        Task<IEnumerable<DispatchOverview>> GetAllAsync();
        Task<PaginationResult<DispatchOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination);
    }
}
