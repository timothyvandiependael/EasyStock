using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface IReceptionLineService
    {
        Task<IEnumerable<ReceptionLineOverview>> GetAllAsync();
        Task<PaginationResult<ReceptionLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination);
    }
}
