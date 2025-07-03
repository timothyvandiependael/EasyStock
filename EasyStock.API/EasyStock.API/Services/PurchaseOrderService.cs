using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;

namespace EasyStock.API.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;

        public PurchaseOrderService(IPurchaseOrderRepository purchaseOrderRepository)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
        }

        public async Task<IEnumerable<PurchaseOrderOverview>> GetAllAsync()
            => await _purchaseOrderRepository.GetAllAsync();

        public async Task<PaginationResult<PurchaseOrderOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _purchaseOrderRepository.GetAdvancedAsync(filters, sorting, pagination);

    }
}
