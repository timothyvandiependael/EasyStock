using EasyStock.API.Common;
using EasyStock.API.Data;
using EasyStock.API.Models;
using EasyStock.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EasyStock.API.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IOrderNumberCounterService _orderNumberCounterService;
        private readonly IRepository<PurchaseOrder> _repository;
        private readonly IRetryableTransactionService _retryableTransactionService;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IRepository<Product> _genericProductRepository;
        private readonly IRepository<Supplier> _genericSupplierRepository;
        private readonly IPurchaseOrderLineProcessor _purchaseOrderLineProcessor;
        private readonly IRepository<SalesOrder> _salesOrderRepository;

        public PurchaseOrderService(IPurchaseOrderRepository purchaseOrderRepository, IOrderNumberCounterService orderNumberCounterService, IRepository<PurchaseOrder> repository, IRetryableTransactionService retryableTransactionService, ISupplierRepository supplierRepository, IRepository<Product> genericProductRepository, IRepository<Supplier> genericSupplierRepository, IPurchaseOrderLineProcessor purchaseOrderLineProcessor, IRepository<SalesOrder> salesOrderRepository)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
            _orderNumberCounterService = orderNumberCounterService;
            _repository = repository;
            _retryableTransactionService = retryableTransactionService;
            _supplierRepository = supplierRepository;
            _genericProductRepository = genericProductRepository;
            _genericSupplierRepository = genericSupplierRepository;
            _purchaseOrderLineProcessor = purchaseOrderLineProcessor;
            _salesOrderRepository = salesOrderRepository;
        }

        public async Task<IEnumerable<PurchaseOrderOverview>> GetAllAsync()
            => await _purchaseOrderRepository.GetAllAsync();

        public async Task<PaginationResult<PurchaseOrderOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination)
            => await _purchaseOrderRepository.GetAdvancedAsync(filters, sorting, pagination);

        public async Task AddAsync(PurchaseOrder entity, string userName, bool useTransaction = true)
        {
            if (useTransaction)
            {
                await _retryableTransactionService.ExecuteAsync(async () =>
                {
                    await AddAsyncInternal(entity, userName);
                });
            }
            else
            {
                await AddAsyncInternal(entity, userName);
            }        
        }

        private async Task AddAsyncInternal(PurchaseOrder entity, string userName)
        {
            entity.OrderNumber = await _orderNumberCounterService.GenerateOrderNumberAsync(OrderType.PurchaseOrder);
            entity.Status = OrderStatus.Open;
            entity.CrDate = DateTime.UtcNow;
            entity.LcDate = entity.CrDate;
            entity.CrUserId = userName;
            entity.LcUserId = userName;

            var lineCounter = 1;
            var lines = entity.Lines.ToList();
            entity.Lines.Clear();

            await _repository.AddAsync(entity);

            foreach (var line in lines)
            {
                line.LineNumber = lineCounter;
                line.PurchaseOrderId = entity.Id;
                await _purchaseOrderLineProcessor.AddAsync(line, userName, null, true);

                lineCounter++;
            }

            
        }

        public async Task DeleteAsync(int id, string userName, bool useTransaction = true)
        {
            if (useTransaction)
            {
                await _retryableTransactionService.ExecuteAsync(async () =>
                {
                    await DeleteAsyncInternal(id, userName);
                });
            }
            else
            {
                await DeleteAsyncInternal(id, userName);
            }
                
        }

        private async Task DeleteAsyncInternal(int id, string userName)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new InvalidOperationException($"Unable to delete record with ID {id}");

            foreach (var line in entity.Lines)
            {
                await _purchaseOrderLineProcessor.DeleteAsync(line.Id, userName);
            }
            await _repository.DeleteAsync(id);
        }

        public async Task BlockAsync(int id, string userName, bool useTransaction = true)
        {
            if (useTransaction)
            {
                await _retryableTransactionService.ExecuteAsync(async () =>
                {
                    await BlockAsyncInternal(id, userName);
                });
            }
            else
            {
                await BlockAsyncInternal(id, userName);
            }
                
        }

        private async Task BlockAsyncInternal(int id, string userName)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new InvalidOperationException($"Unable to block record with ID {id}");
            entity.BlDate = DateTime.UtcNow;
            entity.BlUserId = userName;

            foreach (var line in entity.Lines)
            {
                await _purchaseOrderLineProcessor.BlockAsync(line.Id, userName);
            }
            await _repository.UpdateAsync(entity);
        }

        public async Task UnblockAsync(int id, string userName, bool useTransaction = true)
        {
            if (useTransaction)
            {
                await _retryableTransactionService.ExecuteAsync(async () =>
                {
                    await UnblockAsyncInternal(id, userName);
                });
            }
            else
            {
                await UnblockAsyncInternal(id, userName);
            }
                
        }

        private async Task UnblockAsyncInternal(int id, string userName)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new InvalidOperationException($"Unable to unblock record with ID {id}");
            entity.BlDate = null;
            entity.BlUserId = null;
            entity.LcDate = DateTime.UtcNow;
            entity.LcUserId = userName;

            foreach (var line in entity.Lines)
            {
                await _purchaseOrderLineProcessor.UnblockAsync(line.Id, userName);
            }
            await _repository.UpdateAsync(entity);
        }

        public async Task<List<PurchaseOrder>> AddFromSalesOrder(int salesOrderId, Dictionary<int, int> productSuppliers, string userName, bool useTransaction = true)
        {
            var purchaseOrders = new List<PurchaseOrder>();

            if (useTransaction)
            {
                await _retryableTransactionService.ExecuteAsync(async () =>
                {
                    purchaseOrders = await AddFromSalesOrderInternal(salesOrderId, productSuppliers, userName);
                });
            }
            else
            {
                purchaseOrders = await AddFromSalesOrderInternal(salesOrderId, productSuppliers, userName);
            }    

            return purchaseOrders;
        }

        private async Task<List<PurchaseOrder>> AddFromSalesOrderInternal(int salesOrderId, Dictionary<int, int> productSuppliers, string userName)
        {
            var purchaseOrders = new List<PurchaseOrder>();

            var so = await _salesOrderRepository.GetByIdAsync(salesOrderId);
            if (so == null)
                throw new Exception($"Salesorder with id {salesOrderId} not found.");

            var supplierIds = productSuppliers.Values.Distinct().ToList();
            var suppliers = await _supplierRepository.GetByIds(supplierIds);

            var groupBySupplier = productSuppliers.GroupBy(d => d.Value);

            foreach (var supplierGroup in groupBySupplier)
            {
                var supplierId = supplierGroup.Key;
                var supplier = suppliers.First(s => s.Id == supplierId);

                var po = new PurchaseOrder
                {
                    OrderNumber = await _orderNumberCounterService.GenerateOrderNumberAsync(OrderType.PurchaseOrder),
                    SupplierId = supplierId,
                    Status = OrderStatus.Open,
                    CrDate = DateTime.UtcNow,
                    LcDate = DateTime.UtcNow,
                    CrUserId = userName,
                    LcUserId = userName,
                    Supplier = supplier,
                    Lines = new List<PurchaseOrderLine>()
                };

                var productIdsForSupplier = supplierGroup.Select(p => p.Key).ToList();

                var lineNumber = 1;
                foreach (var productId in productIdsForSupplier)
                {
                    var soLine = so.Lines.First(l => l.ProductId == productId);

                    var poLine = new PurchaseOrderLine
                    {
                        PurchaseOrder = po,
                        ProductId = productId,
                        Product = soLine.Product,
                        Quantity = soLine.Quantity,
                        UnitPrice = soLine.Product.CostPrice,
                        Status = OrderStatus.Open,
                        CrDate = DateTime.UtcNow,
                        LcDate = DateTime.UtcNow,
                        CrUserId = userName,
                        LcUserId = userName,
                        LineNumber = lineNumber
                    };

                    await _purchaseOrderLineProcessor.AddAsync(poLine, userName, null, true);

                    lineNumber++;
                }

                await _repository.AddAsync(po);

                // Get with lazy loaded lines
                po = await _repository.GetByIdAsync(po.Id);
                if (po != null)
                    purchaseOrders.Add(po);
            }

            return purchaseOrders;
        }

        public async Task<PurchaseOrder?> AutoRestockProduct(int productId, string userName, bool useTransaction = true)
        {
            PurchaseOrder? po = null;

            if (useTransaction)
            {
                await _retryableTransactionService.ExecuteAsync(async () =>
                {
                    po = await AutoRestockProductInternal(productId, userName);
                });
            }
            else
            {
                po = await AutoRestockProductInternal(productId, userName);
            }

            return po;

        }

        private async Task<PurchaseOrder?> AutoRestockProductInternal(int productId, string userName)
        {
            PurchaseOrder? po;
            var product = await _genericProductRepository.GetByIdAsync(productId);
            if (product == null)
                throw new Exception($"Product with id {productId} not found.");
            if (product.AutoRestockSupplierId == null)
                throw new Exception($"No autorestock supplier assigned for product with id {productId}");
            if (product.AutoRestockSupplier == null)
                throw new Exception($"No supplier found for auto restock supplier id {product.AutoRestockSupplierId}");

            po = new PurchaseOrder
            {
                OrderNumber = await _orderNumberCounterService.GenerateOrderNumberAsync(OrderType.PurchaseOrder),
                SupplierId = product.AutoRestockSupplierId.Value,
                Status = OrderStatus.Open,
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = userName,
                LcUserId = userName,
                Supplier = product.AutoRestockSupplier,
                Lines = new List<PurchaseOrderLine>()
            };

            var line = new PurchaseOrderLine
            {
                PurchaseOrder = po,
                ProductId = productId,
                Product = product,
                Quantity = product.AutoRestockAmount,
                UnitPrice = product.CostPrice,
                Status = OrderStatus.Open,
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = userName,
                LcUserId = userName,
                LineNumber = 1
            };

            await _purchaseOrderLineProcessor.AddAsync(line, userName, null, true);

            await _repository.AddAsync(po);
            po = await _repository.GetByIdAsync(po.Id);

            return po;
        }

        public async Task<int> GetNextLineNumberAsync(int id)
        {
            var po = await _repository.GetByIdAsync(id);
            if (po == null) throw new Exception($"Purchase order with id {id} not found.");
            var nextLineNumber = po.Lines.Any() ? po.Lines.Max(l => l.LineNumber) + 1 : 1;
            return nextLineNumber;
        }
    }
}
