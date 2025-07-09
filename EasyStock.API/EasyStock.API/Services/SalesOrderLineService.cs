using EasyStock.API.Common;
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

        public SalesOrderLineService(ISalesOrderLineRepository SalesOrderLineRepository, IRetryableTransactionService retryableTransactionService, IRepository<SalesOrderLine> repository, IRepository<Product> genericProductRepository)
        {
            _SalesOrderLineRepository = SalesOrderLineRepository;
            _retryableTransactionService = retryableTransactionService;
            _repository = repository;
            _genericProductRepository = genericProductRepository;
        }

        public async Task<IEnumerable<SalesOrderLineOverview>> GetAllAsync()
            => await _SalesOrderLineRepository.GetAllAsync();

        public async Task<PaginationResult<SalesOrderLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _SalesOrderLineRepository.GetAdvancedAsync(filters, sorting, pagination);

        public async Task AddAsync(SalesOrderLine entity, string userName)
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
                if (product == null)
                    throw new InvalidOperationException($"Product with ID {entity.ProductId} not found when updating reserved stock.");
                
                if (entity.Quantity > product.AvailableStock)
                {
                    product.ReservedStock = product.AvailableStock;
                    product.BackOrderedStock = entity.Quantity - product.AvailableStock;
                    product.AvailableStock = 0;
                }
                else
                {
                    product.ReservedStock += entity.Quantity;
                    product.AvailableStock -= entity.Quantity;
                }

                product.LcUserId = userName;
                product.LcDate = DateTime.UtcNow;
            });
        }

        public async Task UpdateAsync(SalesOrderLine entity, string userName)
        {
            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                entity.LcDate = DateTime.UtcNow;
                entity.LcUserId = userName;
                await _repository.AddAsync(entity);

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
            if (record == null)
                throw new InvalidOperationException($"Unable to delete record with ID {id}");
            if (record.Status == OrderStatus.Open || record.Status == OrderStatus.Partial)
            {
                var product = await _genericProductRepository.GetByIdAsync(record.ProductId);
                if (product == null)
                    throw new InvalidOperationException($"Product with ID {record.ProductId} not found when updating reserved stock.");

                if (product.BackOrderedStock > 0)
                {
                    var tmpBackOrderedStock = product.BackOrderedStock - record.Quantity;
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
                    product.ReservedStock -= record.Quantity;
                    product.AvailableStock += record.Quantity;
                }

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
            if (record == null)
                throw new InvalidOperationException($"Unable to block record with ID {id}");
            record.BlDate = DateTime.UtcNow;
            record.BlUserId = userName;

            if (record.Status == OrderStatus.Open || record.Status == OrderStatus.Partial)
            {
                var product = await _genericProductRepository.GetByIdAsync(record.ProductId);
                if (product == null)
                    throw new InvalidOperationException($"Product with ID {record.ProductId} not found when updating reserved stock.");

                if (product.BackOrderedStock > 0)
                {
                    var tmpBackOrderedStock = product.BackOrderedStock - record.Quantity;
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
                    product.ReservedStock -= record.Quantity;
                    product.AvailableStock += record.Quantity;
                }

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
            if (record == null)
                throw new InvalidOperationException($"Unable to unblock record with ID {id}");
            record.BlDate = null;
            record.BlUserId = null;
            record.LcDate = DateTime.UtcNow;
            record.LcUserId = userName;

            if (record.Status == OrderStatus.Open || record.Status == OrderStatus.Partial)
            {
                var product = await _genericProductRepository.GetByIdAsync(record.ProductId);
                if (product == null)
                    throw new InvalidOperationException($"Product with ID {record.ProductId} not found when updating reserved stock.");

                if (record.Quantity > product.AvailableStock)
                {
                    product.ReservedStock = product.AvailableStock;
                    product.BackOrderedStock = record.Quantity - product.AvailableStock;
                    product.AvailableStock = 0;
                }
                else
                {
                    product.ReservedStock += record.Quantity;
                    product.AvailableStock -= record.Quantity;
                }

                product.LcUserId = userName;
                product.LcDate = DateTime.UtcNow;
            }

            await _repository.UpdateAsync(record);
        }
    }
}
