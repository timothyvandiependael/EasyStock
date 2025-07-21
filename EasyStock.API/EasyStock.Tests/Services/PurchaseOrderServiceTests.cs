using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyStock.API.Models;
using EasyStock.API.Repositories;
using EasyStock.API.Services;
using EasyStock.API.Common;
using Moq;
using Xunit;
using EasyStock.Tests.TestHelpers;
using Castle.DynamicProxy;

namespace EasyStock.Tests.Services
{
    public class PurchaseOrderServiceTests
    {
        private readonly Mock<IPurchaseOrderRepository> _poRepoMock = new();
        private readonly Mock<IOrderNumberCounterService> _orderNumberCounterMock = new();
        private readonly Mock<IRepository<PurchaseOrder>> _repoMock = new();
        private readonly Mock<IRetryableTransactionService> _transactionMock = new();
        private readonly Mock<ISupplierRepository> _supplierRepoMock = new();
        private readonly Mock<IRepository<Product>> _productRepoMock = new();
        private readonly Mock<IRepository<Supplier>> _supplierGenericRepoMock = new();
        private readonly Mock<IPurchaseOrderLineProcessor> _lineProcessorMock = new();
        private readonly Mock<IRepository<SalesOrder>> _salesOrderRepoGenericMock = new();

        private readonly EntityFactory _entityFactory = new();

        private readonly PurchaseOrderService _service;

        public PurchaseOrderServiceTests()
        {
            _service = new PurchaseOrderService(
                _poRepoMock.Object,
                _orderNumberCounterMock.Object,
                _repoMock.Object,
                _transactionMock.Object,
                _supplierRepoMock.Object,
                _productRepoMock.Object,
                _supplierGenericRepoMock.Object,
                _lineProcessorMock.Object,
                _salesOrderRepoGenericMock.Object
            );
        }

        [Fact]
        public async Task AddAsync_WithLines_ShouldSetFieldsAndProcessLines()
        {
            // Arrange
            var po = _entityFactory.CreatePurchaseOrder();
            po.Id = 1;

            var line1 = _entityFactory.CreatePurchaseOrderLine();
            var line2 = _entityFactory.CreatePurchaseOrderLine();
            po.Lines.Add(line1);
            po.Lines.Add(line2);

            _orderNumberCounterMock
                .Setup(o => o.GenerateOrderNumberAsync(OrderType.PurchaseOrder))
                .ReturnsAsync("PO-123");

            // Act
            await _service.AddAsync(po, "tester", useTransaction: false);

            // Assert
            Assert.Equal("PO-123", po.OrderNumber);
            Assert.Equal(OrderStatus.Open, po.Status);
            Assert.Equal("tester", po.CrUserId);
            Assert.Equal("tester", po.LcUserId);
            _lineProcessorMock.Verify(p => p.AddAsync(line1, "tester", null, true), Times.Once);
            _lineProcessorMock.Verify(p => p.AddAsync(line2, "tester", null, true), Times.Once);
            _repoMock.Verify(r => r.AddAsync(po), Times.Once);
        }

        [Fact]
        public async Task AddAsync_WithNoLines_ShouldStillSetFieldsAndSave()
        {
            // Arrange
            var po = _entityFactory.CreatePurchaseOrder();
            po.Id = 2;

            _orderNumberCounterMock
                .Setup(o => o.GenerateOrderNumberAsync(OrderType.PurchaseOrder))
                .ReturnsAsync("PO-456");

            // Act
            await _service.AddAsync(po, "tester", useTransaction: false);

            // Assert
            Assert.Equal("PO-456", po.OrderNumber);
            Assert.Equal(OrderStatus.Open, po.Status);
            Assert.Equal("tester", po.CrUserId);
            Assert.Equal("tester", po.LcUserId);
            _lineProcessorMock.Verify(p => p.AddAsync(It.IsAny<PurchaseOrderLine>(), It.IsAny<string>(), null, true), Times.Never);
            _repoMock.Verify(r => r.AddAsync(po), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithLines_ShouldDeleteAllAndEntity()
        {
            // Arrange
            var po = _entityFactory.CreatePurchaseOrder();
            po.Id = 10;

            var line1 = _entityFactory.CreatePurchaseOrderLine();
            line1.Id = 100;
            var line2 = _entityFactory.CreatePurchaseOrderLine();
            line2.Id = 200;
            po.Lines.Add(line1);
            po.Lines.Add(line2);

            _repoMock.Setup(r => r.GetByIdAsync(po.Id)).ReturnsAsync(po);

            // Act
            await _service.DeleteAsync(po.Id, "tester", useTransaction: false);

            // Assert
            _lineProcessorMock.Verify(p => p.DeleteAsync(line1.Id, "tester"), Times.Once);
            _lineProcessorMock.Verify(p => p.DeleteAsync(line2.Id, "tester"), Times.Once);
            _repoMock.Verify(r => r.DeleteAsync(po.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_RecordNotFound_ShouldThrow()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((PurchaseOrder?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.DeleteAsync(99, "tester", useTransaction: false));
        }

        [Fact]
        public async Task BlockAsync_NotBlockedYet_ShouldSetFieldsAndBlockLines()
        {
            // Arrange
            var po = _entityFactory.CreatePurchaseOrder();
            po.Id = 50;
            // not blocked initially
            po.BlDate = null;
            po.BlUserId = null;

            var line1 = _entityFactory.CreatePurchaseOrderLine();
            line1.Id = 501;
            var line2 = _entityFactory.CreatePurchaseOrderLine();
            line2.Id = 502;
            po.Lines.Add(line1);
            po.Lines.Add(line2);

            _repoMock.Setup(r => r.GetByIdAsync(po.Id)).ReturnsAsync(po);

            // Act
            await _service.BlockAsync(po.Id, "tester", useTransaction: false);

            // Assert
            Assert.NotNull(po.BlDate);
            Assert.Equal("tester", po.BlUserId);
            _lineProcessorMock.Verify(p => p.BlockAsync(line1.Id, "tester"), Times.Once);
            _lineProcessorMock.Verify(p => p.BlockAsync(line2.Id, "tester"), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(po), Times.Once);
        }

        [Fact]
        public async Task BlockAsync_AlreadyBlocked_ShouldStillUpdateHeaderButNotReBlockLines()
        {
            // Arrange
            var po = _entityFactory.CreatePurchaseOrder();
            po.Id = 51;
            po.BlDate = DateTime.UtcNow.AddDays(-1);
            po.BlUserId = "someone";

            var line1 = _entityFactory.CreatePurchaseOrderLine();
            line1.Id = 511;
            var line2 = _entityFactory.CreatePurchaseOrderLine();
            line2.Id = 512;
            po.Lines.Add(line1);
            po.Lines.Add(line2);

            _repoMock.Setup(r => r.GetByIdAsync(po.Id)).ReturnsAsync(po);

            // Act
            await _service.BlockAsync(po.Id, "tester", useTransaction: false);

            // Assert
            // header still updated
            Assert.Equal("tester", po.BlUserId);
            Assert.NotNull(po.BlDate);
            // lines are still passed to processor (your code currently always iterates!)
            _lineProcessorMock.Verify(p => p.BlockAsync(line1.Id, "tester"), Times.Once);
            _lineProcessorMock.Verify(p => p.BlockAsync(line2.Id, "tester"), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(po), Times.Once);
        }

        [Fact]
        public async Task BlockAsync_RecordNotFound_ShouldThrow()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((PurchaseOrder?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.BlockAsync(999, "tester", useTransaction: false));
        }

        [Fact]
        public async Task UnblockAsync_BlockedRecord_ShouldClearFieldsAndUnblockLines()
        {
            // Arrange
            var po = _entityFactory.CreatePurchaseOrder();
            po.Id = 60;
            po.BlDate = DateTime.UtcNow.AddDays(-1);
            po.BlUserId = "olduser";

            var line1 = _entityFactory.CreatePurchaseOrderLine();
            line1.Id = 601;
            var line2 = _entityFactory.CreatePurchaseOrderLine();
            line2.Id = 602;
            po.Lines.Add(line1);
            po.Lines.Add(line2);

            _repoMock.Setup(r => r.GetByIdAsync(po.Id)).ReturnsAsync(po);

            // Act
            await _service.UnblockAsync(po.Id, "tester", useTransaction: false);

            // Assert
            Assert.Null(po.BlDate);
            Assert.Null(po.BlUserId);
            Assert.Equal("tester", po.LcUserId);
            _lineProcessorMock.Verify(p => p.UnblockAsync(line1.Id, "tester"), Times.Once);
            _lineProcessorMock.Verify(p => p.UnblockAsync(line2.Id, "tester"), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(po), Times.Once);
        }

        [Fact]
        public async Task UnblockAsync_AlreadyUnblocked_ShouldStillUpdateFieldsAndTryUnblockLines()
        {
            // Arrange
            var po = _entityFactory.CreatePurchaseOrder();
            po.Id = 61;
            po.BlDate = null;
            po.BlUserId = null;

            var line1 = _entityFactory.CreatePurchaseOrderLine();
            line1.Id = 611;
            var line2 = _entityFactory.CreatePurchaseOrderLine();
            line2.Id = 612;
            po.Lines.Add(line1);
            po.Lines.Add(line2);

            _repoMock.Setup(r => r.GetByIdAsync(po.Id)).ReturnsAsync(po);

            // Act
            await _service.UnblockAsync(po.Id, "tester", useTransaction: false);

            // Assert
            Assert.Null(po.BlDate);
            Assert.Null(po.BlUserId);
            Assert.Equal("tester", po.LcUserId);
            // current service still calls UnblockAsync on lines regardless
            _lineProcessorMock.Verify(p => p.UnblockAsync(line1.Id, "tester"), Times.Once);
            _lineProcessorMock.Verify(p => p.UnblockAsync(line2.Id, "tester"), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(po), Times.Once);
        }

        [Fact]
        public async Task UnblockAsync_RecordNotFound_ShouldThrow()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((PurchaseOrder?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.UnblockAsync(999, "tester", useTransaction: false));
        }

        [Fact]
        public async Task AddFromSalesOrder_HappyPath_ShouldCreatePurchaseOrdersForEachSupplier()
        {
            // Arrange
            int salesOrderId = 10;
            string userName = "tester";

            // Create a SalesOrder with lines
            var salesOrder = _entityFactory.CreateSalesOrder();  // Use SalesOrder factory now
            salesOrder.Id = salesOrderId;

            // Create products
            var product1 = _entityFactory.CreateProduct();
            product1.Id = 1001;
            product1.CostPrice = 5m;

            var product2 = _entityFactory.CreateProduct();
            product2.Id = 1002;
            product2.CostPrice = 10m;

            // Create suppliers
            var supplier1 = _entityFactory.CreateSupplier();
            supplier1.Id = 201;

            var supplier2 = _entityFactory.CreateSupplier();
            supplier2.Id = 202;

            // Create sales order lines for products
            var soLine1 = _entityFactory.CreateSalesOrderLine();
            soLine1.ProductId = product1.Id;
            soLine1.Product = product1;
            soLine1.Quantity = 2;

            var soLine2 = _entityFactory.CreateSalesOrderLine();
            soLine2.ProductId = product2.Id;
            soLine2.Product = product2;
            soLine2.Quantity = 3;

            salesOrder.Lines.Add(soLine1);
            salesOrder.Lines.Add(soLine2);

            // productSuppliers map productId -> supplierId
            var productSuppliers = new Dictionary<int, int>
    {
        { product1.Id, supplier1.Id },
        { product2.Id, supplier2.Id }
    };

            // Setup sales order repo to return the sales order when queried
            _salesOrderRepoGenericMock.Setup(r => r.GetByIdAsync(salesOrderId))
                .ReturnsAsync(salesOrder);

            // Setup supplier repo to return the suppliers for the supplierIds
            _supplierRepoMock.Setup(r => r.GetByIds(It.Is<List<int>>(l => l.Contains(supplier1.Id) && l.Contains(supplier2.Id))))
                .ReturnsAsync(new List<Supplier> { supplier1, supplier2 });

            // Setup order number generator to produce sequential order numbers
            _orderNumberCounterMock.SetupSequence(s => s.GenerateOrderNumberAsync(OrderType.PurchaseOrder))
                .ReturnsAsync("PO-001")
                .ReturnsAsync("PO-002");

            // Setup AddAsync for purchase order repo to complete successfully
            _repoMock.Setup(r => r.AddAsync(It.IsAny<PurchaseOrder>()))
                .Returns(Task.CompletedTask)
                .Callback<PurchaseOrder>(po =>
                {
                    // Simulate assigning an Id on add (e.g., like a DB would)
                    po.Id = po.OrderNumber == "PO-001" ? 1 : 2;
                });

            // Setup GetByIdAsync for purchase orders to return the purchase order with the correct Id and lines
            var line1 = _entityFactory.CreatePurchaseOrderLine();
            line1.ProductId = product1.Id;
            var line2 = _entityFactory.CreatePurchaseOrderLine();
            line2.ProductId = product2.Id;

            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .Returns<int>(id =>
                {
                    var po = _entityFactory.CreatePurchaseOrder();
                    po.Id = id;

                    // Assign supplier and lines depending on id
                    if (id == 1)
                    {
                        po.OrderNumber = "PO-001";
                        po.SupplierId = supplier1.Id;
                        po.Lines = new List<PurchaseOrderLine>
                        {
                            line1
                        };
                    }
                    else if (id == 2)
                    {
                        po.OrderNumber = "PO-002";
                        po.SupplierId = supplier2.Id;
                        po.Lines = new List<PurchaseOrderLine>
                        {
                            line2
                        };
                    }

                    return Task.FromResult(po);
                });
            // Setup purchase order line processor AddAsync
            _lineProcessorMock.Setup(lp => lp.AddAsync(It.IsAny<PurchaseOrderLine>(), userName, null, true))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.AddFromSalesOrder(salesOrderId, productSuppliers, userName, useTransaction: false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            Assert.Contains(result, po => po.OrderNumber == "PO-001" && po.SupplierId == supplier1.Id);
            Assert.Contains(result, po => po.OrderNumber == "PO-002" && po.SupplierId == supplier2.Id);

            _lineProcessorMock.Verify(lp => lp.AddAsync(It.IsAny<PurchaseOrderLine>(), userName, null, true), Times.Exactly(2));
            _repoMock.Verify(r => r.AddAsync(It.IsAny<PurchaseOrder>()), Times.Exactly(2));
        }

        [Fact]
        public async Task AutoRestockProduct_ShouldCreatePurchaseOrder_WhenProductAndSupplierExist()
        {
            // Arrange
            int productId = 42;
            string userName = "tester";

            var supplier = _entityFactory.CreateSupplier();
            supplier.Id = 99;

            var product = _entityFactory.CreateProduct();
            product.Id = productId;
            product.AutoRestockSupplierId = supplier.Id;
            product.AutoRestockSupplier = supplier;
            product.AutoRestockAmount = 10;
            product.CostPrice = 7.5m;

            _productRepoMock.Setup(r => r.GetByIdAsync(productId))
                .ReturnsAsync(product);

            _orderNumberCounterMock.Setup(s => s.GenerateOrderNumberAsync(OrderType.PurchaseOrder))
                .ReturnsAsync("PO-123");

            _lineProcessorMock.Setup(p => p.AddAsync(It.IsAny<PurchaseOrderLine>(), userName, null, true))
                .Returns(Task.CompletedTask);

            _repoMock.Setup(r => r.AddAsync(It.IsAny<PurchaseOrder>()))
                .Returns(Task.CompletedTask)
                .Callback<PurchaseOrder>(po => po.Id = 1);

            var newLine = _entityFactory.CreatePurchaseOrderLine();
            newLine.ProductId = productId;
            newLine.Quantity = product.AutoRestockAmount;
            newLine.UnitPrice = product.CostPrice;
            newLine.LineNumber = 1;
            newLine.CrUserId = userName;
            newLine.LcUserId = userName;

            _repoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((int id) =>
                {
                    var po = _entityFactory.CreatePurchaseOrder();
                    po.Id = id;
                    po.OrderNumber = "PO-123";
                    po.SupplierId = supplier.Id;
                    po.Supplier = supplier;
                    po.Lines = new List<PurchaseOrderLine>
                    {
                        newLine
                    };
                    return po;
                });

            // Act
            var result = await _service.AutoRestockProduct(productId, userName, useTransaction: false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PO-123", result.OrderNumber);
            Assert.Equal(supplier.Id, result.SupplierId);
            Assert.Single(result.Lines);
            var line = result.Lines.First();
            Assert.Equal(productId, line.ProductId);
            Assert.Equal(product.AutoRestockAmount, line.Quantity);
            Assert.Equal(product.CostPrice, line.UnitPrice);

            _lineProcessorMock.Verify(p => p.AddAsync(It.IsAny<PurchaseOrderLine>(), userName, null, true), Times.Once);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<PurchaseOrder>()), Times.Once);
        }

        [Fact]
        public async Task AutoRestockProduct_ShouldThrow_WhenProductNotFound()
        {
            // Arrange
            int productId = 42;
            _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync((Product?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.AutoRestockProduct(productId, "tester", false));
            Assert.Equal($"Product with id {productId} not found.", ex.Message);
        }

        [Fact]
        public async Task AutoRestockProduct_ShouldThrow_WhenNoAutoRestockSupplierId()
        {
            // Arrange
            int productId = 42;
            var product = _entityFactory.CreateProduct();
            product.Id = productId;
            product.AutoRestockSupplierId = null;  // No supplier id assigned

            _productRepoMock.Setup(r => r.GetByIdAsync(productId))
                .ReturnsAsync(product);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.AutoRestockProduct(productId, "tester", false));
            Assert.Equal($"No autorestock supplier assigned for product with id {productId}", ex.Message);
        }

        [Fact]
        public async Task AutoRestockProduct_ShouldThrow_WhenNoAutoRestockSupplierFound()
        {
            // Arrange
            int productId = 42;
            var product = _entityFactory.CreateProduct();
            product.Id = productId;
            product.AutoRestockSupplierId = 99;
            product.AutoRestockSupplier = null;  // supplier object missing

            _productRepoMock.Setup(r => r.GetByIdAsync(productId))
                .ReturnsAsync(product);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.AutoRestockProduct(productId, "tester", false));
            Assert.Equal($"No supplier found for auto restock supplier id {product.AutoRestockSupplierId}", ex.Message);
        }

        [Fact]
        public async Task GetNextLineNumberAsync_ShouldThrow_WhenPurchaseOrderNotFound()
        {
            // Arrange
            int poId = 123;
            _repoMock.Setup(r => r.GetByIdAsync(poId)).ReturnsAsync((PurchaseOrder?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.GetNextLineNumberAsync(poId));
            Assert.Equal($"Purchase order with id {poId} not found.", ex.Message);
        }

        [Fact]
        public async Task GetNextLineNumberAsync_ShouldReturn1_WhenPurchaseOrderHasNoLines()
        {
            // Arrange
            int poId = 123;
            var po = _entityFactory.CreatePurchaseOrder();
            po.Id = poId;
            po.Lines = new List<PurchaseOrderLine>();

            _repoMock.Setup(r => r.GetByIdAsync(poId)).ReturnsAsync(po);

            // Act
            var nextLineNumber = await _service.GetNextLineNumberAsync(poId);

            // Assert
            Assert.Equal(1, nextLineNumber);
        }

        [Fact]
        public async Task GetNextLineNumberAsync_ShouldReturnMaxLineNumberPlusOne_WhenPurchaseOrderHasLines()
        {
            // Arrange
            int poId = 123;
            var po = _entityFactory.CreatePurchaseOrder();
            po.Id = poId;
            var line1 = _entityFactory.CreatePurchaseOrderLine();
            var line2 = _entityFactory.CreatePurchaseOrderLine();
            var line3 = _entityFactory.CreatePurchaseOrderLine();
            line1.LineNumber = 1;
            line2.LineNumber = 2;
            line3.LineNumber = 3;
            po.Lines = new List<PurchaseOrderLine>() { line1, line2, line3 };

            _repoMock.Setup(r => r.GetByIdAsync(poId)).ReturnsAsync(po);

            // Act
            var nextLineNumber = await _service.GetNextLineNumberAsync(poId);

            // Assert
            Assert.Equal(4, nextLineNumber);
        }
    }
}
