using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;

namespace EasyStock.API.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IRepository<Product> _repository;

        public ProductService(IProductRepository productRepository, IRepository<Product> repository)
        {
            _productRepository = productRepository;
            _repository = repository;
        }

        public async Task<IEnumerable<ProductOverview>> GetAllAsync()
            => await _productRepository.GetAllAsync();

        public async Task<PaginationResult<ProductOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _productRepository.GetAdvancedAsync(filters, sorting, pagination); 

        public async Task<bool> IsProductBelowMinimumStock(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
                throw new Exception($"Product with id {id} not found.");
            return product.MinimumStock > product.AvailableStock;
        }

    }
}
