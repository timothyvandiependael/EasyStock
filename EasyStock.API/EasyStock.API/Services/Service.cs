using EasyStock.API.Common;
using EasyStock.API.Repositories;

namespace EasyStock.API.Services
{
    public class Service<T> : IService<T> where T : class
    {
        private readonly IRepository<T> _repository;

        public Service(IRepository<T> repository)
        {
            _repository = repository;
        }

        public Task<T?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
        public Task<IEnumerable<T>> GetAllAsync() => _repository.GetAllAsync();
        public Task AddAsync(T entity) => _repository.AddAsync(entity);
        public Task UpdateAsync(T entity) => _repository.UpdateAsync(entity);
        public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
        public Task<PaginationResult<T>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => _repository.GetAdvancedAsync(filters, sorting, pagination);
    }
}
