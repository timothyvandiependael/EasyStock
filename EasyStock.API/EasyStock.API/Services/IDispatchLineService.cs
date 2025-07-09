using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface IDispatchLineService
    {
        Task<IEnumerable<DispatchLineOverview>> GetAllAsync();
        Task<PaginationResult<DispatchLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination);
        Task AddAsync(DispatchLine entity, string userName);
        Task UpdateAsync(DispatchLine entity, string userName);
        Task DeleteAsync(int id, string userName, bool manageTransaction = true);
        Task BlockAsync(int id, string userName, bool manageTransaction = true);
        Task UnblockAsync(int id, string userName, bool manageTransaction = true);
    }
}
