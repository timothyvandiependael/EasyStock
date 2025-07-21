using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyStock.API.Models;
using EasyStock.API.Services;
using EasyStock.API.Common;
using EasyStock.API.Repositories;
using Moq;
using Xunit;
using EasyStock.Tests.TestHelpers;

namespace EasyStock.Tests.Services
{
    public class ReceptionServiceTests
    {
        private readonly EntityFactory _entityFactory = new EntityFactory();

        private readonly Mock<IReceptionRepository> _mockReceptionRepository = new();
        private readonly Mock<IOrderNumberCounterService> _mockOrderNumberCounterService = new();
        private readonly Mock<IRepository<Reception>> _mockGenericReceptionRepository = new();
        private readonly Mock<IRetryableTransactionService> _mockRetryableTransactionService = new();
        private readonly Mock<IReceptionLineProcessor> _mockReceptionLineProcessor = new();

        private ReceptionService CreateService()
        {
            return new ReceptionService(
                _mockReceptionRepository.Object,
                _mockOrderNumberCounterService.Object,
                _mockGenericReceptionRepository.Object,
                _mockRetryableTransactionService.Object,
                _mockReceptionLineProcessor.Object);
        }

        [Fact]
        public async Task AddAsync_ShouldSetFieldsAndAddLinesAndReception()
        {
            // Arrange
            var reception = _entityFactory.CreateReception();
            var lines = new List<ReceptionLine>
        {
            _entityFactory.CreateReceptionLine(),
            _entityFactory.CreateReceptionLine()
        };
            reception.Lines = lines;

            _mockOrderNumberCounterService.Setup(s => s.GenerateOrderNumberAsync(OrderType.Reception))
                .Returns(Task.FromResult("123"));

            _mockReceptionLineProcessor.Setup(p => p.AddAsync(It.IsAny<ReceptionLine>(), It.IsAny<string>(), null, true))
                .Returns(Task.CompletedTask);

            _mockGenericReceptionRepository.Setup(r => r.AddAsync(reception)).Returns(Task.CompletedTask);

            var service = CreateService();
            var userName = "testUser";

            // Act
            await service.AddAsync(reception, userName, useTransaction: false);

            // Assert
            Assert.Equal("123", reception.ReceptionNumber);
            Assert.NotEqual(default, reception.CrDate);
            Assert.NotEqual(default, reception.LcDate);
            Assert.Equal(userName, reception.CrUserId);
            Assert.Equal(userName, reception.LcUserId);

            Assert.Empty(reception.Lines); // Lines cleared after processing

            _mockReceptionLineProcessor.Verify(p => p.AddAsync(It.IsAny<ReceptionLine>(), userName, null, true), Times.Exactly(2));
            _mockGenericReceptionRepository.Verify(r => r.AddAsync(reception), Times.Once);
        }

        [Fact]
        public async Task AddAsync_ShouldThrowExceptionIfAddFails()
        {
            // Arrange
            var reception = _entityFactory.CreateReception();
            reception.Lines = new List<ReceptionLine>();

            _mockOrderNumberCounterService.Setup(s => s.GenerateOrderNumberAsync(OrderType.Reception))
                .ThrowsAsync(new Exception("Fail"));

            var service = CreateService();
            var userName = "user";

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => service.AddAsync(reception, userName, false));
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteLinesAndReception()
        {
            // Arrange
            var reception = _entityFactory.CreateReception();
            var line1 = _entityFactory.CreateReceptionLine();
            line1.Id = 1;
            var line2 = _entityFactory.CreateReceptionLine();
            line2.Id = 2;
            reception.Lines = new List<ReceptionLine> { line1, line2 };

            _mockGenericReceptionRepository.Setup(r => r.GetByIdAsync(reception.Id)).ReturnsAsync(reception);
            _mockReceptionLineProcessor.Setup(p => p.DeleteAsync(It.IsAny<int>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            _mockGenericReceptionRepository.Setup(r => r.DeleteAsync(reception.Id)).Returns(Task.CompletedTask);

            var service = CreateService();
            var userName = "user";

            // Act
            await service.DeleteAsync(reception.Id, userName, useTransaction: false);

            // Assert
            _mockReceptionLineProcessor.Verify(p => p.DeleteAsync(line1.Id, userName), Times.Once);
            _mockReceptionLineProcessor.Verify(p => p.DeleteAsync(line2.Id, userName), Times.Once);
            _mockGenericReceptionRepository.Verify(r => r.DeleteAsync(reception.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowIfReceptionNotFound()
        {
            // Arrange
            _mockGenericReceptionRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Reception)null);
            var service = CreateService();
            var userName = "user";

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(1, userName, false));
        }

        [Fact]
        public async Task BlockAsync_ShouldSetBlockFieldsAndBlockLines()
        {
            // Arrange
            var reception = _entityFactory.CreateReception();
            reception.BlDate = null; // not blocked yet
            var line = _entityFactory.CreateReceptionLine();
            reception.Lines = new List<ReceptionLine> { line };

            _mockGenericReceptionRepository.Setup(r => r.GetByIdAsync(reception.Id)).ReturnsAsync(reception);
            _mockReceptionLineProcessor.Setup(p => p.BlockAsync(line.Id, It.IsAny<string>())).Returns(Task.CompletedTask);
            _mockGenericReceptionRepository.Setup(r => r.UpdateAsync(reception)).Returns(Task.CompletedTask);

            var service = CreateService();
            var userName = "blocker";

            // Act
            await service.BlockAsync(reception.Id, userName, useTransaction: false);

            // Assert
            Assert.NotNull(reception.BlDate);
            Assert.Equal(userName, reception.BlUserId);
            _mockReceptionLineProcessor.Verify(p => p.BlockAsync(line.Id, userName), Times.Once);
            _mockGenericReceptionRepository.Verify(r => r.UpdateAsync(reception), Times.Once);
        }

        [Fact]
        public async Task BlockAsync_ShouldNotChangeIfAlreadyBlocked()
        {
            // Arrange
            var reception = _entityFactory.CreateReception();
            reception.BlDate = DateTime.UtcNow;
            reception.BlUserId = "someone";
            var line = _entityFactory.CreateReceptionLine();
            reception.Lines = new List<ReceptionLine> { line };

            _mockGenericReceptionRepository.Setup(r => r.GetByIdAsync(reception.Id)).ReturnsAsync(reception);
            _mockReceptionLineProcessor.Setup(p => p.BlockAsync(line.Id, It.IsAny<string>())).Returns(Task.CompletedTask);
            _mockGenericReceptionRepository.Setup(r => r.UpdateAsync(reception)).Returns(Task.CompletedTask);

            var service = CreateService();
            var userName = "blocker";

            // Act
            await service.BlockAsync(reception.Id, userName, useTransaction: false);

            // Assert

            _mockReceptionLineProcessor.Verify(p => p.BlockAsync(line.Id, userName), Times.Once);
            _mockGenericReceptionRepository.Verify(r => r.UpdateAsync(reception), Times.Once);
        }

        [Fact]
        public async Task BlockAsync_ShouldThrowIfReceptionNotFound()
        {
            // Arrange
            _mockGenericReceptionRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Reception)null);
            var service = CreateService();
            var userName = "user";

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.BlockAsync(1, userName, false));
        }

        [Fact]
        public async Task UnblockAsync_ShouldClearBlockFieldsAndUnblockLines()
        {
            // Arrange
            var reception = _entityFactory.CreateReception();
            reception.BlDate = DateTime.UtcNow;
            reception.BlUserId = "userOld";
            var line = _entityFactory.CreateReceptionLine();
            reception.Lines = new List<ReceptionLine> { line };

            _mockGenericReceptionRepository.Setup(r => r.GetByIdAsync(reception.Id)).ReturnsAsync(reception);
            _mockReceptionLineProcessor.Setup(p => p.UnblockAsync(line.Id, It.IsAny<string>())).Returns(Task.CompletedTask);
            _mockGenericReceptionRepository.Setup(r => r.UpdateAsync(reception)).Returns(Task.CompletedTask);

            var service = CreateService();
            var userName = "unblocker";

            // Act
            await service.UnblockAsync(reception.Id, userName, useTransaction: false);

            // Assert
            Assert.Null(reception.BlDate);
            Assert.Null(reception.BlUserId);
            Assert.NotEqual(default, reception.LcDate);
            Assert.Equal(userName, reception.LcUserId);
            _mockReceptionLineProcessor.Verify(p => p.UnblockAsync(line.Id, userName), Times.Once);
            _mockGenericReceptionRepository.Verify(r => r.UpdateAsync(reception), Times.Once);
        }

        [Fact]
        public async Task UnblockAsync_ShouldThrowIfReceptionNotFound()
        {
            // Arrange
            _mockGenericReceptionRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Reception)null);
            var service = CreateService();
            var userName = "user";

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UnblockAsync(1, userName, false));
        }

        [Fact]
        public async Task GetNextLineNumberAsync_ShouldReturnNextLineNumber()
        {
            // Arrange
            var reception = _entityFactory.CreateReception();
            var line1 = _entityFactory.CreateReceptionLine();
            line1.LineNumber = 1;
            var line2 = _entityFactory.CreateReceptionLine();
            line2.LineNumber = 2;
            reception.Lines = new List<ReceptionLine> { line1, line2 };

            _mockGenericReceptionRepository.Setup(r => r.GetByIdAsync(reception.Id)).ReturnsAsync(reception);
            var service = CreateService();

            // Act
            var nextLineNumber = await service.GetNextLineNumberAsync(reception.Id);

            // Assert
            Assert.Equal(3, nextLineNumber);
        }

        [Fact]
        public async Task GetNextLineNumberAsync_ShouldReturn1IfNoLines()
        {
            // Arrange
            var reception = _entityFactory.CreateReception();
            reception.Lines = new List<ReceptionLine>();

            _mockGenericReceptionRepository.Setup(r => r.GetByIdAsync(reception.Id)).ReturnsAsync(reception);
            var service = CreateService();

            // Act
            var nextLineNumber = await service.GetNextLineNumberAsync(reception.Id);

            // Assert
            Assert.Equal(1, nextLineNumber);
        }

        [Fact]
        public async Task GetNextLineNumberAsync_ShouldThrowIfReceptionNotFound()
        {
            // Arrange
            _mockGenericReceptionRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Reception)null);
            var service = CreateService();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.GetNextLineNumberAsync(1));
            Assert.Contains("not found", ex.Message);
        }

    }
}
