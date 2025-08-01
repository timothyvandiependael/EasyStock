﻿using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductOverview>> GetAllAsync();
        Task<PaginationResult<ProductOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination);

        Task<bool> IsProductBelowMinimumStock(int id);
        Task<bool> IsProductOrderedEnough(int id)

        Task UpdateAsync(Product product, string userName, bool useTransaction = true);

        Task AddSupplierAsync(int id, int supplierId);
        Task RemoveSupplierAsync(int id, int supplierId);
    }
}
