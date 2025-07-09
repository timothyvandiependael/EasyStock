using EasyStock.API.Data;
using EasyStock.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyStock.API.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly AppDbContext _context;

        public SupplierRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Supplier>> GetByIds(List<int> ids)
        {
            var suppliers = await _context.Suppliers.Where(s => ids.Contains(s.Id)).ToListAsync();
            return suppliers;
        }
    }
}
