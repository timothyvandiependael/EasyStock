using EasyStock.API.Models;
using EasyStock.API.Repositories;
using EasyStock.API.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using EasyStock.API.Dtos;

namespace EasyStock.Tests.Services
{
    public class FakeModel : ModelBase, IEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class FakeModelMappingProfile : Profile
    {
        public FakeModelMappingProfile()
        {
            CreateMap<FakeModel, FakeModel>();
        }
    }

    public class ServiceTests
    {
        private readonly Mock<IRepository<FakeModel>> _repoMock;
        private readonly IService<FakeModel> _service;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IUpdateService<FakeModel>> _updateServiceMock = new Mock<IUpdateService<FakeModel>>();

        public ServiceTests()
        {
            _repoMock = new Mock<IRepository<FakeModel>>();
            _mapperMock = new Mock<IMapper>();
            _service = new Service<FakeModel>(_repoMock.Object, _updateServiceMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_CallsRepoAndReturnsResult()
        {
            // Arrange
            var model = new FakeModel
            {
                Id = 1,
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "test",
                LcUserId = "test"
            };

            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(model);

            // Act
            var result = await _service.GetByIdAsync(1);

            // Assert
            Assert.Equal(model, result);
            _repoMock.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task AddAsync_SetsAuditFieldsAndCallsRepository()
        {
            // Arrange
            var model = new FakeModel
            {
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "",
                LcUserId = ""
            };
            var userName = "tester";

            // Act
            await _service.AddAsync(model, userName);

            // Assert
            Assert.Equal(userName, model.CrUserId);
            Assert.Equal(userName, model.LcUserId);
            Assert.NotEqual(default, model.CrDate);
            Assert.NotEqual(default, model.LcDate);
            _repoMock.Verify(r => r.AddAsync(model), Times.Once);
        }

        [Fact]
        public async Task BlockAsync_SetsBlockFieldsAndCallsRepository()
        {
            // Arrange
            var model = new FakeModel
            {
                Id = 5,
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "",
                LcUserId = ""
            };

            _repoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(model);
            var userName = "tester";

            // Act
            await _service.BlockAsync(5, userName);

            // Assert
            Assert.Equal(userName, model.BlUserId);
            Assert.NotNull(model.BlDate);
            _repoMock.Verify(r => r.UpdateAsync(model), Times.Once);
        }

        [Fact]
        public async Task UnblockAsync_ClearsBlockFieldsAndCallsRepository()
        {
            // Arrange
            var model = new FakeModel
            {
                Id = 5,
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "",
                LcUserId = ""
            };

            _repoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(model);
            var userName = "tester";

            // Act
            await _service.UnblockAsync(5, userName);

            // Assert
            Assert.Equal(userName, model.LcUserId);
            Assert.NotEqual(default, model.LcDate);
            Assert.Null(model.BlDate);
            Assert.Null(model.BlUserId);
            _repoMock.Verify(r => r.UpdateAsync(model), Times.Once);
        }
    }
}
