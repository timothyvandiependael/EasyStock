using EasyStock.API.Common;
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

        public DispatchLineService(IDispatchLineRepository dispatchLineRepository, IRetryableTransactionService retryableTransactionService, IRepository<DispatchLine> repository, IRepository<Product> genericProductRepository, IRepository<SalesOrder> genericSalesOrderRepository, IRepository<SalesOrderLine> genericSalesOrderLineRepository, IDispatchService dispatchService, IRepository<StockMovement> genericStockMovementRepository)
        {
            _dispatchLineRepository = dispatchLineRepository;
            _retryableTransactionService = retryableTransactionService;
            _repository = repository;
            _genericProductRepository = genericProductRepository;
            _genericSalesOrderRepository = genericSalesOrderRepository;
            _genericSalesOrderLineRepository = genericSalesOrderLineRepository;
            _dispatchService = dispatchService;
            _genericStockMovementRepository = genericStockMovementRepository;
        }

        public async Task<IEnumerable<DispatchLineOverview>> GetAllAsync()
            => await _dispatchLineRepository.GetAllAsync();

        public async Task<PaginationResult<DispatchLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _dispatchLineRepository.GetAdvancedAsync(filters, sorting, pagination);

        private async Task SetSOStatusFields(int quantity, int salesOrderLineId, string userName)
        {
            var soLine = await _genericSalesOrderLineRepository.GetByIdAsync(salesOrderLineId);
            if (soLine == null)
                throw new InvalidOperationException($"Unable to find sales order line with ID {salesOrderLineId}");
            soLine.LcDate = DateTime.UtcNow;
            soLine.LcUserId = userName;
            if (quantity < soLine.Quantity)
            {
                soLine.Status = OrderStatus.Partial;
            }

            else
            {
                soLine.Status = OrderStatus.Complete;
            }

            var po = await _genericSalesOrderRepository.GetByIdAsync(soLine.SalesOrderId);
            if (po == null)
                throw new InvalidOperationException($"Unable to find sales order with ID {soLine.SalesOrderId}");
            po.LcDate = DateTime.UtcNow;
            po.LcUserId = userName;
            if (po.Lines.Any(l => l.Status == OrderStatus.Partial || l.Status == OrderStatus.Open))
            {
                po.Status = OrderStatus.Partial;
            }
            else
            {
                po.Status = OrderStatus.Complete;
            }
        }

        public async Task AddAsync(DispatchLine entity, string userName, bool fromParent = false)
        {
            if (!fromParent)
            {
                await _retryableTransactionService.ExecuteAsync(async () =>
                {
                    await AddInternalAsync(entity, userName, fromParent);
                });
            }
            else
            {
                await AddInternalAsync(entity, userName, fromParent);
            }
            
        }

        private async Task AddInternalAsync(DispatchLine entity, string userName, bool fromParent)
        {
            entity.CrDate = DateTime.UtcNow;
            entity.LcDate = entity.CrDate;
            entity.CrUserId = userName;
            entity.LcUserId = userName;
            if (!fromParent)
                entity.LineNumber = await _dispatchService.GetNextLineNumberAsync(entity.DispatchId);

            await SetSOStatusFields(entity.Quantity, entity.SalesOrderLineId, userName);

            var product = await _genericProductRepository.GetByIdAsync(entity.ProductId);
            if (product == null)
                throw new InvalidOperationException($"Unable to find product with ID {entity.ProductId}");
            product.LcDate = DateTime.UtcNow;
            product.LcUserId = userName;
            product.ReservedStock -= entity.Quantity;
            product.TotalStock -= entity.Quantity;

            await _repository.AddAsync(entity);

            var stockMovement = new StockMovement
            {
                ProductId = product.Id,
                Product = product,
                QuantityChange = 0 - entity.Quantity,
                Reason = "Dispatch",
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = userName,
                LcUserId = userName,
                SalesOrderId = entity.SalesOrderLine.SalesOrderId
            };

            await _genericStockMovementRepository.AddAsync(stockMovement);
        }

        public async Task UpdateAsync(DispatchLine entity, string userName)
        {
            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                entity.LcDate = entity.CrDate;
                entity.LcUserId = userName;

                var ogRecord = await _repository.GetByIdAsync(entity.Id);
                if (ogRecord == null)
                    throw new InvalidOperationException($"Unable to find dispatch line with ID {entity.Id}");
                if (ogRecord.Quantity != entity.Quantity)
                {
                    var difference = entity.Quantity - ogRecord.Quantity;

                    await SetSOStatusFields(entity.Quantity, entity.SalesOrderLineId, userName);

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

                await _repository.UpdateAsync(entity);
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
            var dispatchLine = await _repository.GetByIdAsync(id);
            if (dispatchLine == null)
                throw new InvalidOperationException($"Unable to find record with ID {id}");

            await SetSOStatusFields(0, dispatchLine.SalesOrderLineId, userName);

            var product = await _genericProductRepository.GetByIdAsync(dispatchLine.ProductId);
            if (product == null)
                throw new InvalidOperationException($"Unable to find product with ID {dispatchLine.ProductId}");
            product.LcDate = DateTime.UtcNow;
            product.LcUserId = userName;
            product.ReservedStock += dispatchLine.Quantity;
            product.TotalStock += dispatchLine.Quantity;

            await _repository.DeleteAsync(id);

            var stockMovement = new StockMovement
            {
                ProductId = product.Id,
                Product = product,
                QuantityChange = dispatchLine.Quantity,
                Reason = "Deletion of dispatch line",
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = userName,
                LcUserId = userName,
                SalesOrderId = dispatchLine.SalesOrderLine.SalesOrderId
            };

            await _genericStockMovementRepository.AddAsync(stockMovement);
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
            var dispatchLine = await _repository.GetByIdAsync(id);
            if (dispatchLine == null)
                throw new InvalidOperationException($"Unable to block record with ID {id}");
            dispatchLine.BlDate = DateTime.UtcNow;
            dispatchLine.BlUserId = userName;

            await SetSOStatusFields(0, dispatchLine.SalesOrderLineId, userName);

            var product = await _genericProductRepository.GetByIdAsync(dispatchLine.ProductId);
            if (product == null)
                throw new InvalidOperationException($"Unable to find product with ID {dispatchLine.ProductId}");
            product.LcDate = DateTime.UtcNow;
            product.LcUserId = userName;
            product.ReservedStock += dispatchLine.Quantity;
            product.TotalStock += dispatchLine.Quantity;

            await _repository.UpdateAsync(dispatchLine);

            var stockMovement = new StockMovement
            {
                ProductId = product.Id,
                Product = product,
                QuantityChange = dispatchLine.Quantity,
                Reason = "Blocking of dispatch line",
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = userName,
                LcUserId = userName,
                SalesOrderId = dispatchLine.SalesOrderLine.SalesOrderId
            };

            await _genericStockMovementRepository.AddAsync(stockMovement);
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
            var dispatchLine = await _repository.GetByIdAsync(id);
            if (dispatchLine == null)
                throw new InvalidOperationException($"Unable to unblock record with ID {id}");
            dispatchLine.BlDate = null;
            dispatchLine.BlUserId = null;
            dispatchLine.LcDate = DateTime.UtcNow;
            dispatchLine.LcUserId = userName;

            await SetSOStatusFields(dispatchLine.Quantity, dispatchLine.SalesOrderLineId, userName);

            var product = await _genericProductRepository.GetByIdAsync(dispatchLine.ProductId);
            if (product == null)
                throw new InvalidOperationException($"Unable to find product with ID {dispatchLine.ProductId}");
            product.LcDate = DateTime.UtcNow;
            product.LcUserId = userName;
            product.ReservedStock -= dispatchLine.Quantity;
            product.TotalStock -= dispatchLine.Quantity;

            await _repository.UpdateAsync(dispatchLine);

            var stockMovement = new StockMovement
            {
                ProductId = product.Id,
                Product = product,
                QuantityChange = 0 - dispatchLine.Quantity,
                Reason = "Unblocking of dispatch line",
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = userName,
                LcUserId = userName,
                SalesOrderId = dispatchLine.SalesOrderLine.SalesOrderId
            };

            await _genericStockMovementRepository.AddAsync(stockMovement);
        }
    }
}
