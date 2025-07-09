using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface IReceptionService
    {
        Task<IEnumerable<ReceptionOverview>> GetAllAsync();
        Task<PaginationResult<ReceptionOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination);
        Task AddAsync(Reception entity, string userName);
        Task DeleteAsync(int id, string userName);
        Task BlockAsync(int id, string userName);
        Task UnblockAsync(int id, string userName);
    }
}
