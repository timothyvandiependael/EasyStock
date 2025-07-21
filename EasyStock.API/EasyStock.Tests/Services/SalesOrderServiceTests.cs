using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;
using EasyStock.API.Services;
using EasyStock.Tests.TestHelpers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EasyStock.Tests.Services
{
    public class SalesOrderServiceTests
    {
        private readonly EntityFactory _entityFactory = new EntityFactory();

        private readonly Mock<ISalesOrderRepository> _mockSalesOrderRepository = new();
        private readonly Mock<IRetryableTransactionService> _mockRetryableTransactionService = new();
        private readonly Mock<IOrderNumberCounterService> _mockOrderNumberCounterService = new();
        private readonly Mock<IRepository<SalesOrder>> _mockRepository = new();
        private readonly Mock<ISalesOrderLineProcessor> _mockSalesOrderLineProcessor = new();

        private SalesOrderService CreateService()
        {
            return new SalesOrderService(
                _mockSalesOrderRepository.Object,
                _mockRetryableTransactionService.Object,
                _mockOrderNumberCounterService.Object,
                _mockRepository.Object,
                _mockSalesOrderLineProcessor.Object);
        }

        [Fact]
        public async Task AddAsync_WithoutTransaction_ShouldAddSalesOrderAndLines()
        {
            // Arrange
            var service = CreateService();
            var userName = "testUser";

            var salesOrder = _entityFactory.CreateSalesOrder();
            var line1 = _entityFactory.CreateSalesOrderLine();
            var line2 = _entityFactory.CreateSalesOrderLine();
            salesOrder.Lines.Add(line1);
            salesOrder.Lines.Add(line2);

            _mockOrderNumberCounterService.Setup(x => x.GenerateOrderNumberAsync(OrderType.SalesOrder)).ReturnsAsync("123");
            _mockSalesOrderLineProcessor.Setup(x => x.AddAsync(It.IsAny<SalesOrderLine>(), userName, null, true))
                .Returns(Task.CompletedTask);
            _mockRepository.Setup(x => x.AddAsync(It.IsAny<SalesOrder>())).Returns(Task.CompletedTask);

            // Act
            await service.AddAsync(salesOrder, userName, useTransaction: false);

            // Assert
            _mockOrderNumberCounterService.Verify(x => x.GenerateOrderNumberAsync(OrderType.SalesOrder), Times.Once);
            _mockSalesOrderLineProcessor.Verify(x => x.AddAsync(It.IsAny<SalesOrderLine>(), userName, null, true), Times.Exactly(2));
            _mockRepository.Verify(x => x.AddAsync(It.Is<SalesOrder>(so =>
                so.OrderNumber == "123" &&
                so.Status == OrderStatus.Open &&
                so.CrUserId == userName &&
                so.LcUserId == userName &&
                so.Lines.Count == 0
            )), Times.Once);
        }

        [Fact]
        public async Task AddAsync_ShouldSetLineNumbersSequentially()
        {
            // Arrange
            var service = CreateService();
            var userName = "user";
            var so = _entityFactory.CreateSalesOrder();
            var line1 = _entityFactory.CreateSalesOrderLine();
            var line2 = _entityFactory.CreateSalesOrderLine();
            so.Lines.Add(line1);
            so.Lines.Add(line2);

            _mockOrderNumberCounterService.Setup(x => x.GenerateOrderNumberAsync(OrderType.SalesOrder)).ReturnsAsync("1");
            _mockSalesOrderLineProcessor.Setup(x => x.AddAsync(It.IsAny<SalesOrderLine>(), userName, null, true)).Returns(Task.CompletedTask);
            _mockRepository.Setup(x => x.AddAsync(It.IsAny<SalesOrder>())).Returns(Task.CompletedTask);

            // Act
            await service.AddAsync(so, userName, useTransaction: false);

            // Assert
            Assert.Equal(1, line1.LineNumber);
            Assert.Equal(2, line2.LineNumber);
        }

        [Fact]
        public async Task DeleteAsync_WithoutTransaction_ExistingOrder_ShouldDeleteLinesAndOrder()
        {
            // Arrange
            var service = CreateService();
            var userName = "deleter";
            var so = _entityFactory.CreateSalesOrder();
            var line1 = _entityFactory.CreateSalesOrderLine();
            var line2 = _entityFactory.CreateSalesOrderLine();
            line2.Id = 2;
            so.Lines.Add(line1);
            so.Lines.Add(line2);

            _mockRepository.Setup(x => x.GetByIdAsync(so.Id)).ReturnsAsync(so);
            _mockSalesOrderLineProcessor.Setup(x => x.DeleteAsync(It.IsAny<int>(), userName)).Returns(Task.CompletedTask);
            _mockRepository.Setup(x => x.DeleteAsync(so.Id)).Returns(Task.CompletedTask);

            // Act
            await service.DeleteAsync(so.Id, userName, useTransaction: false);

            // Assert
            _mockSalesOrderLineProcessor.Verify(x => x.DeleteAsync(line1.Id, userName), Times.Once);
            _mockSalesOrderLineProcessor.Verify(x => x.DeleteAsync(line2.Id, userName), Times.Once);
            _mockRepository.Verify(x => x.DeleteAsync(so.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_OrderDoesNotExist_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var service = CreateService();
            _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((SalesOrder)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(1, "user", useTransaction: false));
        }

        [Fact]
        public async Task BlockAsync_WithoutTransaction_ExistingOrder_ShouldSetBlockPropertiesAndBlockLines()
        {
            // Arrange
            var service = CreateService();
            var userName = "blocker";
            var so = _entityFactory.CreateSalesOrder();
            var line1 = _entityFactory.CreateSalesOrderLine();
            var line2 = _entityFactory.CreateSalesOrderLine();
            line2.Id = 2;
            so.Lines.Add(line1);
            so.Lines.Add(line2);

            _mockRepository.Setup(x => x.GetByIdAsync(so.Id)).ReturnsAsync(so);
            _mockSalesOrderLineProcessor.Setup(x => x.BlockAsync(It.IsAny<int>(), userName)).Returns(Task.CompletedTask);
            _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<SalesOrder>())).Returns(Task.CompletedTask);

            // Act
            await service.BlockAsync(so.Id, userName, useTransaction: false);

            // Assert
            Assert.NotNull(so.BlDate);
            Assert.Equal(userName, so.BlUserId);
            _mockSalesOrderLineProcessor.Verify(x => x.BlockAsync(line1.Id, userName), Times.Once);
            _mockSalesOrderLineProcessor.Verify(x => x.BlockAsync(line2.Id, userName), Times.Once);
            _mockRepository.Verify(x => x.UpdateAsync(so), Times.Once);
        }

        [Fact]
        public async Task BlockAsync_OrderDoesNotExist_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var service = CreateService();
            _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((SalesOrder)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.BlockAsync(1, "user", useTransaction: false));
        }

        [Fact]
        public async Task UnblockAsync_WithoutTransaction_ExistingOrder_ShouldUnsetBlockPropertiesAndUnblockLines()
        {
            // Arrange
            var service = CreateService();
            var userName = "unblocker";
            var so = _entityFactory.CreateSalesOrder();
            var line1 = _entityFactory.CreateSalesOrderLine();
            var line2 = _entityFactory.CreateSalesOrderLine();
            line2.Id = 2;
            so.Lines.Add(line1);
            so.Lines.Add(line2);
            so.BlDate = DateTime.UtcNow;
            so.BlUserId = "someone";

            _mockRepository.Setup(x => x.GetByIdAsync(so.Id)).ReturnsAsync(so);
            _mockSalesOrderLineProcessor.Setup(x => x.UnblockAsync(It.IsAny<int>(), userName)).Returns(Task.CompletedTask);
            _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<SalesOrder>())).Returns(Task.CompletedTask);

            // Act
            await service.UnblockAsync(so.Id, userName, useTransaction: false);

            // Assert
            Assert.Null(so.BlDate);
            Assert.Null(so.BlUserId);
            Assert.Equal(userName, so.LcUserId);
            Assert.True(so.LcDate <= DateTime.UtcNow && so.LcDate > DateTime.UtcNow.AddMinutes(-1)); // roughly now

            _mockSalesOrderLineProcessor.Verify(x => x.UnblockAsync(line1.Id, userName), Times.Once);
            _mockSalesOrderLineProcessor.Verify(x => x.UnblockAsync(line2.Id, userName), Times.Once);
            _mockRepository.Verify(x => x.UpdateAsync(so), Times.Once);
        }

        [Fact]
        public async Task UnblockAsync_OrderDoesNotExist_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var service = CreateService();
            _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((SalesOrder)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UnblockAsync(1, "user", useTransaction: false));
        }

        [Fact]
        public async Task GetProductsWithSuppliersForOrderAsync_ExistingOrder_ShouldReturnProductList()
        {
            // Arrange
            var service = CreateService();
            var so = _entityFactory.CreateSalesOrder();
            var product1 = _entityFactory.CreateProduct();
            var product2 = _entityFactory.CreateProduct();

            var line1 = _entityFactory.CreateSalesOrderLine();
            line1.Product = product1;
            var line2 = _entityFactory.CreateSalesOrderLine();
            line2.Product = product2;

            so.Lines.Add(line1);
            so.Lines.Add(line2);

            _mockRepository.Setup(x => x.GetByIdAsync(so.Id)).ReturnsAsync(so);

            // Act
            var products = await service.GetProductsWithSuppliersForOrderAsync(so.Id);

            // Assert
            Assert.Contains(product1, products);
            Assert.Contains(product2, products);
            Assert.Equal(2, products.Count);
        }

        [Fact]
        public async Task GetProductsWithSuppliersForOrderAsync_OrderDoesNotExist_ShouldThrowException()
        {
            // Arrange
            var service = CreateService();
            _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((SalesOrder)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.GetProductsWithSuppliersForOrderAsync(1));
            Assert.Contains("not found", ex.Message);
        }

        [Fact]
        public async Task GetNextLineNumberAsync_WithLines_ShouldReturnMaxPlusOne()
        {
            // Arrange
            var service = CreateService();
            var so = _entityFactory.CreateSalesOrder();

            var line1 = _entityFactory.CreateSalesOrderLine();
            line1.LineNumber = 1;
            var line2 = _entityFactory.CreateSalesOrderLine();
            line2.LineNumber = 3;
            so.Lines.Add(line1);
            so.Lines.Add(line2);

            _mockRepository.Setup(x => x.GetByIdAsync(so.Id)).ReturnsAsync(so);

            // Act
            var nextLineNumber = await service.GetNextLineNumberAsync(so.Id);

            // Assert
            Assert.Equal(4, nextLineNumber);
        }

        [Fact]
        public async Task GetNextLineNumberAsync_NoLines_ShouldReturnOne()
        {
            // Arrange
            var service = CreateService();
            var so = _entityFactory.CreateSalesOrder();

            _mockRepository.Setup(x => x.GetByIdAsync(so.Id)).ReturnsAsync(so);

            // Act
            var nextLineNumber = await service.GetNextLineNumberAsync(so.Id);

            // Assert
            Assert.Equal(1, nextLineNumber);
        }

        [Fact]
        public async Task GetNextLineNumberAsync_OrderDoesNotExist_ShouldThrowException()
        {
            // Arrange
            var service = CreateService();
            _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((SalesOrder)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.GetNextLineNumberAsync(1));
            Assert.Contains("not found", ex.Message);
        }
    }
}
