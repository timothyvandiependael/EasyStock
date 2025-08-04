using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;
using Moq;
using EasyStock.API.Models;
using EasyStock.API.Repositories;
using EasyStock.API.Services;
using EasyStock.Tests.TestHelpers;
using EasyStock.API.Common;


namespace EasyStock.Tests.Services
{
    public class SalesOrderLineProcessorTests
    {
        private readonly EntityFactory _entityFactory;
        private readonly Mock<IRepository<Product>> _mockProductRepository;
        private readonly Mock<IRepository<SalesOrderLine>> _mockSalesOrderLineRepository;
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<IPurchaseOrderService> _mockPurchaseOrderService;
        private readonly SalesOrderLineProcessor _service;

        public SalesOrderLineProcessorTests()
        {
            _entityFactory = new EntityFactory();

            _mockProductRepository = new Mock<IRepository<Product>>();
            _mockSalesOrderLineRepository = new Mock<IRepository<SalesOrderLine>>();
            _mockProductService = new Mock<IProductService>();
            _mockPurchaseOrderService = new Mock<IPurchaseOrderService>();

            _service = new SalesOrderLineProcessor(_mockProductRepository.Object, _mockSalesOrderLineRepository.Object, _mockProductService.Object, _mockPurchaseOrderService.Object);
        }


        [Fact]
        public async Task AddAsync_ValidInput_ShouldSetDatesUserAndStatusAndUpdateStocks()
        {
            // Arrange
            var line = _entityFactory.CreateSalesOrderLine();
            var product = _entityFactory.CreateProduct();
            product.AvailableStock = 10;
            product.ReservedStock = 0;
            line.Quantity = 5;
            line.ProductId = product.Id;
            line.SalesOrderId = 1;

            _mockProductRepository.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);
            _mockSalesOrderLineRepository.Setup(r => r.AddAsync(line)).Returns(Task.CompletedTask);

            var userName = "testUser";
            Func<int, Task<int>> getNextLineNumber = _ => Task.FromResult(1);

            // Act
            await _service.AddAsync(line, userName, getNextLineNumber, fromParent: false);

            // Assert
            Assert.Equal(OrderStatus.Open, line.Status);
            Assert.Equal(userName, line.CrUserId);
            Assert.Equal(userName, line.LcUserId);
            Assert.True(line.LineNumber == 1);

            // Stock assertions
            Assert.Equal(5, product.ReservedStock);
            Assert.Equal(5, product.AvailableStock);
            Assert.Equal(userName, product.LcUserId);
            Assert.NotEqual(default, product.LcDate);

            _mockSalesOrderLineRepository.Verify(r => r.AddAsync(line), Times.Once);
            _mockProductRepository.Verify(r => r.GetByIdAsync(product.Id), Times.Once);
        }

        [Fact]
        public async Task AddAsync_ProductNotFound_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var line = _entityFactory.CreateSalesOrderLine();
            line.ProductId = 99; // nonexistent
            _mockProductRepository.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync((Product?)null);
            _mockSalesOrderLineRepository.Setup(r => r.AddAsync(line)).Returns(Task.CompletedTask);

            var userName = "testUser";

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddAsync(line, userName, null, false));
            Assert.Contains("Product with ID 99 not found", ex.Message);
        }

        [Fact]
        public async Task AddAsync_QuantityGreaterThanAvailableStock_ShouldAdjustBackorder()
        {
            // Arrange
            var line = _entityFactory.CreateSalesOrderLine();
            var product = _entityFactory.CreateProduct();
            product.AvailableStock = 3;
            product.ReservedStock = 0;
            line.Quantity = 5; // greater than available stock
            line.ProductId = product.Id;

            _mockProductRepository.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);
            _mockSalesOrderLineRepository.Setup(r => r.AddAsync(line)).Returns(Task.CompletedTask);

            var userName = "testUser";

            // Act
            await _service.AddAsync(line, userName, null, false);

            // Assert
            Assert.Equal(0, product.AvailableStock);
            Assert.Equal(3, product.ReservedStock);
            Assert.Equal(2, product.BackOrderedStock);
        }

        [Fact]
        public async Task DeleteAsync_RecordNotFound_ShouldThrowInvalidOperationException()
        {
            // Arrange
            _mockSalesOrderLineRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((SalesOrderLine?)null);
            var userName = "user";

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(1, userName));
            Assert.Contains("Unable to delete record with ID 1", ex.Message);
        }

        [Fact]
        public async Task DeleteAsync_RecordOpenStatus_ShouldUpdateProductStockCorrectly()
        {
            // Arrange
            var line = _entityFactory.CreateSalesOrderLine();
            line.Status = OrderStatus.Open;
            line.Quantity = 5;

            var product = _entityFactory.CreateProduct();
            product.BackOrderedStock = 0;
            product.ReservedStock = 10;
            product.AvailableStock = 5;

            _mockSalesOrderLineRepository.Setup(r => r.GetByIdAsync(line.Id)).ReturnsAsync(line);
            _mockProductRepository.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync(product);
            _mockSalesOrderLineRepository.Setup(r => r.DeleteAsync(line.Id)).Returns(Task.CompletedTask);

            var userName = "user";

            // Act
            await _service.DeleteAsync(line.Id, userName);

            // Assert
            Assert.Equal(5, product.ReservedStock); // 10 - 5
            Assert.Equal(10, product.AvailableStock); // 5 + 5
            Assert.Equal(userName, product.LcUserId);

            _mockSalesOrderLineRepository.Verify(r => r.DeleteAsync(line.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_RecordBackOrderedStockAdjust_ShouldHandleBackOrderStock()
        {
            // Arrange
            var line = _entityFactory.CreateSalesOrderLine();
            line.Status = OrderStatus.Open;
            line.Quantity = 7;

            var product = _entityFactory.CreateProduct();
            product.BackOrderedStock = 10;
            product.ReservedStock = 5;
            product.AvailableStock = 0;

            _mockSalesOrderLineRepository.Setup(r => r.GetByIdAsync(line.Id)).ReturnsAsync(line);
            _mockProductRepository.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync(product);
            _mockSalesOrderLineRepository.Setup(r => r.DeleteAsync(line.Id)).Returns(Task.CompletedTask);

            var userName = "user";

            // Act
            await _service.DeleteAsync(line.Id, userName);

            // Assert
            Assert.Equal(3, product.BackOrderedStock); // 10 - 7
            Assert.Equal(5, product.ReservedStock);
            Assert.Equal(0, product.AvailableStock);
        }

        [Fact]
        public async Task BlockAsync_RecordNotFound_ShouldThrowInvalidOperationException()
        {
            // Arrange
            _mockSalesOrderLineRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((SalesOrderLine?)null);
            var userName = "user";

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.BlockAsync(1, userName));
            Assert.Contains("Unable to block record with ID 1", ex.Message);
        }

        [Fact]
        public async Task BlockAsync_RecordAlreadyBlocked_ShouldNotChangeProductStock()
        {
            // Arrange
            var line = _entityFactory.CreateSalesOrderLine();
            line.Status = OrderStatus.Complete; // Not Open or Partial, so no stock update
            line.BlDate = DateTime.UtcNow;
            line.BlUserId = "someone";

            _mockSalesOrderLineRepository.Setup(r => r.GetByIdAsync(line.Id)).ReturnsAsync(line);
            _mockSalesOrderLineRepository.Setup(r => r.UpdateAsync(line)).Returns(Task.CompletedTask);

            var userName = "user";

            // Act
            await _service.BlockAsync(line.Id, userName);

            // Assert
            Assert.NotNull(line.BlDate);
            Assert.Equal(userName, line.BlUserId);
            _mockSalesOrderLineRepository.Verify(r => r.UpdateAsync(line), Times.Once);
            _mockProductRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task BlockAsync_RecordOpenStatus_ShouldAdjustProductStock()
        {
            // Arrange
            var line = _entityFactory.CreateSalesOrderLine();
            line.Status = OrderStatus.Open;
            line.Quantity = 5;

            var product = _entityFactory.CreateProduct();
            product.BackOrderedStock = 3;
            product.ReservedStock = 10;
            product.AvailableStock = 2;

            _mockSalesOrderLineRepository.Setup(r => r.GetByIdAsync(line.Id)).ReturnsAsync(line);
            _mockProductRepository.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync(product);
            _mockSalesOrderLineRepository.Setup(r => r.UpdateAsync(line)).Returns(Task.CompletedTask);

            var userName = "user";

            // Act
            await _service.BlockAsync(line.Id, userName);

            // Assert
            Assert.Equal(userName, line.BlUserId);
            Assert.NotNull(line.BlDate);

            // Backordered stock reduced by 5, but cannot go below zero
            Assert.Equal(0, product.BackOrderedStock);
            // Reserved stock reduced by difference (5 - 3 = 2)
            Assert.Equal(8, product.ReservedStock); // 10 - 2
            Assert.Equal(4, product.AvailableStock); // 2 + 2
            Assert.Equal(userName, product.LcUserId);
        }

        [Fact]
        public async Task UnblockAsync_RecordNotFound_ShouldThrowInvalidOperationException()
        {
            // Arrange
            _mockSalesOrderLineRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((SalesOrderLine?)null);
            var userName = "user";

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UnblockAsync(1, userName));
            Assert.Contains("Unable to unblock record with ID 1", ex.Message);
        }

        [Fact]
        public async Task UnblockAsync_RecordOpenStatus_ShouldRestoreProductStock()
        {
            // Arrange
            var line = _entityFactory.CreateSalesOrderLine();
            line.Status = OrderStatus.Open;
            line.Quantity = 6;

            var product = _entityFactory.CreateProduct();
            product.AvailableStock = 4;
            product.ReservedStock = 2;
            product.BackOrderedStock = 0;

            _mockSalesOrderLineRepository.Setup(r => r.GetByIdAsync(line.Id)).ReturnsAsync(line);
            _mockProductRepository.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync(product);
            _mockSalesOrderLineRepository.Setup(r => r.UpdateAsync(line)).Returns(Task.CompletedTask);

            var userName = "user";

            // Act
            await _service.UnblockAsync(line.Id, userName);

            // Assert
            Assert.Null(line.BlDate);
            Assert.Null(line.BlUserId);
            Assert.Equal(userName, line.LcUserId);
            Assert.NotEqual(default, line.LcDate);

            // Because quantity (6) > available stock (4), backorder triggered
            Assert.Equal(4, product.ReservedStock);
            Assert.Equal(2, product.BackOrderedStock);
            Assert.Equal(0, product.AvailableStock);
            Assert.Equal(userName, product.LcUserId);
        }

        [Fact]
        public async Task UnblockAsync_RecordNotOpenOrPartial_ShouldNotUpdateProductStock()
        {
            // Arrange
            var line = _entityFactory.CreateSalesOrderLine();
            line.Status = OrderStatus.Complete;
            line.Quantity = 4;

            _mockSalesOrderLineRepository.Setup(r => r.GetByIdAsync(line.Id)).ReturnsAsync(line);
            _mockSalesOrderLineRepository.Setup(r => r.UpdateAsync(line)).Returns(Task.CompletedTask);

            var userName = "user";

            // Act
            await _service.UnblockAsync(line.Id, userName);

            // Assert
            Assert.Null(line.BlDate);
            Assert.Null(line.BlUserId);
            Assert.Equal(userName, line.LcUserId);
            _mockProductRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }
    }
}
