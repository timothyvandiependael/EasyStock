using EasyStock.API.Repositories;

namespace EasyStock.API.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IUserAuthRepository _userAuthRepository;
        private readonly IUserPermissionRepository _userPermissionRepository;

        public PermissionService(IUserAuthRepository userAuthRepository, IUserPermissionRepository userPermissionRepository)
        {
            _userAuthRepository = userAuthRepository;
            _userPermissionRepository = userPermissionRepository;
        }

        public async Task<bool> HasPermissionAsync(string userName, string resource, string action)
        {
            var user = await _userAuthRepository.GetByUserNameAsync(userName);
            if (user == null) return false;

            if (user.Role == Common.UserRole.Admin) return true;

            var permissions = await _userPermissionRepository.GetPermissionsForUser(userName);
            if (permissions == null) return false;

            return action.ToLower() switch
            {
                "view" => permissions.Any(p => p.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase) && p.CanView),
                "add" => permissions.Any(p => p.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase) && p.CanAdd),
                "edit" => permissions.Any(p => p.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase) && p.CanEdit),
                "delete" => permissions.Any(p => p.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase) && p.CanDelete),
                _ => false
            };
        }
    }
}
