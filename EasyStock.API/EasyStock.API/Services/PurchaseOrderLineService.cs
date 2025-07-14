using EasyStock.API.Common;
using EasyStock.API.Data;
using EasyStock.API.Models;
using EasyStock.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EasyStock.API.Services
{
    public class PurchaseOrderLineService : IPurchaseOrderLineService
    {
        private readonly IPurchaseOrderLineRepository _purchaseOrderLineRepository;
        private readonly IRepository<PurchaseOrderLine> _repository;
        private readonly IRepository<Product> _genericProductRepository;
        private readonly IRetryableTransactionService _retryableTransactionService;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IPurchaseOrderLineProcessor _purchaseOrderLineProcessor;

        public PurchaseOrderLineService(IPurchaseOrderLineRepository purchaseOrderLineRepository, IRepository<PurchaseOrderLine> repository, IRetryableTransactionService retryableTransactionService, IRepository<Product> genericProductRepository, IPurchaseOrderService purchaseOrderService, IPurchaseOrderLineProcessor purchaseOrderLineProcessor)
        {
            _purchaseOrderLineRepository = purchaseOrderLineRepository;
            _repository = repository;
            _retryableTransactionService = retryableTransactionService;
            _genericProductRepository = genericProductRepository;
            _purchaseOrderService = purchaseOrderService;
            _purchaseOrderLineProcessor = purchaseOrderLineProcessor;
        }

        public async Task<IEnumerable<PurchaseOrderLineOverview>> GetAllAsync()
            => await _purchaseOrderLineRepository.GetAllAsync();

        public async Task<PaginationResult<PurchaseOrderLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _purchaseOrderLineRepository.GetAdvancedAsync(filters, sorting, pagination);

        public async Task AddAsync(PurchaseOrderLine entity, string userName)
        {
            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                await _purchaseOrderLineProcessor.AddAsync(entity, userName, _purchaseOrderService.GetNextLineNumberAsync);
            });
        }

        public async Task UpdateAsync(PurchaseOrderLine entity, string userName)
        {
            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                entity.LcDate = DateTime.UtcNow;
                entity.LcUserId = userName;
                await _repository.AddAsync(entity);

                var oldRecord = await _repository.GetByIdAsync(entity.Id);
                if (oldRecord == null)
                    throw new InvalidOperationException($"Purchase order line with ID {entity.Id} not found when trying to update it.");

                if (oldRecord.Quantity != entity.Quantity)
                {
                    if (entity.Status == OrderStatus.Open || entity.Status == OrderStatus.Partial)
                    {
                        var difference = entity.Quantity - oldRecord.Quantity;

                        var product = await _genericProductRepository.GetByIdAsync(entity.ProductId);
                        if (product == null)
                            throw new InvalidOperationException($"Product with ID {entity.ProductId} not found when updating inbound stock.");
                        product.InboundStock += difference;
                        product.LcUserId = userName;
                        product.LcDate = DateTime.UtcNow;
                    }
                }
            });
        }

        public async Task DeleteAsync(int id, string userName)
        {

            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                await _purchaseOrderLineProcessor.DeleteAsync(id, userName);
            });

        }

        public async Task BlockAsync(int id, string userName)
        {

            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                await _purchaseOrderLineProcessor.BlockAsync(id, userName);
            });

        }

        public async Task UnblockAsync(int id, string userName)
        {
            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                await _purchaseOrderLineProcessor.UnblockAsync(id, userName);
            });

        }

        
    }
}
