using EasyStock.API.Repositories;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _repository;
        private readonly IUpdateService<User> _userUpdateService;
        public UserService(IRepository<User> repository, IUpdateService<User> userUpdateService)
        {
            _repository = repository;
            _userUpdateService = userUpdateService;
        }

        public async Task AddAsync(User entity, string userName)
        {
            foreach (var permission in entity.Permissions)
            {
                permission.CrDate = DateTime.UtcNow;
                permission.LcDate = DateTime.UtcNow;
                permission.CrUserId = userName;
                permission.LcUserId = userName;
                permission.UserId = entity.Id;
            }

            entity.CrDate = DateTime.UtcNow;
            entity.LcDate = DateTime.UtcNow;
            entity.CrUserId = userName;
            entity.LcUserId = userName;

            await _repository.AddAsync(entity);
        }
        public async Task UpdateAsync(User entity, string userName)
        {
            var existing = await _repository.GetByIdAsync(entity.Id);
            if (existing == null)
                throw new KeyNotFoundException($"User with id {entity.Id} not found.");

            foreach (var permission in entity.Permissions)
            {
                var existingPermission = existing.Permissions.Where(p => p.Resource == permission.Resource).FirstOrDefault();
                if (existingPermission == null)
                {
                    permission.CrDate = DateTime.UtcNow;
                    permission.LcDate = DateTime.UtcNow;
                    permission.CrUserId = userName;
                    permission.LcUserId = userName;
                    permission.UserId = entity.Id;
                    existing.Permissions.Add(permission);
                }
                else
                {
                    existingPermission.LcDate = DateTime.UtcNow;
                    existingPermission.LcUserId = userName;
                    existingPermission.CanAdd = permission.CanAdd;
                    existingPermission.CanView = permission.CanView;
                    existingPermission.CanDelete = permission.CanDelete;
                    existingPermission.CanEdit = permission.CanEdit;
                }
            }

            existing.UserName = entity.UserName;
            existing.LcDate = DateTime.UtcNow;
            existing.LcUserId = userName;

            await _repository.UpdateAsync(existing);
        }
        public async Task BlockAsync(int id, string userName)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new InvalidOperationException($"Unable to block record with ID {id}");
            entity.BlDate = DateTime.UtcNow;
            entity.BlUserId = userName;

            foreach (var permission in entity.Permissions)
            {
                permission.BlDate = DateTime.UtcNow;
                permission.BlUserId = userName;
            }

            await _repository.UpdateAsync(entity);
        }
        public async Task UnblockAsync(int id, string userName)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new InvalidOperationException($"Unable to unblock record with ID {id}");
            entity.BlDate = null;
            entity.BlUserId = null;
            entity.LcDate = DateTime.UtcNow;
            entity.LcUserId = userName;

            foreach (var permission in entity.Permissions)
            {
                permission.BlDate = null;
                permission.BlUserId = null;
                permission.LcDate = DateTime.UtcNow;
                permission.LcUserId = userName;
            }

            await _repository.UpdateAsync(entity);
        }
    }
}
