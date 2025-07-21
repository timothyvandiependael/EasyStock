using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using EasyStock.API.Models;
using EasyStock.API.Services;
using EasyStock.API.Common;
using EasyStock.API.Repositories;
using System.Collections.Generic;
using EasyStock.Tests.TestHelpers;

namespace EasyStock.Tests.Services
{
    public class SalesOrderLineServiceTests
    {
        private readonly EntityFactory entityFactory = new();

        private readonly Mock<IRepository<SalesOrderLine>> mockSalesOrderLineRepository = new();
        private readonly Mock<IRepository<Product>> mockProductRepository = new();
        private readonly Mock<ISalesOrderLineRepository> mockSalesOrderLineRepoAdvanced = new();
        private readonly Mock<IRetryableTransactionService> mockTransactionService = new();
        private readonly Mock<ISalesOrderService> mockSalesOrderService = new();
        private readonly Mock<ISalesOrderLineProcessor> mockSalesOrderLineProcessor = new();

        public SalesOrderLineService CreateService()
        {
            return new SalesOrderLineService(
                mockSalesOrderLineRepoAdvanced.Object,
                mockTransactionService.Object,
                mockSalesOrderLineRepository.Object,
                mockProductRepository.Object,
                mockSalesOrderService.Object,
                mockSalesOrderLineProcessor.Object);
        }

        [Fact]
        public async Task UpdateAsync_QuantityIncreased_ShouldUpdateReservedAndAvailableStock()
        {
            // Arrange
            var service = CreateService();

            var oldLine = entityFactory.CreateSalesOrderLine();
            oldLine.Id = 1;
            oldLine.Quantity = 5;
            oldLine.Status = OrderStatus.Open;
            oldLine.ProductId = 1;

            var updatedLine = entityFactory.CreateSalesOrderLine();
            updatedLine.Id = 1;
            updatedLine.Quantity = 8; // increased by 3
            updatedLine.Status = OrderStatus.Open;
            updatedLine.ProductId = 1;

            var product = entityFactory.CreateProduct();
            product.Id = 1;
            product.AvailableStock = 10;
            product.ReservedStock = 2;
            product.BackOrderedStock = 0;

            mockSalesOrderLineRepository
                .Setup(r => r.GetByIdAsync(updatedLine.Id))
                .ReturnsAsync(oldLine);

            mockProductRepository
                .Setup(r => r.GetByIdAsync(updatedLine.ProductId))
                .ReturnsAsync(product);

            mockSalesOrderLineRepository
                .Setup(r => r.AddAsync(It.IsAny<SalesOrderLine>()))
                .Returns(Task.CompletedTask);

            // Act
            await service.UpdateAsync(updatedLine, "user", useTransaction: false);

            // Assert
            Assert.Equal(5, product.ReservedStock);
            Assert.Equal(7, product.AvailableStock);

            mockSalesOrderLineRepository.Verify(r => r.AddAsync(updatedLine), Times.Once);
            mockSalesOrderLineRepository.Verify(r => r.GetByIdAsync(updatedLine.Id), Times.Once);
            mockProductRepository.Verify(r => r.GetByIdAsync(updatedLine.ProductId), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_QuantityDecreased_ShouldUpdateReservedAndAvailableStock()
        {
            // Arrange
            var service = CreateService();

            var oldLine = entityFactory.CreateSalesOrderLine();
            oldLine.Id = 1;
            oldLine.Quantity = 8;
            oldLine.Status = OrderStatus.Partial;
            oldLine.ProductId = 1;

            var updatedLine = entityFactory.CreateSalesOrderLine();
            updatedLine.Id = 1;
            updatedLine.Quantity = 5; // decreased by 3
            updatedLine.Status = OrderStatus.Partial;
            updatedLine.ProductId = 1;

            var product = entityFactory.CreateProduct();
            product.Id = 1;
            product.ReservedStock = 10;
            product.AvailableStock = 2;
            product.BackOrderedStock = 0;

            mockSalesOrderLineRepository
                .Setup(r => r.GetByIdAsync(updatedLine.Id))
                .ReturnsAsync(oldLine);

            mockProductRepository
                .Setup(r => r.GetByIdAsync(updatedLine.ProductId))
                .ReturnsAsync(product);

            mockSalesOrderLineRepository
                .Setup(r => r.AddAsync(It.IsAny<SalesOrderLine>()))
                .Returns(Task.CompletedTask);

            // Act
            await service.UpdateAsync(updatedLine, "user", useTransaction: false);

            // Assert
            Assert.Equal(7, product.ReservedStock); // 10 - 3
            Assert.Equal(5, product.AvailableStock); // 2 + 3

            mockSalesOrderLineRepository.Verify(r => r.AddAsync(updatedLine), Times.Once);
            mockSalesOrderLineRepository.Verify(r => r.GetByIdAsync(updatedLine.Id), Times.Once);
            mockProductRepository.Verify(r => r.GetByIdAsync(updatedLine.ProductId), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_QuantityDecreased_WithBackOrderedStock_ShouldAdjustBackOrderedStockCorrectly()
        {
            // Arrange
            var service = CreateService();

            var oldLine = entityFactory.CreateSalesOrderLine();
            oldLine.Id = 1;
            oldLine.Quantity = 10;
            oldLine.Status = OrderStatus.Open;
            oldLine.ProductId = 1;

            var updatedLine = entityFactory.CreateSalesOrderLine();
            updatedLine.Id = 1;
            updatedLine.Quantity = 6; // decreased by 4
            updatedLine.Status = OrderStatus.Open;
            updatedLine.ProductId = 1;

            var product = entityFactory.CreateProduct();
            product.Id = 1;
            product.ReservedStock = 10;
            product.AvailableStock = 0;
            product.BackOrderedStock = 3;

            mockSalesOrderLineRepository
                .Setup(r => r.GetByIdAsync(updatedLine.Id))
                .ReturnsAsync(oldLine);

            mockProductRepository
                .Setup(r => r.GetByIdAsync(updatedLine.ProductId))
                .ReturnsAsync(product);

            mockSalesOrderLineRepository
                .Setup(r => r.AddAsync(It.IsAny<SalesOrderLine>()))
                .Returns(Task.CompletedTask);

            // Act
            await service.UpdateAsync(updatedLine, "user", useTransaction: false);

            // Assert
            Assert.Equal(9, product.ReservedStock); // 10 - 1
            Assert.Equal(1, product.AvailableStock); // 0 + 1
            Assert.Equal(0, product.BackOrderedStock);

            mockSalesOrderLineRepository.Verify(r => r.AddAsync(updatedLine), Times.Once);
            mockSalesOrderLineRepository.Verify(r => r.GetByIdAsync(updatedLine.Id), Times.Once);
            mockProductRepository.Verify(r => r.GetByIdAsync(updatedLine.ProductId), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_QuantityUnchanged_ShouldNotChangeProductStocks()
        {
            // Arrange
            var service = CreateService();

            var oldLine = entityFactory.CreateSalesOrderLine();
            oldLine.Id = 1;
            oldLine.Quantity = 5;
            oldLine.Status = OrderStatus.Open;
            oldLine.ProductId = 1;

            var updatedLine = entityFactory.CreateSalesOrderLine();
            updatedLine.Id = 1;
            updatedLine.Quantity = 5; // same quantity
            updatedLine.Status = OrderStatus.Open;
            updatedLine.ProductId = 1;

            mockSalesOrderLineRepository
                .Setup(r => r.GetByIdAsync(updatedLine.Id))
                .ReturnsAsync(oldLine);

            mockSalesOrderLineRepository
                .Setup(r => r.AddAsync(It.IsAny<SalesOrderLine>()))
                .Returns(Task.CompletedTask);

            // Act
            await service.UpdateAsync(updatedLine, "user", useTransaction: false);

            // Assert
            mockProductRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
            mockSalesOrderLineRepository.Verify(r => r.AddAsync(updatedLine), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_OldRecordNotFound_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var service = CreateService();

            var updatedLine = entityFactory.CreateSalesOrderLine();
            updatedLine.Id = 1;

            mockSalesOrderLineRepository
                .Setup(r => r.GetByIdAsync(updatedLine.Id))
                .ReturnsAsync((SalesOrderLine?)null);

            mockSalesOrderLineRepository
                .Setup(r => r.AddAsync(It.IsAny<SalesOrderLine>()))
                .Returns(Task.CompletedTask);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateAsync(updatedLine, "user", useTransaction: false));

            Assert.Contains("Sales order line with ID", ex.Message);
        }

        [Fact]
        public async Task UpdateAsync_ProductNotFound_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var service = CreateService();

            var oldLine = entityFactory.CreateSalesOrderLine();
            oldLine.Id = 1;
            oldLine.Quantity = 5;
            oldLine.Status = OrderStatus.Open;
            oldLine.ProductId = 1;

            var updatedLine = entityFactory.CreateSalesOrderLine();
            updatedLine.Id = 1;
            updatedLine.Quantity = 10; // increased quantity
            updatedLine.Status = OrderStatus.Open;
            updatedLine.ProductId = 1;

            mockSalesOrderLineRepository
                .Setup(r => r.GetByIdAsync(updatedLine.Id))
                .ReturnsAsync(oldLine);

            mockSalesOrderLineRepository
                .Setup(r => r.AddAsync(It.IsAny<SalesOrderLine>()))
                .Returns(Task.CompletedTask);

            mockProductRepository
                .Setup(r => r.GetByIdAsync(updatedLine.ProductId))
                .ReturnsAsync((Product?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateAsync(updatedLine, "user", useTransaction: false));

            Assert.Contains("Product with ID", ex.Message);
        }

    }
}
