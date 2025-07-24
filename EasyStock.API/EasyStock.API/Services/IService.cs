using EasyStock.API.Common;

namespace EasyStock.API.Services
{
    public interface IService<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity, string userName);
        Task UpdateAsync(T entity, string userName);
        Task DeleteAsync(int id);
        Task BlockAsync(int id, string userName);
        Task UnblockAsync(int id, string userName);
        Task<PaginationResult<T>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination);
    }
}
