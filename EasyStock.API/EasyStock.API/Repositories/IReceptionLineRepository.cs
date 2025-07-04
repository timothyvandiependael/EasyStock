using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Repositories
{
    public interface IReceptionLineRepository
    {
        Task<IEnumerable<ReceptionLineOverview>> GetAllAsync();
        Task<PaginationResult<ReceptionLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination);
    }
}
