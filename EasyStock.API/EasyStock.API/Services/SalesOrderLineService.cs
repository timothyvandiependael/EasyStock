using EasyStock.API.Common;
using EasyStock.API.Dtos;
using EasyStock.API.Models;
using EasyStock.API.Repositories;

namespace EasyStock.API.Services
{
    public class SalesOrderLineService : ISalesOrderLineService
    {
        private readonly ISalesOrderLineRepository _SalesOrderLineRepository;
        private readonly IRetryableTransactionService _retryableTransactionService;
        private readonly IRepository<SalesOrderLine> _repository;
        private readonly IRepository<Product> _genericProductRepository;
        private readonly ISalesOrderService _salesOrderService;
        private readonly ISalesOrderLineProcessor _salesOrderLineProcessor;
        private readonly IUpdateService<SalesOrderLine> _updateService;

        public SalesOrderLineService(ISalesOrderLineRepository SalesOrderLineRepository, IRetryableTransactionService retryableTransactionService, IRepository<SalesOrderLine> repository, IRepository<Product> genericProductRepository, ISalesOrderService salesOrderService, ISalesOrderLineProcessor salesOrderLineProcessor, IUpdateService<SalesOrderLine> updateService)
        {
            _SalesOrderLineRepository = SalesOrderLineRepository;
            _retryableTransactionService = retryableTransactionService;
            _repository = repository;
            _genericProductRepository = genericProductRepository;
            _salesOrderService = salesOrderService;
            _salesOrderLineProcessor = salesOrderLineProcessor;
            _updateService = updateService;
        }

        public async Task<IEnumerable<SalesOrderLineOverview>> GetAllAsync()
            => await _SalesOrderLineRepository.GetAllAsync();

        public async Task<PaginationResult<SalesOrderLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination)
            => await _SalesOrderLineRepository.GetAdvancedAsync(filters, sorting, pagination);

        public async Task<AutoRestockDto> AddAsync(SalesOrderLine entity, string userName)
        {
            var dto = new AutoRestockDto();

            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                dto = await _salesOrderLineProcessor.AddAsync(entity, userName, _salesOrderService.GetNextLineNumberAsync);
            });

            return dto;
        }

        public async Task UpdateAsync(SalesOrderLine entity, string userName, bool useTransaction = true)
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

        private async Task UpdateAsyncInternal(SalesOrderLine entity, string userName)
        {
            entity.LcDate = DateTime.UtcNow;
            entity.LcUserId = userName;

            var oldRecord = await _repository.GetByIdAsync(entity.Id);
            if (oldRecord == null)
                throw new InvalidOperationException($"Sales order line with ID {entity.Id} not found when trying to update it.");

            if (oldRecord.Quantity != entity.Quantity)
            {
                if (entity.Status == OrderStatus.Open || entity.Status == OrderStatus.Partial)
                {
                    var difference = entity.Quantity - oldRecord.Quantity;

                    var product = await _genericProductRepository.GetByIdAsync(entity.ProductId);
                    if (product == null)
                        throw new InvalidOperationException($"Product with ID {entity.ProductId} not found when updating reserved stock.");

                    if (difference > 0)
                    {
                        if (difference > product.AvailableStock)
                        {
                            product.ReservedStock += product.AvailableStock;
                            product.BackOrderedStock += difference - product.AvailableStock;
                            product.AvailableStock = 0;
                        }
                        else
                        {
                            product.ReservedStock += difference;
                            product.AvailableStock -= difference;
                        }
                    }
                    else
                    {
                        difference = Math.Abs(difference);

                        if (product.BackOrderedStock > 0)
                        {
                            var tmpBackOrderedStock = product.BackOrderedStock - difference;
                            if (tmpBackOrderedStock >= 0)
                            {
                                product.BackOrderedStock = tmpBackOrderedStock;
                            }
                            else
                            {
                                product.BackOrderedStock = 0;
                                tmpBackOrderedStock = Math.Abs(tmpBackOrderedStock);
                                product.ReservedStock -= tmpBackOrderedStock;
                                product.AvailableStock += tmpBackOrderedStock;
                            }
                        }
                        else
                        {
                            product.ReservedStock -= difference;
                            product.AvailableStock += difference;
                        }
                    }

                    product.LcUserId = userName;
                    product.LcDate = DateTime.UtcNow;
                }
            }

            var record = _updateService.MapAndUpdateAuditFields(oldRecord, entity, userName);
            await _repository.UpdateAsync(record);
        }

        public async Task DeleteAsync(int id, string userName)
        {
            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                await _salesOrderLineProcessor.DeleteAsync(id, userName);
            });
        }

        public async Task BlockAsync(int id, string userName)
        {

            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                await _salesOrderLineProcessor.BlockAsync(id, userName);
            });

        }

        public async Task UnblockAsync(int id, string userName)
        {
            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                await _salesOrderLineProcessor.UnblockAsync(id, userName);
            });

        }
    }
}
