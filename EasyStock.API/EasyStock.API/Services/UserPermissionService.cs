using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;

namespace EasyStock.API.Services
{
    public class UserPermissionService : IUserPermissionService
    {
        private readonly IUserPermissionRepository _userPermissionRepository;

        public UserPermissionService(IUserPermissionRepository userPermissionRepository)
        {
            _userPermissionRepository = userPermissionRepository;
        }

        public async Task<IEnumerable<UserPermissionOverview>> GetAllAsync()
            => await _userPermissionRepository.GetAllAsync();

        public async Task<PaginationResult<UserPermissionOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination)
            => await _userPermissionRepository.GetAdvancedAsync(filters, sorting, pagination);

        public async Task<List<UserPermission>> GetPermissionsForUser(string userName)
        {
            return await _userPermissionRepository.GetPermissionsForUser(userName);
        }
    }
}
