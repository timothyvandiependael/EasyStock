using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface IClientService
    {
        Task<PaginationResult<Client>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination);
    }
}
