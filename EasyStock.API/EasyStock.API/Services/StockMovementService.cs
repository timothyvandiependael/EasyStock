using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;

namespace EasyStock.API.Services
{
    public class StockMovementService : IStockMovementService
    {
        private readonly IStockMovementRepository _stockMovementRepository;

        public StockMovementService(IStockMovementRepository stockMovementRepository)
        {
            _stockMovementRepository = stockMovementRepository;
        }

        public async Task<IEnumerable<StockMovementOverview>> GetAllAsync()
            => await _stockMovementRepository.GetAllAsync();

        public async Task<PaginationResult<StockMovementOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _stockMovementRepository.GetAdvancedAsync(filters, sorting, pagination);
    }
}
