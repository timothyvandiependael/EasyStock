﻿using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;

namespace EasyStock.API.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IRepository<Product> _repository;
        private readonly IRetryableTransactionService _retryableTransactionService;
        private readonly IRepository<StockMovement> _genericStockMovementRepository;

        public ProductService(IProductRepository productRepository, IRepository<Product> repository, IRetryableTransactionService retryableTransactionService, IRepository<StockMovement> genericStockMovementRepository)
        {
            _productRepository = productRepository;
            _repository = repository;
            _retryableTransactionService = retryableTransactionService;
            _genericStockMovementRepository = genericStockMovementRepository;
        }

        public async Task<IEnumerable<ProductOverview>> GetAllAsync()
            => await _productRepository.GetAllAsync();

        public async Task<PaginationResult<ProductOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _productRepository.GetAdvancedAsync(filters, sorting, pagination); 

        public async Task<bool> IsProductBelowMinimumStock(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
                throw new Exception($"Product with id {id} not found.");
            return product.MinimumStock > product.AvailableStock;
        }

        public async Task UpdateAsync(Product product, string userName)
        {
            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                var oldProduct = await _repository.GetByIdAsync(product.Id);
                if (oldProduct == null) throw new Exception($"Product with id {product.Id} not found.");
                if (oldProduct.TotalStock != product.TotalStock)
                {
                    var difference = product.TotalStock - oldProduct.TotalStock;
                    var tmpAvailableStock = product.AvailableStock - difference;
                    if (tmpAvailableStock < 0)
                    {
                        product.AvailableStock = 0;
                        var minusAvailableStock = Math.Abs(tmpAvailableStock);
                        product.BackOrderedStock += minusAvailableStock;
                    }
                    else
                    {
                        product.AvailableStock -= difference;
                    }

                    var stockMovement = new StockMovement
                    {
                        ProductId = product.Id,
                        Product = product,
                        QuantityChange = difference,
                        Reason = "Stock correction",
                        CrDate = DateTime.UtcNow,
                        LcDate = DateTime.UtcNow,
                        CrUserId = userName,
                        LcUserId = userName
                    };

                    await _genericStockMovementRepository.AddAsync(stockMovement);
                }

                product.LcDate = DateTime.UtcNow;
                product.LcUserId = userName;

                await _repository.UpdateAsync(product);
            });
            
        }

    }
}
