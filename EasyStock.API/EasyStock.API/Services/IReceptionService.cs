using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface IReceptionService
    {
        Task<IEnumerable<ReceptionOverview>> GetAllAsync();
        Task<PaginationResult<ReceptionOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination);
        Task AddAsync(Reception entity, string userName, bool useTransaction = true);
        Task DeleteAsync(int id, string userName, bool useTransaction = true);
        Task BlockAsync(int id, string userName, bool useTransaction = true);
        Task UnblockAsync(int id, string userName, bool useTransaction = true);
        Task<int> GetNextLineNumberAsync(int id);
    }
}
