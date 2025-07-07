using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;

namespace EasyStock.API.Services
{
    public class Service<T> : IService<T> where T : ModelBase
    {
        private readonly IRepository<T> _repository;

        public Service(IRepository<T> repository)
        {
            _repository = repository;
        }

        public async Task<T?> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);
        public async Task<IEnumerable<T>> GetAllAsync() => await _repository.GetAllAsync();
        public async Task AddAsync(T entity, string userName)
        {
            entity.LcUserId = userName;
            entity.LcDate = DateTime.UtcNow;
            entity.CrUserId = userName;
            entity.CrDate = entity.LcDate;
            await _repository.AddAsync(entity);
        }
        public async Task UpdateAsync(T entity, string userName)
        {
            entity.LcUserId = userName;
            entity.LcDate = DateTime.UtcNow;
            await _repository.UpdateAsync(entity);
        }

        public async Task BlockAsync(int id, string userName)
        {
            var entity = await _repository.GetByIdAsync(id);
            entity.BlDate = DateTime.UtcNow;
            entity.BlUserId = userName;
            await _repository.UpdateAsync(entity);
        }

        public async Task UnblockAsync(int id, string userName)
        {
            var entity = await _repository.GetByIdAsync(id);
            entity.BlDate = null;
            entity.BlUserId = null;
            entity.LcDate = DateTime.Now;
            entity.LcUserId = userName;
            await _repository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(int id) 
        {
            await _repository.DeleteAsync(id); 
        }
        public async Task<PaginationResult<T>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _repository.GetAdvancedAsync(filters, sorting, pagination);
    }
}
