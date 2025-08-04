using EasyStock.API.Common;
using EasyStock.API.Dtos;
using EasyStock.API.Models;
using EasyStock.API.Repositories;

namespace EasyStock.API.Services
{
    public class SalesOrderLineProcessor : ISalesOrderLineProcessor
    {
        private readonly IRepository<Product> _genericProductRepository;
        private readonly IRepository<SalesOrderLine> _repository;
        private readonly IProductService _productService;
        private readonly IPurchaseOrderService _purchaseOrderService;
        public SalesOrderLineProcessor(IRepository<Product> genericProductRepository, IRepository<SalesOrderLine> repository, IProductService productService, IPurchaseOrderService purchaseOrderService) 
        { 
            _genericProductRepository = genericProductRepository;
            _repository = repository;
            _productService = productService;
            _purchaseOrderService = purchaseOrderService;
        }

        public async Task<AutoRestockDto> AddAsync(SalesOrderLine entity, string userName, Func<int, Task<int>>? getNextLineNumberAsync, bool fromParent = false)
        {
            entity.CrDate = DateTime.UtcNow;
            entity.LcDate = entity.CrDate;
            entity.CrUserId = userName;
            entity.LcUserId = userName;
            entity.Status = OrderStatus.Open;
            if (!fromParent && getNextLineNumberAsync != null)
                entity.LineNumber = await getNextLineNumberAsync(entity.SalesOrderId);
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

            var resultDto = new AutoRestockDto();
            resultDto.AutoRestocked = false;
            resultDto.ProductName = product.Name;
            var isBelowMinimumStock = await _productService.IsProductBelowMinimumStock(entity.ProductId);
            var productSurplus = await _productService.GetProductSurplusAfterReceptions(entity.ProductId);

            if (isBelowMinimumStock)
            {
                if (productSurplus < 0)
                {
                    if (entity.Product.AutoRestock)
                    {
                        var po = await _purchaseOrderService.AutoRestockProduct(entity.ProductId, userName, false);
                        if (po == null)
                            throw new Exception("Error while creating purchase order for autorestock.");
                        if (product == null) throw new Exception("Error while finding product for autorestock.");
                        resultDto.ProductName = product.Name;
                        resultDto.AutoRestockPurchaseOrderNumber = po.OrderNumber;
                        resultDto.AutoRestocked = true;
                    }
                    else // notify user of low stock
                    {
                        resultDto.ProductShortage = Math.Abs(productSurplus);
                    }
                }
            }
            else // Return shortage if any
            {
                if (productSurplus < 0)
                {
                    resultDto.ProductShortage = Math.Abs(productSurplus);
                }

            }

            return resultDto;
        }

        public async Task DeleteAsync(int id, string userName)
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

        public async Task BlockAsync(int id, string userName)
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

        public async Task UnblockAsync(int id, string userName)
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
