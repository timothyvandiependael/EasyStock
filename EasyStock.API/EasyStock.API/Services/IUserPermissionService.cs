using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface IUserPermissionService
    {
        Task<IEnumerable<UserPermissionOverview>> GetAllAsync();
        Task<PaginationResult<UserPermissionOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination);
        Task<List<UserPermission>> GetPermissionsForUser(string userName);
    }
}
