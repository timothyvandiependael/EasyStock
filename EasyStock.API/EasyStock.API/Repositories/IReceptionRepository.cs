using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Repositories
{
    public interface IReceptionRepository
    {
        Task<IEnumerable<ReceptionOverview>> GetAllAsync();
        Task<PaginationResult<ReceptionOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination);
    }
}
