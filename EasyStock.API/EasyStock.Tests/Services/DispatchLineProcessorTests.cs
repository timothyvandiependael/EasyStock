using EasyStock.API.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyStock.API.Models;
using EasyStock.API.Services;
using Newtonsoft.Json;
using EasyStock.API.Migrations;
using EasyStock.Tests.TestHelpers;

namespace EasyStock.Tests.Services
{
    public class DispatchLineProcessorTests
    {
        private readonly Mock<IRepository<DispatchLine>> _dispatchLineRepoMock;
        private readonly Mock<IRepository<SalesOrderLine>> _salesOrderLineRepoMock;
        private readonly Mock<IRepository<SalesOrder>> _salesOrderRepoMock;
        private readonly Mock<IRepository<Product>> _productRepoMock;
        private readonly Mock<IRepository<StockMovement>> _stockMovementRepoMock;
        private readonly IDispatchLineProcessor _dispatchLineProcessor;

        private readonly EntityFactory _entityFactory;

        public DispatchLineProcessorTests()
        {
            _dispatchLineRepoMock = new Mock<IRepository<DispatchLine>>();
            _salesOrderLineRepoMock = new Mock<IRepository<SalesOrderLine>>();
            _salesOrderRepoMock = new Mock<IRepository<SalesOrder>>();
            _productRepoMock = new Mock<IRepository<Product>>();
            _stockMovementRepoMock = new Mock<IRepository<StockMovement>>();

            _dispatchLineProcessor = new DispatchLineProcessor(
                _dispatchLineRepoMock.Object,
                _salesOrderLineRepoMock.Object,
                _salesOrderRepoMock.Object,
                _productRepoMock.Object,
                _stockMovementRepoMock.Object);

            _entityFactory = new EntityFactory();
        }

        [Fact]
        public async Task SetSOStatusFields_SetsLcDateAndUserOnSalesOrderLineAndCallsRepositories_WhenInputsCorrect()
        {
            // Arrange
            var quantity = 5;
            var salesOrderLineId = 1;
            var userName = "testuser";

            var salesOrderLine = _entityFactory.CreateSalesOrderLine();
            salesOrderLine.Quantity = 10;
            salesOrderLine.LcDate = new DateTime(2000, 1, 1);
            salesOrderLine.LcUserId = "olduser";
            var salesOrder = _entityFactory.CreateSalesOrder();
            salesOrder.Id = 1;

            _salesOrderLineRepoMock.Setup(r => r.GetByIdAsync(salesOrderLineId)).ReturnsAsync(salesOrderLine);
            _salesOrderRepoMock.Setup(r => r.GetByIdAsync(salesOrder.Id)).ReturnsAsync(salesOrder);

            // Act
            await _dispatchLineProcessor.SetSOStatusFields(quantity, salesOrderLineId, userName);

            // Assert
            Assert.NotEqual(new DateTime(2000, 1, 1), salesOrderLine.LcDate);
            Assert.True((DateTime.UtcNow - salesOrderLine.LcDate).TotalSeconds < 5);
            Assert.Equal(userName, salesOrderLine.LcUserId);
            _salesOrderLineRepoMock.Verify(r => r.GetByIdAsync(salesOrderLineId), Times.Once);
            _salesOrderRepoMock.Verify(r => r.GetByIdAsync(salesOrder.Id), Times.Once);
        }

        [Fact]
        public async Task SetSOStatusFields_SetsLineStatusToPartial_WhenQuantitySmallerThanLineQuantity()
        {
            // Arrange
            var quantity = 5;
            var salesOrderLineId = 1;
            var userName = "testuser";

            var salesOrderLine = _entityFactory.CreateSalesOrderLine();
            salesOrderLine.Quantity = 10;
            salesOrderLine.Status = API.Common.OrderStatus.Open;
            var salesOrder = _entityFactory.CreateSalesOrder();
            salesOrder.Id = 1;

            _salesOrderLineRepoMock.Setup(r => r.GetByIdAsync(salesOrderLineId)).ReturnsAsync(salesOrderLine);
            _salesOrderRepoMock.Setup(r => r.GetByIdAsync(salesOrder.Id)).ReturnsAsync(salesOrder);

            // Act
            await _dispatchLineProcessor.SetSOStatusFields(quantity, salesOrderLineId, userName);

            // Assert
            Assert.True(salesOrderLine.Status == API.Common.OrderStatus.Partial);
        }

        [Fact]
        public async Task SetSOStatusFields_SetsLineStatusToComplete_WhenQuantityEqualToLineQuantity()
        {
            // Arrange
            var quantity = 10;
            var salesOrderLineId = 1;
            var userName = "testuser";

            var salesOrderLine = _entityFactory.CreateSalesOrderLine();
            salesOrderLine.Quantity = 10;
            salesOrderLine.Status = API.Common.OrderStatus.Open;
            var salesOrder = _entityFactory.CreateSalesOrder();
            salesOrder.Id = 1;

            _salesOrderLineRepoMock.Setup(r => r.GetByIdAsync(salesOrderLineId)).ReturnsAsync(salesOrderLine);
            _salesOrderRepoMock.Setup(r => r.GetByIdAsync(salesOrder.Id)).ReturnsAsync(salesOrder);

            // Act
            await _dispatchLineProcessor.SetSOStatusFields(quantity, salesOrderLineId, userName);

            // Assert
            Assert.True(salesOrderLine.Status == API.Common.OrderStatus.Complete);
        }

        [Fact]
        public async Task SetSOStatusFields_ThrowsError_WhenQuantityGreaterThanLineQuantity()
        {
            // Arrange
            var quantity = 15;
            var salesOrderLineId = 1;
            var userName = "testuser";

            var salesOrderLine = _entityFactory.CreateSalesOrderLine();
            salesOrderLine.Quantity = 10;
            salesOrderLine.Status = API.Common.OrderStatus.Open;
            var salesOrder = _entityFactory.CreateSalesOrder();
            salesOrder.Id = 1;

            _salesOrderLineRepoMock.Setup(r => r.GetByIdAsync(salesOrderLineId)).ReturnsAsync(salesOrderLine);
            _salesOrderRepoMock.Setup(r => r.GetByIdAsync(salesOrder.Id)).ReturnsAsync(salesOrder);

            // Act
            InvalidOperationException? exception = null;
            try
            {
                await _dispatchLineProcessor.SetSOStatusFields(quantity, salesOrderLineId, userName);
            }
            catch (InvalidOperationException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.NotNull(exception);
            Assert.Contains("Input quantity for dispatch line is greater than quantity on sales order line", exception.Message);
        }

        [Fact]
        public async Task SetSOStatusFields_ThrowsError_WhenSalesOrderLineNotFound()
        {
            // Arrange
            var quantity = 5;
            var salesOrderLineId = 1;
            var userName = "testuser";

            var salesOrderLine = _entityFactory.CreateSalesOrderLine();

            _salesOrderLineRepoMock.Setup(r => r.GetByIdAsync(salesOrderLineId)).ReturnsAsync((SalesOrderLine?)null);

            // Act
            InvalidOperationException? exception = null;
            try
            {
                await _dispatchLineProcessor.SetSOStatusFields(quantity, salesOrderLineId, userName);
            }
            catch (InvalidOperationException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.NotNull(exception);
            Assert.Contains("Unable to find sales order line with ID", exception.Message);
        }

        [Fact]
        public async Task SetSOStatusFields_ThrowsError_WhenSalesOrderNotFound()
        {
            // Arrange
            var quantity = 5;
            var salesOrderLineId = 1;
            var userName = "testuser";

            var salesOrderLine = _entityFactory.CreateSalesOrderLine();
            salesOrderLine.Quantity = 10;

            var salesOrder = _entityFactory.CreateSalesOrder();

            _salesOrderLineRepoMock.Setup(r => r.GetByIdAsync(salesOrderLineId))
                .ReturnsAsync(salesOrderLine);
            _salesOrderRepoMock.Setup(r => r.GetByIdAsync(salesOrder.Id))
                .ReturnsAsync((SalesOrder?)null);

            // Act
            InvalidOperationException? exception = null;
            try
            {
                await _dispatchLineProcessor.SetSOStatusFields(quantity, salesOrderLineId, userName);
            }
            catch (InvalidOperationException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.NotNull(exception);
            Assert.Contains("Unable to find sales order with ID", exception.Message);
        }

        [Fact]
        public async Task SetSOStatusFields_SetsLcDateAndUserOnSalesOrder_WhenInputsCorrect()
        {
            // Arrange
            var quantity = 5;
            var salesOrderLineId = 1;
            var userName = "testuser";

            var salesOrderLine = _entityFactory.CreateSalesOrderLine();
            salesOrderLine.Quantity = 10;
            var salesOrder = _entityFactory.CreateSalesOrder();
            salesOrder.Id = 1;
            salesOrder.LcDate = new DateTime(2000, 1, 1);
            salesOrder.LcUserId = "olduser";


            _salesOrderLineRepoMock.Setup(r => r.GetByIdAsync(salesOrderLineId)).ReturnsAsync(salesOrderLine);
            _salesOrderRepoMock.Setup(r => r.GetByIdAsync(salesOrder.Id)).ReturnsAsync(salesOrder);

            // Act
            await _dispatchLineProcessor.SetSOStatusFields(quantity, salesOrderLineId, userName);

            // Assert
            Assert.NotEqual(new DateTime(2000, 1, 1), salesOrder.LcDate);
            Assert.True((DateTime.UtcNow - salesOrder.LcDate).TotalSeconds < 5);
            Assert.Equal(userName, salesOrder.LcUserId);
        }

        [Fact]
        public async Task SetSOStatusFields_SetsOrderStatusToPartial_WhenPartialLineFound()
        {
            // Arrange
            var quantity = 5;
            var salesOrderLineId = 1;
            var userName = "testuser";

            var salesOrderLine = _entityFactory.CreateSalesOrderLine();
            salesOrderLine.Quantity = 10;
            salesOrderLine.Status = API.Common.OrderStatus.Open;
            var salesOrder = _entityFactory.CreateSalesOrder();
            salesOrder.Id = 1;
            salesOrder.Status = API.Common.OrderStatus.Open;
            salesOrder.Lines.Add(salesOrderLine);

            _salesOrderLineRepoMock.Setup(r => r.GetByIdAsync(salesOrderLineId)).ReturnsAsync(salesOrderLine);
            _salesOrderRepoMock.Setup(r => r.GetByIdAsync(salesOrder.Id)).ReturnsAsync(salesOrder);

            // Act
            await _dispatchLineProcessor.SetSOStatusFields(quantity, salesOrderLineId, userName);

            // Assert
            Assert.True(salesOrder.Status == API.Common.OrderStatus.Partial);
        }

        [Fact]
        public async Task SetSOStatusFields_SetsOrderStatusToComplete_WhenNoPartialLinesFound()
        {
            // Arrange
            var quantity = 10;
            var salesOrderLineId = 1;
            var userName = "testuser";

            var salesOrderLine = _entityFactory.CreateSalesOrderLine();
            salesOrderLine.Quantity = 10;
            salesOrderLine.Status = API.Common.OrderStatus.Open;
            var salesOrder = _entityFactory.CreateSalesOrder();
            salesOrder.Id = 1;
            salesOrder.Status = API.Common.OrderStatus.Open;

            _salesOrderLineRepoMock.Setup(r => r.GetByIdAsync(salesOrderLineId)).ReturnsAsync(salesOrderLine);
            _salesOrderRepoMock.Setup(r => r.GetByIdAsync(salesOrder.Id)).ReturnsAsync(salesOrder);

            // Act
            await _dispatchLineProcessor.SetSOStatusFields(quantity, salesOrderLineId, userName);

            // Assert
            Assert.True(salesOrder.Status == API.Common.OrderStatus.Complete);
        }

        [Fact]
        public async Task AddAsync_SetsAuditFieldsOnDispatchLine_AndCallsRepositoryAdd()
        {
            // Arrange
            var userName = "testuser";
            var now = DateTime.UtcNow;
            var dispatchLine = _entityFactory.CreateDispatchLine();
            dispatchLine.CrDate = new DateTime(2000, 1, 1);
            dispatchLine.LcDate = new DateTime(2000, 1, 1);
            dispatchLine.CrUserId = "olduser";
            dispatchLine.LcUserId = "olduser";

            var product = _entityFactory.CreateProduct();

            _productRepoMock.Setup(r => r.GetByIdAsync(dispatchLine.ProductId))
                .ReturnsAsync(product);

            _dispatchLineRepoMock.Setup(r => r.AddAsync(It.IsAny<DispatchLine>()))
                .Returns(Task.CompletedTask);

            _stockMovementRepoMock.Setup(r => r.AddAsync(It.IsAny<StockMovement>()))
                .Returns(Task.CompletedTask);


            var salesOrderLine = _entityFactory.CreateSalesOrderLine();
            _salesOrderLineRepoMock.Setup(r => r.GetByIdAsync(salesOrderLine.Id)).ReturnsAsync(salesOrderLine);
            var salesOrder = _entityFactory.CreateSalesOrder();
            _salesOrderRepoMock.Setup(r => r.GetByIdAsync(salesOrder.Id)).ReturnsAsync(salesOrder);

            // Act
            await _dispatchLineProcessor.AddAsync(dispatchLine, userName, null);

            // Assert
            Assert.True((DateTime.UtcNow - dispatchLine.CrDate).TotalSeconds < 5);
            Assert.True((DateTime.UtcNow - dispatchLine.LcDate).TotalSeconds < 5);
            Assert.Equal(userName, dispatchLine.CrUserId);
            Assert.Equal(userName, dispatchLine.LcUserId);
            _dispatchLineRepoMock.Verify(r => r.AddAsync(dispatchLine), Times.Once);
        }

        [Fact]
        public async Task
            AddAsync_SetsLineNumber_WhenGetNextLineNumberAsyncProvidedAndNotFromParent()
        {
            // Arrange
            var dispatchLine = _entityFactory.CreateDispatchLine();
            dispatchLine.LineNumber = 0;
            var userName = "testuser";

            var product = _entityFactory.CreateProduct();
            _productRepoMock.Setup(r => r.GetByIdAsync(dispatchLine.ProductId))
                .ReturnsAsync(product);

            int nextLineNumberCalledWith = -1;
            Task<int> FakeNextLineNumber(int id)
            {
                nextLineNumberCalledWith = id;
                return Task.FromResult(42);
            }

            var salesOrderLine = _entityFactory.CreateSalesOrderLine();
            _salesOrderLineRepoMock.Setup(r => r.GetByIdAsync(salesOrderLine.Id)).ReturnsAsync(salesOrderLine);
            var salesOrder = _entityFactory.CreateSalesOrder();
            _salesOrderRepoMock.Setup(r => r.GetByIdAsync(salesOrder.Id)).ReturnsAsync(salesOrder);


            // Act
            await _dispatchLineProcessor.AddAsync(dispatchLine, userName, FakeNextLineNumber);

            // Assert
            Assert.Equal(42, dispatchLine.LineNumber);
            Assert.Equal(dispatchLine.DispatchId, nextLineNumberCalledWith);
        }

        [Fact]
        public async Task AddAsync_DoesNotCallGetNextLineNumber_WhenFromParentTrue()
        {
            // Arrange
            var dispatchLine = _entityFactory.CreateDispatchLine();
            var userName = "testuser";
            bool called = false;
            Task<int> FakeNextLineNumber(int id)
            {
                called = true;
                return Task.FromResult(999);
            }

            var product = _entityFactory.CreateProduct();
            _productRepoMock.Setup(r => r.GetByIdAsync(dispatchLine.ProductId))
                .ReturnsAsync(product);

            var salesOrderLine = _entityFactory.CreateSalesOrderLine();
            _salesOrderLineRepoMock.Setup(r => r.GetByIdAsync(salesOrderLine.Id)).ReturnsAsync(salesOrderLine);
            var salesOrder = _entityFactory.CreateSalesOrder();
            _salesOrderRepoMock.Setup(r => r.GetByIdAsync(salesOrder.Id)).ReturnsAsync(salesOrder);

            // Act
            await _dispatchLineProcessor.AddAsync(dispatchLine, userName, FakeNextLineNumber, fromParent: true);

            // Assert
            Assert.False(called, "getNextLineNumberAsync should not have been called.");
        }

        [Fact]
        public async Task AddAsync_Throws_WhenProductNotFound()
        {
            // Arrange
            var dispatchLine = _entityFactory.CreateDispatchLine();
            var userName = "testuser";

            _productRepoMock.Setup(r => r.GetByIdAsync(dispatchLine.ProductId))
                .ReturnsAsync((Product?)null);

            var salesOrderLine = _entityFactory.CreateSalesOrderLine();
            _salesOrderLineRepoMock.Setup(r => r.GetByIdAsync(salesOrderLine.Id)).ReturnsAsync(salesOrderLine);
            var salesOrder = _entityFactory.CreateSalesOrder();
            _salesOrderRepoMock.Setup(r => r.GetByIdAsync(salesOrder.Id)).ReturnsAsync(salesOrder);

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _dispatchLineProcessor.AddAsync(dispatchLine, userName, null));

            // Assert
            Assert.Contains($"Unable to find product with ID {dispatchLine.ProductId}", ex.Message);
        }

        [Fact]
        public async Task AddAsync_UpdatesProductStockAndFields()
        {
            // Arrange
            var dispatchLine = _entityFactory.CreateDispatchLine();
            dispatchLine.Quantity = 5;
            var userName = "testuser";

            var product = _entityFactory.CreateProduct();
            product.ReservedStock = 50;
            product.TotalStock = 100;
            product.LcDate = new DateTime(2000, 1, 1);
            product.LcUserId = "olduser";

            _productRepoMock.Setup(r => r.GetByIdAsync(dispatchLine.ProductId))
                .ReturnsAsync(product);

            var salesOrderLine = _entityFactory.CreateSalesOrderLine();
            salesOrderLine.Quantity = 10;
            _salesOrderLineRepoMock.Setup(r => r.GetByIdAsync(salesOrderLine.Id)).ReturnsAsync(salesOrderLine);
            var salesOrder = _entityFactory.CreateSalesOrder();
            _salesOrderRepoMock.Setup(r => r.GetByIdAsync(salesOrder.Id)).ReturnsAsync(salesOrder);

            // Act
            await _dispatchLineProcessor.AddAsync(dispatchLine, userName, null);

            // Assert
            Assert.Equal(45, product.ReservedStock);
            Assert.Equal(95, product.TotalStock);
            Assert.Equal(userName, product.LcUserId);
            Assert.True((DateTime.UtcNow - product.LcDate).TotalSeconds < 5);
        }

        [Fact]
        public async Task AddAsync_CreatesStockMovementWithCorrectValues()
        {
            // Arrange
            var dispatchLine = _entityFactory.CreateDispatchLine();
            dispatchLine.Quantity = 5;
            dispatchLine.SalesOrderLineId = 1;
            var userName = "testuser";

            var product = _entityFactory.CreateProduct();
            product.ReservedStock = 10;
            product.TotalStock = 20;
            dispatchLine.Product = product;

            _productRepoMock.Setup(r => r.GetByIdAsync(dispatchLine.ProductId))
                .ReturnsAsync(product);

            StockMovement? capturedMovement = null;
            _stockMovementRepoMock.Setup(r => r.AddAsync(It.IsAny<StockMovement>()))
                .Callback<StockMovement>(sm => capturedMovement = sm)
                .Returns(Task.CompletedTask);

            var salesOrderLine = _entityFactory.CreateSalesOrderLine();
            salesOrderLine.Quantity = 10;
            _salesOrderLineRepoMock.Setup(r => r.GetByIdAsync(salesOrderLine.Id)).ReturnsAsync(salesOrderLine);
            var salesOrder = _entityFactory.CreateSalesOrder();
            _salesOrderRepoMock.Setup(r => r.GetByIdAsync(salesOrder.Id)).ReturnsAsync(salesOrder);

            // Act
            await _dispatchLineProcessor.AddAsync(dispatchLine, userName, null);

            // Assert
            Assert.NotNull(capturedMovement);
            Assert.Equal(product.Id, capturedMovement!.ProductId);
            Assert.Equal(-5, capturedMovement.QuantityChange);
            Assert.Equal("Dispatch", capturedMovement.Reason);
            Assert.Equal(1, capturedMovement.SalesOrderId);
            Assert.Equal(userName, capturedMovement.CrUserId);
            Assert.Equal(userName, capturedMovement.LcUserId);
            Assert.True((DateTime.UtcNow - capturedMovement.CrDate).TotalSeconds < 5);
        }


        [Fact]
        public async Task DeleteAsync_ShouldThrow_WhenDispatchLineNotFound()
        {
            // Arrange
            int id = 42;
            _dispatchLineRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((DispatchLine?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _dispatchLineProcessor.DeleteAsync(id, "testuser"));
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrow_WhenProductNotFound()
        {
            // Arrange
            var dispatchLine = _entityFactory.CreateDispatchLine();
            dispatchLine.Id = 42;
            _dispatchLineRepoMock.Setup(r => r.GetByIdAsync(dispatchLine.Id)).ReturnsAsync(dispatchLine);

            _productRepoMock.Setup(r => r.GetByIdAsync(dispatchLine.ProductId))
                .ReturnsAsync((Product?)null);

            var salesOrderLine = _entityFactory.CreateSalesOrderLine();
            _salesOrderLineRepoMock.Setup(r => r.GetByIdAsync(salesOrderLine.Id)).ReturnsAsync(salesOrderLine);
            var salesOrder = _entityFactory.CreateSalesOrder();
            _salesOrderRepoMock.Setup(r => r.GetByIdAsync(salesOrder.Id)).ReturnsAsync(salesOrder);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _dispatchLineProcessor.DeleteAsync(dispatchLine.Id, "testuser"));
        }

        [Fact]
        public async Task DeleteAsync_ShouldUpdateSOStatusFields_AndAdjustStock_AndDelete_AndCreateStockMovement()
        {
            // Arrange

            var userName = "testuser";

            var dispatchLine = _entityFactory.CreateDispatchLine();
            dispatchLine.Id = 99;
            dispatchLine.Quantity = 5;

            var salesOrderLine = _entityFactory.CreateSalesOrderLine();
            _salesOrderLineRepoMock.Setup(r => r.GetByIdAsync(salesOrderLine.Id)).ReturnsAsync(salesOrderLine);
            var salesOrder = _entityFactory.CreateSalesOrder();
            _salesOrderRepoMock.Setup(r => r.GetByIdAsync(salesOrder.Id)).ReturnsAsync(salesOrder);

            _dispatchLineRepoMock.Setup(r => r.GetByIdAsync(dispatchLine.Id)).ReturnsAsync(dispatchLine);

            var product = _entityFactory.CreateProduct();
            product.ReservedStock = 50;
            product.TotalStock = 100;
            _productRepoMock.Setup(r => r.GetByIdAsync(dispatchLine.ProductId)).ReturnsAsync(product);

            // Act
            await _dispatchLineProcessor.DeleteAsync(dispatchLine.Id, userName);

            // Assert

            // Stock fields updated correctly
            Assert.Equal(55, product.ReservedStock);
            Assert.Equal(105, product.TotalStock);
            Assert.Equal(userName, product.LcUserId);

            // Repository delete called
            _dispatchLineRepoMock.Verify(r => r.DeleteAsync(dispatchLine.Id), Times.Once);

            // Stock movement created
            _stockMovementRepoMock.Verify(r => r.AddAsync(It.Is<StockMovement>(sm =>
                sm.ProductId == product.Id &&
                sm.QuantityChange == dispatchLine.Quantity &&
                sm.Reason == "Deletion of dispatch line" &&
                sm.SalesOrderId == dispatchLine.SalesOrderLine.SalesOrderId &&
                sm.CrUserId == userName &&
                sm.LcUserId == userName
            )), Times.Once);

        }

        [Fact]
        public async Task BlockAsync_ShouldThrow_WhenDispatchLineNotFound()
        {
            // Arrange
            int id = 99;
            _dispatchLineRepoMock.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((DispatchLine?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _dispatchLineProcessor.BlockAsync(id, "tester"));
        }

        [Fact]
        public async Task BlockAsync_ShouldThrow_WhenProductNotFound()
        {
            // Arrange
            var dispatchLine = _entityFactory.CreateDispatchLine();
            dispatchLine.Id = 99;
            _dispatchLineRepoMock.Setup(r => r.GetByIdAsync(dispatchLine.Id)).ReturnsAsync(dispatchLine);

            _productRepoMock.Setup(r => r.GetByIdAsync(dispatchLine.ProductId))
                .ReturnsAsync((Product?)null);

            var salesOrderLine = _entityFactory.CreateSalesOrderLine();
            _salesOrderLineRepoMock.Setup(r => r.GetByIdAsync(salesOrderLine.Id)).ReturnsAsync(salesOrderLine);
            var salesOrder = _entityFactory.CreateSalesOrder();
            _salesOrderRepoMock.Setup(r => r.GetByIdAsync(salesOrder.Id)).ReturnsAsync(salesOrder);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _dispatchLineProcessor.BlockAsync(dispatchLine.Id, "tester"));
        }

        [Fact]
        public async Task BlockAsync_AlreadyBlocked_ShouldNotChangeProductOrCreateStockMovement()
        {
            // Arrange
            var dispatchLine = _entityFactory.CreateDispatchLine();
            dispatchLine.BlDate = DateTime.UtcNow;
            dispatchLine.BlUserId = "test";

            var product = _entityFactory.CreateProduct();
            product.TotalStock = 50;
            var originalStock = product.TotalStock;

            _dispatchLineRepoMock.Setup(r => r.GetByIdAsync(dispatchLine.Id)).ReturnsAsync(dispatchLine);
            _productRepoMock.Setup(r => r.GetByIdAsync(dispatchLine.ProductId))
                .ReturnsAsync(product);
            var salesOrderLine = _entityFactory.CreateSalesOrderLine();
            _salesOrderLineRepoMock.Setup(r => r.GetByIdAsync(salesOrderLine.Id)).ReturnsAsync(salesOrderLine);
            var salesOrder = _entityFactory.CreateSalesOrder();
            _salesOrderRepoMock.Setup(r => r.GetByIdAsync(salesOrder.Id)).ReturnsAsync(salesOrder);

            // Act
            await _dispatchLineProcessor.BlockAsync(dispatchLine.Id, "test");

            // Assert
            Assert.Equal(originalStock, product.TotalStock);
            _stockMovementRepoMock.Verify(r => r.AddAsync(It.IsAny<StockMovement>()), Times.Never);    
        }

        [Fact]
        public async Task BlockAsync_ShouldUpdateStock_AndUpdateDispatchLine_AndAddStockMovement()
        {
            // Arrange
            var dispatchLine = _entityFactory.CreateDispatchLine();
            dispatchLine.Id = 99;
            dispatchLine.Quantity = 5;

            var salesOrderLine = _entityFactory.CreateSalesOrderLine();
            _salesOrderLineRepoMock.Setup(r => r.GetByIdAsync(salesOrderLine.Id)).ReturnsAsync(salesOrderLine);
            var salesOrder = _entityFactory.CreateSalesOrder();
            _salesOrderRepoMock.Setup(r => r.GetByIdAsync(salesOrder.Id)).ReturnsAsync(salesOrder);

            _dispatchLineRepoMock.Setup(r => r.GetByIdAsync(dispatchLine.Id)).ReturnsAsync(dispatchLine);

            var product = _entityFactory.CreateProduct();
            product.ReservedStock = 50;
            product.TotalStock = 100;
            _productRepoMock.Setup(r => r.GetByIdAsync(dispatchLine.ProductId)).ReturnsAsync(product);

            // Act
            await _dispatchLineProcessor.BlockAsync(dispatchLine.Id, "tester");

            // Assert

            // Dispatch line should have BlDate and BlUserId set
            _dispatchLineRepoMock.Verify(r => r.UpdateAsync(It.Is<DispatchLine>(dl =>
                dl.Id == dispatchLine.Id &&
                dl.BlUserId == "tester" &&
                dl.BlDate.HasValue
            )), Times.Once);

            // Product stock should be updated
            Assert.Equal(55, product.ReservedStock);
            Assert.Equal(105, product.TotalStock);
            Assert.Equal("tester", product.LcUserId);

            // Stock movement added
            _stockMovementRepoMock.Verify(r => r.AddAsync(It.Is<StockMovement>(sm =>
                sm.ProductId == product.Id &&
                sm.QuantityChange == dispatchLine.Quantity &&
                sm.Reason == "Blocking of dispatch line" &&
                sm.SalesOrderId == dispatchLine.SalesOrderLine.SalesOrderId &&
                sm.CrUserId == "tester" &&
                sm.LcUserId == "tester"
            )), Times.Once);
        }

        [Fact]
        public async Task UnblockAsync_ShouldUpdateFieldsAndCreateStockMovement()
        {
            // Arrange
            var userName = "testuser";

            var dispatchLine = _entityFactory.CreateDispatchLine();
            dispatchLine.BlDate = DateTime.UtcNow;
            dispatchLine.BlUserId = "test";
            var oldProduct = _entityFactory.CreateProduct();
            var salesOrderLine = _entityFactory.CreateSalesOrderLine();
            var salesOrder = _entityFactory.CreateSalesOrder();

            var product = _entityFactory.CreateProduct();

            _dispatchLineRepoMock.Setup(r => r.GetByIdAsync(dispatchLine.Id)).ReturnsAsync(dispatchLine);
            _productRepoMock.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);
            _salesOrderLineRepoMock.Setup(r => r.GetByIdAsync(salesOrderLine.Id)).ReturnsAsync(salesOrderLine);
            _salesOrderRepoMock.Setup(r => r.GetByIdAsync(salesOrder.Id)).ReturnsAsync(salesOrder);

            // Act
            await _dispatchLineProcessor.UnblockAsync(dispatchLine.Id, userName);
            

            // Assert: dispatchLine fields updated
            _dispatchLineRepoMock.Verify(r => r.UpdateAsync(It.Is<DispatchLine>(d =>
                d.Id == dispatchLine.Id &&
                d.BlDate == null &&
                d.BlUserId == null &&
                d.LcUserId == userName &&
                d.LcDate <= DateTime.UtcNow
            )), Times.Once);

            // Assert: product fields updated
            Assert.Equal(oldProduct.ReservedStock - dispatchLine.Quantity, product.ReservedStock);
            Assert.Equal(oldProduct.TotalStock - dispatchLine.Quantity, product.TotalStock);
            Assert.Equal(userName, product.LcUserId);
            Assert.True(product.LcDate <= DateTime.UtcNow);

            // Assert: stock movement created
            _stockMovementRepoMock.Verify(r => r.AddAsync(It.Is<StockMovement>(sm =>
                sm.ProductId == product.Id &&
                sm.QuantityChange == -dispatchLine.Quantity &&
                sm.Reason == "Unblocking of dispatch line" &&
                sm.CrUserId == userName &&
                sm.LcUserId == userName &&
                sm.SalesOrderId == salesOrder.Id
            )), Times.Once);
        }

        [Fact]
        public async Task UnblockAsync_Notblocked_ShouldNotChangeProductOrCreateStockMovement()
        {
            // Arrange
            var dispatchLine = _entityFactory.CreateDispatchLine();
            dispatchLine.BlDate = null;
            dispatchLine.BlUserId = null;

            var product = _entityFactory.CreateProduct();
            product.TotalStock = 50;
            var originalStock = product.TotalStock;

            _dispatchLineRepoMock.Setup(r => r.GetByIdAsync(dispatchLine.Id)).ReturnsAsync(dispatchLine);
            _productRepoMock.Setup(r => r.GetByIdAsync(dispatchLine.ProductId))
                .ReturnsAsync(product);
            var salesOrderLine = _entityFactory.CreateSalesOrderLine();
            _salesOrderLineRepoMock.Setup(r => r.GetByIdAsync(salesOrderLine.Id)).ReturnsAsync(salesOrderLine);
            var salesOrder = _entityFactory.CreateSalesOrder();
            _salesOrderRepoMock.Setup(r => r.GetByIdAsync(salesOrder.Id)).ReturnsAsync(salesOrder);

            // Act
            await _dispatchLineProcessor.UnblockAsync(dispatchLine.Id, "test");

            // Assert
            Assert.Equal(originalStock, product.TotalStock);
            _stockMovementRepoMock.Verify(r => r.AddAsync(It.IsAny<StockMovement>()), Times.Never);
        }

        [Fact]
        public async Task UnblockAsync_ShouldThrowIfDispatchLineNotFound()
        {
            // Arrange
            _dispatchLineRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((DispatchLine?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _dispatchLineProcessor.UnblockAsync(999, "testuser"));
        }

        [Fact]
        public async Task UnblockAsync_ShouldThrowIfProductNotFound()
        {
            // Arrange
            var dispatchLine = _entityFactory.CreateDispatchLine();
            _dispatchLineRepoMock.Setup(r => r.GetByIdAsync(dispatchLine.Id)).ReturnsAsync(dispatchLine);
            _productRepoMock.Setup(r => r.GetByIdAsync(dispatchLine.ProductId))
                .ReturnsAsync((Product?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _dispatchLineProcessor.UnblockAsync(66, "testuser"));
        }
    }
}
