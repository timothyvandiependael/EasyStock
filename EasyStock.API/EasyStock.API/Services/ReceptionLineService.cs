using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;

namespace EasyStock.API.Services
{
    public class ReceptionLineService : IReceptionLineService
    {
        private readonly IReceptionLineRepository _receptionLineRepository;
        private readonly IRetryableTransactionService _retryableTransactionService;
        private readonly IRepository<PurchaseOrderLine> _genericPurchaseOrderLineRepository;
        private readonly IRepository<ReceptionLine> _repository;
        private readonly IRepository<Product> _genericProductRepository;
        private readonly IRepository<PurchaseOrder> _genericPurchaseOrderRepository;

        public ReceptionLineService(IReceptionLineRepository receptionLineRepository, IRetryableTransactionService retryableTransactionService, IRepository<PurchaseOrderLine> genericPurchaseOrderLineRepository, IRepository<ReceptionLine> repository, IRepository<Product> genericProductRepository, IRepository<PurchaseOrder> genericPurchaseOrderRepository)
        {
            _receptionLineRepository = receptionLineRepository;
            _retryableTransactionService = retryableTransactionService;
            _genericPurchaseOrderLineRepository = genericPurchaseOrderLineRepository;
            _repository = repository;
            _genericProductRepository = genericProductRepository;
            _genericPurchaseOrderRepository = genericPurchaseOrderRepository;
        }

        public async Task<IEnumerable<ReceptionLineOverview>> GetAllAsync()
            => await _receptionLineRepository.GetAllAsync();

        public async Task<PaginationResult<ReceptionLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _receptionLineRepository.GetAdvancedAsync(filters, sorting, pagination);

        private async Task SetPOStatusFields(int quantity, int purchaseOrderLineId, string userName)
        {
            var poLine = await _genericPurchaseOrderLineRepository.GetByIdAsync(purchaseOrderLineId);
            if (poLine == null)
                throw new InvalidOperationException($"Unable to find purchase order line with ID {purchaseOrderLineId}");
            poLine.LcDate = DateTime.UtcNow;
            poLine.LcUserId = userName;
            if (quantity < poLine.Quantity)
            {
                poLine.Status = OrderStatus.Partial;
            }

            else
            {
                poLine.Status = OrderStatus.Complete;
            }

            var po = await _genericPurchaseOrderRepository.GetByIdAsync(poLine.PurchaseOrderId);
            if (po == null)
                throw new InvalidOperationException($"Unable to find purchase order with ID {poLine.PurchaseOrderId}");
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

        public async Task AddAsync(ReceptionLine entity, string userName)
        {
            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                entity.CrDate = DateTime.UtcNow;
                entity.LcDate = entity.CrDate;
                entity.CrUserId = userName;
                entity.LcUserId = userName;

                await SetPOStatusFields(entity.Quantity, entity.PurchaseOrderLineId, userName);

                var product = await _genericProductRepository.GetByIdAsync(entity.ProductId);
                if (product == null)
                    throw new InvalidOperationException($"Unable to find product with ID {entity.ProductId}");
                product.LcDate = DateTime.UtcNow;
                product.LcUserId = userName;
                product.InboundStock -= entity.Quantity;
                product.TotalStock += entity.Quantity;

                product.AvailableStock +=
                    product.BackOrderedStock > entity.Quantity
                    ? 0
                    : (entity.Quantity - product.BackOrderedStock);

                product.ReservedStock +=
                    product.BackOrderedStock > entity.Quantity
                    ? entity.Quantity
                    : product.BackOrderedStock;

                if (product.BackOrderedStock > 0)
                {
                    product.BackOrderedStock -=
                        product.BackOrderedStock > entity.Quantity
                        ? entity.Quantity
                        : product.BackOrderedStock;
                }

                await _repository.AddAsync(entity);
            });
        }

        public async Task UpdateAsync(ReceptionLine entity, string userName)
        {
            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                entity.LcDate = entity.CrDate;
                entity.LcUserId = userName;

                var ogRecord = await _repository.GetByIdAsync(entity.Id);
                if (ogRecord == null)
                    throw new InvalidOperationException($"Unable to find reception line with ID {entity.Id}");
                if (ogRecord.Quantity != entity.Quantity)
                {
                    var difference = entity.Quantity - ogRecord.Quantity;

                    await SetPOStatusFields(entity.Quantity, entity.PurchaseOrderLineId, userName);

                    var product = await _genericProductRepository.GetByIdAsync(entity.ProductId);
                    if (product == null)
                        throw new InvalidOperationException($"Unable to update product with ID {entity.ProductId}");
                    product.LcDate = DateTime.UtcNow;
                    product.LcUserId = userName;
                    product.InboundStock -= difference;
                    product.TotalStock += difference;

                    if (difference > 0)
                    {
                        product.AvailableStock +=
                        product.BackOrderedStock > difference
                        ? 0
                        : (difference - product.BackOrderedStock);

                        product.ReservedStock +=
                            product.BackOrderedStock > difference
                            ? difference
                            : product.BackOrderedStock;

                        if (product.BackOrderedStock > 0)
                        {
                            product.BackOrderedStock -=
                                product.BackOrderedStock > difference
                                ? difference
                                : product.BackOrderedStock;
                        }
                    }
                    else
                    {
                        difference = Math.Abs(difference);
                        var tmpAvailableStock = product.AvailableStock - difference;
                        if (tmpAvailableStock < 0) // Meaning it came originally from backorder
                        {
                            var stockShortage = Math.Abs(tmpAvailableStock);
                            product.AvailableStock = 0;
                            product.ReservedStock -= stockShortage;
                            product.BackOrderedStock += stockShortage;
                        }
                        else
                        {
                            product.AvailableStock -= difference;
                        }
                    }


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
            var receptionLine = await _repository.GetByIdAsync(id);
            if (receptionLine == null)
                throw new InvalidOperationException($"Unable to find record with ID {id}");

            await SetPOStatusFields(0, receptionLine.PurchaseOrderLineId, userName);

            var product = await _genericProductRepository.GetByIdAsync(receptionLine.ProductId);
            if (product == null)
                throw new InvalidOperationException($"Unable to find product with ID {receptionLine.ProductId}");
            product.LcDate = DateTime.UtcNow;
            product.LcUserId = userName;
            product.InboundStock += receptionLine.Quantity;
            product.TotalStock -= receptionLine.Quantity;

            var tmpAvailableStock = product.AvailableStock - receptionLine.Quantity;
            if (tmpAvailableStock < 0) // Meaning it came originally from backorder
            {
                var stockShortage = Math.Abs(tmpAvailableStock);
                product.AvailableStock = 0;
                product.ReservedStock -= stockShortage;
                product.BackOrderedStock += stockShortage;
            }
            else
            {
                product.AvailableStock -= receptionLine.Quantity;
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
            var receptionLine = await _repository.GetByIdAsync(id);
            if (receptionLine == null)
                throw new InvalidOperationException($"Unable to block record with ID {id}");
            receptionLine.BlDate = DateTime.UtcNow;
            receptionLine.BlUserId = userName;

            await SetPOStatusFields(0, receptionLine.PurchaseOrderLineId, userName);

            var product = await _genericProductRepository.GetByIdAsync(receptionLine.ProductId);
            if (product == null)
                throw new InvalidOperationException($"Unable to find product with ID {receptionLine.ProductId}");
            product.LcDate = DateTime.UtcNow;
            product.LcUserId = userName;
            product.InboundStock += receptionLine.Quantity;
            product.TotalStock -= receptionLine.Quantity;

            var tmpAvailableStock = product.AvailableStock - receptionLine.Quantity;
            if (tmpAvailableStock < 0) // Meaning it came originally from backorder
            {
                var stockShortage = Math.Abs(tmpAvailableStock);
                product.AvailableStock = 0;
                product.ReservedStock -= stockShortage;
                product.BackOrderedStock += stockShortage;
            }
            else
            {
                product.AvailableStock -= receptionLine.Quantity;
            }

            await _repository.UpdateAsync(receptionLine);
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
            var receptionLine = await _repository.GetByIdAsync(id);
            if (receptionLine == null)
                throw new InvalidOperationException($"Unable to unblock record with ID {id}");
            receptionLine.BlDate = null;
            receptionLine.BlUserId = null;
            receptionLine.LcDate = DateTime.UtcNow;
            receptionLine.LcUserId = userName;

            await SetPOStatusFields(receptionLine.Quantity, receptionLine.PurchaseOrderLineId, userName);

            var product = await _genericProductRepository.GetByIdAsync(receptionLine.ProductId);
            if (product == null)
                throw new InvalidOperationException($"Unable to find product with ID {receptionLine.ProductId}");
            product.LcDate = DateTime.UtcNow;
            product.LcUserId = userName;
            product.InboundStock -= receptionLine.Quantity;
            product.TotalStock += receptionLine.Quantity;

            product.AvailableStock +=
                    product.BackOrderedStock > receptionLine.Quantity
                    ? 0
                    : (receptionLine.Quantity - product.BackOrderedStock);

            product.ReservedStock +=
                product.BackOrderedStock > receptionLine.Quantity
                ? receptionLine.Quantity
                : product.BackOrderedStock;

            if (product.BackOrderedStock > 0)
            {
                product.BackOrderedStock -=
                    product.BackOrderedStock > receptionLine.Quantity
                    ? receptionLine.Quantity
                    : product.BackOrderedStock;
            }

            await _repository.UpdateAsync(receptionLine);
        }
    }
}
