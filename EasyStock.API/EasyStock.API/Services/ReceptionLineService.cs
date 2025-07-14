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
        private readonly IReceptionService _receptionService;
        private readonly IRepository<StockMovement> _genericStockMovementRepository;
        private readonly IReceptionLineProcessor _receptionLineProcessor;

        public ReceptionLineService(IReceptionLineRepository receptionLineRepository, IRetryableTransactionService retryableTransactionService, IRepository<PurchaseOrderLine> genericPurchaseOrderLineRepository, IRepository<ReceptionLine> repository, IRepository<Product> genericProductRepository, IRepository<PurchaseOrder> genericPurchaseOrderRepository, IReceptionService receptionService, IRepository<StockMovement> genericStockMovementRepository, IReceptionLineProcessor receptionLineProcessor)
        {
            _receptionLineRepository = receptionLineRepository;
            _retryableTransactionService = retryableTransactionService;
            _genericPurchaseOrderLineRepository = genericPurchaseOrderLineRepository;
            _repository = repository;
            _genericProductRepository = genericProductRepository;
            _genericPurchaseOrderRepository = genericPurchaseOrderRepository;
            _receptionService = receptionService;
            _genericStockMovementRepository = genericStockMovementRepository;
            _receptionLineProcessor = receptionLineProcessor;
        }

        public async Task<IEnumerable<ReceptionLineOverview>> GetAllAsync()
            => await _receptionLineRepository.GetAllAsync();

        public async Task<PaginationResult<ReceptionLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _receptionLineRepository.GetAdvancedAsync(filters, sorting, pagination);



        public async Task AddAsync(ReceptionLine entity, string userName)
        {
            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                await _receptionLineProcessor.AddAsync(entity, userName, _receptionService.GetNextLineNumberAsync);
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

                    await _receptionLineProcessor.SetPOStatusFields(entity.Quantity, entity.PurchaseOrderLineId, userName);

                    var product = await _genericProductRepository.GetByIdAsync(entity.ProductId);
                    if (product == null)
                        throw new InvalidOperationException($"Unable to update product with ID {entity.ProductId}");
                    product.LcDate = DateTime.UtcNow;
                    product.LcUserId = userName;
                    product.InboundStock -= difference;
                    product.TotalStock += difference;

                    var stockMovement = new StockMovement
                    {
                        ProductId = product.Id,
                        Product = product,
                        QuantityChange = difference,
                        Reason = "Reception line update",
                        CrDate = DateTime.UtcNow,
                        LcDate = DateTime.UtcNow,
                        CrUserId = userName,
                        LcUserId = userName,
                        PurchaseOrderId = entity.PurchaseOrderLine.PurchaseOrderId
                    };

                    await _genericStockMovementRepository.AddAsync(stockMovement);

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

        public async Task DeleteAsync(int id, string userName)
        {
            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                await _receptionLineProcessor.DeleteAsync(id, userName);
            });
        }

        public async Task BlockAsync(int id, string userName)
        {
            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                await _receptionLineProcessor.BlockAsync(id, userName);
            });
        }

        public async Task UnblockAsync(int id, string userName)
        {
            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                await _receptionLineProcessor.UnblockAsync(id, userName);
            });
        }
    }
}
