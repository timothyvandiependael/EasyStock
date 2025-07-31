using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;

namespace EasyStock.API.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _repository;

        public SupplierService(ISupplierRepository repository)
        {
            _repository = repository;
        }

        public async Task<PaginationResult<Supplier>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination)
            => await _repository.GetAdvancedAsync(filters, sorting, pagination);

        public async Task AddProductAsync(int id, int productId) => await _repository.AddProductAsync(id, productId);

        public async Task RemoveProductAsync(int id, int productId) => await _repository.RemoveProductAsync(id, productId);
    }
}
