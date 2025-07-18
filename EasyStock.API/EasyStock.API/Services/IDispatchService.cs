﻿using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface IDispatchService
    {
        Task<IEnumerable<DispatchOverview>> GetAllAsync();
        Task<PaginationResult<DispatchOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination);
        Task AddAsync(Dispatch entity, string userName);
        Task DeleteAsync(int id, string userName);
        Task BlockAsync(int id, string userName);
        Task UnblockAsync(int id, string userName);
        Task<int> GetNextLineNumberAsync(int id);
        Task<Dispatch?> AddFromSalesOrder(int salesOrderId, string userName);
    }
}
