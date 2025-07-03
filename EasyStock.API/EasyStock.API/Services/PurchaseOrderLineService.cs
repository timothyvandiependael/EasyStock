using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;

namespace EasyStock.API.Services
{
    public class PurchaseOrderLineService : IPurchaseOrderLineService
    {
        private readonly IPurchaseOrderLineRepository _purchaseOrderLineRepository;

        public PurchaseOrderLineService(IPurchaseOrderLineRepository purchaseOrderLineRepository)
        {
            _purchaseOrderLineRepository = purchaseOrderLineRepository;
        }

        public async Task<IEnumerable<PurchaseOrderLineOverview>> GetAllAsync()
            => await _purchaseOrderLineRepository.GetAllAsync();

        public async Task<PaginationResult<PurchaseOrderLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _purchaseOrderLineRepository.GetAdvancedAsync(filters, sorting, pagination);

    }
}
