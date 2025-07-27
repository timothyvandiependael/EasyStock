using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface IUpdateService<T> where T: ModelBase, IEntity
    {
        T MapAndUpdateAuditFields(T oldEntity, T newEntity, string userName);
    }
}
