using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface IReceptionLineService
    {
        Task<IEnumerable<ReceptionLineOverview>> GetAllAsync();
        Task<PaginationResult<ReceptionLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination);

        Task AddAsync(ReceptionLine entity, string userName);
        Task UpdateAsync(ReceptionLine entity, string userName);
        Task DeleteAsync(int id, string userName, bool manageTransaction = true);
        Task BlockAsync(int id, string userName, bool manageTransaction = true);
        Task UnblockAsync(int id, string userName, bool manageTransaction = true);
    }
}
