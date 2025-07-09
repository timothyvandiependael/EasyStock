using EasyStock.API.Models;

namespace EasyStock.API.Repositories
{
    public interface ISupplierRepository
    {
        Task<List<Supplier>> GetByIds(List<int> ids);
    }
}
