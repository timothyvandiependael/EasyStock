using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;

namespace EasyStock.API.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<ProductOverview>> GetAllAsync()
            => await _productRepository.GetAllAsync();

        public async Task<PaginationResult<ProductOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _productRepository.GetAdvancedAsync(filters, sorting, pagination); 

    }
}
