namespace EasyStock.API.Services
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(string userName, string resource, string action);
    }
}
