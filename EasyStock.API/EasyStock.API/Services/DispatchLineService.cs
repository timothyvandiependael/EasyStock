﻿using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;
using System.Runtime.CompilerServices;

namespace EasyStock.API.Services
{
    public class DispatchLineService : IDispatchLineService
    {
        private readonly IDispatchLineRepository _dispatchLineRepository;
        private readonly IRetryableTransactionService _retryableTransactionService;
        private readonly IRepository<DispatchLine> _repository;
        private readonly IRepository<Product> _genericProductRepository;
        private readonly IRepository<SalesOrderLine> _genericSalesOrderLineRepository;
        private readonly IRepository<SalesOrder> _genericSalesOrderRepository;
        private readonly IDispatchService _dispatchService;
        private readonly IRepository<StockMovement> _genericStockMovementRepository;
        private readonly IDispatchLineProcessor _dispatchLineProcessor;
        private readonly IUpdateService<DispatchLine> _updateService;

        public DispatchLineService(IDispatchLineRepository dispatchLineRepository, IRetryableTransactionService retryableTransactionService, IRepository<DispatchLine> repository, IRepository<Product> genericProductRepository, IRepository<SalesOrder> genericSalesOrderRepository, IRepository<SalesOrderLine> genericSalesOrderLineRepository, IDispatchService dispatchService, IRepository<StockMovement> genericStockMovementRepository, IDispatchLineProcessor dispatchLineProcessor, IUpdateService<DispatchLine> updateService)
        {
            _dispatchLineRepository = dispatchLineRepository;
            _retryableTransactionService = retryableTransactionService;
            _repository = repository;
            _genericProductRepository = genericProductRepository;
            _genericSalesOrderRepository = genericSalesOrderRepository;
            _genericSalesOrderLineRepository = genericSalesOrderLineRepository;
            _dispatchService = dispatchService;
            _genericStockMovementRepository = genericStockMovementRepository;
            _dispatchLineProcessor = dispatchLineProcessor;
            _updateService = updateService;
        }

        public async Task<IEnumerable<DispatchLineOverview>> GetAllAsync()
            => await _dispatchLineRepository.GetAllAsync();

        public async Task<PaginationResult<DispatchLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination)
            => await _dispatchLineRepository.GetAdvancedAsync(filters, sorting, pagination);

        public async Task AddAsync(DispatchLine entity, string userName)
        {
            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                await _dispatchLineProcessor.AddAsync(entity, userName, _dispatchService.GetNextLineNumberAsync);
            });
        }

        public async Task UpdateAsync(DispatchLine entity, string userName, bool useTransaction = true)
        {
            if (useTransaction)
            {
                await _retryableTransactionService.ExecuteAsync(async () =>
                {
                    await UpdateAsyncInternal(entity, userName);
                });
            }
            else
            {
                await UpdateAsyncInternal(entity, userName);
            }
            
        }

        private async Task UpdateAsyncInternal(DispatchLine entity, string userName)
        {
            entity.LcDate = DateTime.UtcNow;
            entity.LcUserId = userName;

            var ogRecord = await _repository.GetByIdAsync(entity.Id);
            if (ogRecord == null)
                throw new InvalidOperationException($"Unable to find dispatch line with ID {entity.Id}");
            if (ogRecord.Quantity != entity.Quantity)
            {
                var difference = entity.Quantity - ogRecord.Quantity;

                await _dispatchLineProcessor.SetSOStatusFields(entity.Quantity, entity.SalesOrderLineId, userName);

                var product = await _genericProductRepository.GetByIdAsync(entity.ProductId);
                if (product == null)
                    throw new InvalidOperationException($"Unable to update product with ID {entity.ProductId}");
                product.LcDate = DateTime.UtcNow;
                product.LcUserId = userName;
                product.ReservedStock -= difference;
                product.TotalStock -= difference;

                var stockMovement = new StockMovement
                {
                    ProductId = product.Id,
                    Product = product,
                    QuantityChange = 0 - difference,
                    Reason = "Correction of dispatch line",
                    CrDate = DateTime.UtcNow,
                    LcDate = DateTime.UtcNow,
                    CrUserId = userName,
                    LcUserId = userName,
                    SalesOrderId = entity.SalesOrderLine.SalesOrderId
                };

                await _genericStockMovementRepository.AddAsync(stockMovement);
            }

            var record = _updateService.MapAndUpdateAuditFields(ogRecord, entity, userName);
            await _repository.UpdateAsync(record);
        }

        public async Task DeleteAsync(int id, string userName)
        {
            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                await _dispatchLineProcessor.DeleteAsync(id, userName);
            });
        }

        public async Task BlockAsync(int id, string userName)
        {
            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                await _dispatchLineProcessor.BlockAsync(id, userName);
            });
        }

        public async Task UnblockAsync(int id, string userName)
        {
            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                await _dispatchLineProcessor.UnblockAsync(id, userName);
            });

        }
    }
}
