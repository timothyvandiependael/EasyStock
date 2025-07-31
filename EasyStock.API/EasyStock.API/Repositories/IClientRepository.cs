using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Repositories
{
    public interface IClientRepository
    {
        Task<PaginationResult<Client>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination);
    }
}
