using EasyStock.API.Common;

namespace EasyStock.API.Services
{
    public interface IService<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task<PaginationResult<T>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination);
    }
}
