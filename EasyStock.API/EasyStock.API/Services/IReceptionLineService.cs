using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface IReceptionLineService
    {
        Task<IEnumerable<ReceptionLineOverview>> GetAllAsync();
        Task<PaginationResult<ReceptionLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination);
        Task AddAsync(ReceptionLine entity, string userName);
        Task UpdateAsync(ReceptionLine entity, string userName, bool useTransaction = true);
        Task DeleteAsync(int id, string userName);
        Task BlockAsync(int id, string userName);
        Task UnblockAsync(int id, string userName);
    }
}
