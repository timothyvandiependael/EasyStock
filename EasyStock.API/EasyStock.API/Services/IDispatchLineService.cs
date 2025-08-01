﻿using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface IDispatchLineService
    {
        Task<IEnumerable<DispatchLineOverview>> GetAllAsync();
        Task<PaginationResult<DispatchLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination);
        Task AddAsync(DispatchLine entity, string userName);
        Task UpdateAsync(DispatchLine entity, string userName, bool useTransaction = true);
        Task DeleteAsync(int id, string userName);
        Task BlockAsync(int id, string userName);
        Task UnblockAsync(int id, string userName);
    }
}
