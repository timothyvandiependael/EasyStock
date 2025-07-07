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

        public PurchaseOrderLineService(IPurchaseOrderLineRepository purchaseOrderLineRepository, IRepository<PurchaseOrderLine> repository, IRetryableTransactionService retryableTransactionService)
        {
            _purchaseOrderLineRepository = purchaseOrderLineRepository;
            _repository = repository;
            _retryableTransactionService = retryableTransactionService;
        }

        public async Task<IEnumerable<PurchaseOrderLineOverview>> GetAllAsync()
            => await _purchaseOrderLineRepository.GetAllAsync();

        public async Task<PaginationResult<PurchaseOrderLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _purchaseOrderLineRepository.GetAdvancedAsync(filters, sorting, pagination);

        public async Task AddAsync(PurchaseOrderLine entity, string userName)
        {
            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                entity.CrDate = DateTime.UtcNow;
                entity.LcDate = entity.CrDate;
                entity.CrUserId = userName;
                entity.LcUserId = userName;
                entity.Status = OrderStatus.Open;
                await _repository.AddAsync(entity);

                var product = await _genericProductRepository.GetByIdAsync(entity.ProductId);
                product.InboundStock += entity.Quantity;
                product.LcUserId = userName;
                product.LcDate = DateTime.UtcNow;
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
                if (oldRecord.Quantity != entity.Quantity)
                {
                    if (entity.Status == OrderStatus.Open || entity.Status == OrderStatus.Partial)
                    {
                        var difference = entity.Quantity - oldRecord.Quantity;

                        var product = await _genericProductRepository.GetByIdAsync(entity.ProductId);
                        product.InboundStock += difference;
                        product.LcUserId = userName;
                        product.LcDate = DateTime.UtcNow;
                    }
                }
            });
        }

        public async Task DeleteAsync(int id, string userName, bool manageTransaction = true)
        {
            if (manageTransaction)
            {
                await _retryableTransactionService.ExecuteAsync(async () =>
                {
                    await DeleteInternalAsync(id, userName);
                });
            }
            else
            {
                await DeleteInternalAsync(id, userName);
            }
            
        }

        private async Task DeleteInternalAsync(int id, string userName)
        {
            var record = await _repository.GetByIdAsync(id);
            if (record.Status == OrderStatus.Open || record.Status == OrderStatus.Partial)
            {
                var product = await _genericProductRepository.GetByIdAsync(record.ProductId);
                product.InboundStock -= record.Quantity;
                product.LcUserId = userName;
                product.LcDate = DateTime.UtcNow;
            }

            await _repository.DeleteAsync(id);
        }

        public async Task BlockAsync(int id, string userName, bool manageTransaction = true)
        {
            if (manageTransaction)
            {
                await _retryableTransactionService.ExecuteAsync(async () =>
                {
                    await BlockInternalAsync(id, userName);
                });
            }
            else
            {
                await BlockInternalAsync(id, userName);
            }
            
        }

        private async Task BlockInternalAsync(int id, string userName)
        {
            var record = await _repository.GetByIdAsync(id);
            record.BlDate = DateTime.UtcNow;
            record.BlUserId = userName;

            if (record.Status == OrderStatus.Open || record.Status == OrderStatus.Partial)
            {
                var product = await _genericProductRepository.GetByIdAsync(record.ProductId);
                product.InboundStock -= record.Quantity;
                product.LcUserId = userName;
                product.LcDate = DateTime.UtcNow;
            }

            await _repository.UpdateAsync(record);
        }

        public async Task UnblockAsync(int id, string userName, bool manageTransaction = true)
        {
            if (manageTransaction)
            {
                await _retryableTransactionService.ExecuteAsync(async () =>
                {
                    await UnblockInternalAsync(id, userName);
                });
            }
            else
            {
                await UnblockInternalAsync(id, userName);
            }
            
        }

        private async Task UnblockInternalAsync(int id, string userName)
        {
            var record = await _repository.GetByIdAsync(id);
            record.BlDate = null;
            record.BlUserId = null;
            record.LcDate = DateTime.UtcNow;
            record.LcUserId = userName;

            if (record.Status == OrderStatus.Open || record.Status == OrderStatus.Partial)
            {
                var product = await _genericProductRepository.GetByIdAsync(record.ProductId);
                product.InboundStock += record.Quantity;
                product.LcUserId = userName;
                product.LcDate = DateTime.UtcNow;
            }

            await _repository.UpdateAsync(record);
        }
    }
}
