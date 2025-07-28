using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyStock.API.Models;
using EasyStock.API.Repositories;
using EasyStock.API.Services;
using EasyStock.Tests.TestHelpers;
using Moq;
using Xunit;

namespace EasyStock.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _productRepositoryMock = new();
        private readonly Mock<IRepository<Product>> _productRepoMock = new();
        private readonly Mock<IRetryableTransactionService> _transactionServiceMock = new();
        private readonly Mock<IRepository<StockMovement>> _stockMovementRepoMock = new();
        private readonly Mock<IUpdateService<Product>> _updateServiceMock = new();

        private readonly IProductService _service;

        private readonly EntityFactory _entityFactory;

        public ProductServiceTests()
        {
            _service = new ProductService(
                _productRepositoryMock.Object,
                _productRepoMock.Object,
                _transactionServiceMock.Object,
                _stockMovementRepoMock.Object,
                _updateServiceMock.Object
            );

            _entityFactory = new EntityFactory();
        }

        [Fact]
        public async Task IsProductBelowMinimumStock_ShouldReturnTrue_WhenBelow()
        {
            // Arrange
            var product = _entityFactory.CreateProduct();
            product.TotalStock = 10;
            product.AvailableStock = 2;
            product.MinimumStock = 5;
            _productRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(product);

            // Act
            var result = await _service.IsProductBelowMinimumStock(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsProductBelowMinimumStock_ShouldThrow_WhenNotFound()
        {
            // Arrange
            _productRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Product?)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.IsProductBelowMinimumStock(1));
        }

        [Fact]
        public async Task UpdateAsync_ShouldAdjustStocks_AndCreateStockMovement()
        {
            // Arrange
            var oldProduct = _entityFactory.CreateProduct();
            oldProduct.TotalStock = 100;
            oldProduct.AvailableStock = 100;
            var updatedProduct = _entityFactory.CreateProduct();
            updatedProduct.TotalStock = 120;
            updatedProduct.AvailableStock = 100;

            _productRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(oldProduct);

            _stockMovementRepoMock.Setup(r => r.AddAsync(It.IsAny<StockMovement>()))
                .Returns(Task.CompletedTask);

            _productRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            _updateServiceMock.Setup(r => r.MapAndUpdateAuditFields(oldProduct, updatedProduct, "tester")).Returns(oldProduct);

            // Act
            await _service.UpdateAsync(updatedProduct, "tester", useTransaction: false);

            // Assert: AvailableStock decreased by the difference
            Assert.Equal(80, updatedProduct.AvailableStock);

            // Assert: Stock movement was recorded
            _stockMovementRepoMock.Verify(r => r.AddAsync(It.Is<StockMovement>(
                m => m.ProductId == 1 && m.QuantityChange == 20 && m.CrUserId == "tester")), Times.Once);

        }

        [Fact]
        public async Task UpdateAsync_ShouldHandleNegativeAvailableStock()
        {
            var oldProduct = _entityFactory.CreateProduct();
            oldProduct.TotalStock = 50;
            oldProduct.AvailableStock = 50;
            var updatedProduct = _entityFactory.CreateProduct(); 
            updatedProduct.TotalStock = 100;
            updatedProduct.AvailableStock = 10;

            _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(oldProduct);

            // Act
            await _service.UpdateAsync(updatedProduct, "tester", useTransaction: false);

            // Assert: AvailableStock is zero, BackOrderedStock increased
            Assert.Equal(0, updatedProduct.AvailableStock);
            Assert.True(updatedProduct.BackOrderedStock > 0);
        }
    }
}
