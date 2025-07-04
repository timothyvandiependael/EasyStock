using EasyStock.API.Common;
using EasyStock.API.Data;
using EasyStock.API.Models;
using EasyStock.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EasyStock.API.Services
{
    public class PurchaseOrderLineService : IPurchaseOrderLineService
    {
        private readonly IPurchaseOrderLineRepository _purchaseOrderLineRepository;
        private readonly IRepository<PurchaseOrderLine> _repository;
        private readonly IRepository<Product> _genericProductRepository;
        private readonly AppDbContext _context;

        public PurchaseOrderLineService(IPurchaseOrderLineRepository purchaseOrderLineRepository, IRepository<PurchaseOrderLine> repository, AppDbContext context)
        {
            _purchaseOrderLineRepository = purchaseOrderLineRepository;
            _repository = repository;
            _context = context;
        }

        public async Task<IEnumerable<PurchaseOrderLineOverview>> GetAllAsync()
            => await _purchaseOrderLineRepository.GetAllAsync();

        public async Task<PaginationResult<PurchaseOrderLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _purchaseOrderLineRepository.GetAdvancedAsync(filters, sorting, pagination);

        public async Task AddAsync(PurchaseOrderLine entity, string userName)
        {
            var attempt = 1;
            while (attempt <= 3)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    await _repository.AddAsync(entity);

                    var product = await _genericProductRepository.GetByIdAsync(entity.ProductId);
                    product.InboundStock += entity.Quantity;
                    product.LcUserId = userName;
                    product.LcDate = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    break;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    await transaction.RollbackAsync();

                    if (++attempt > 3)
                        throw;

                    await Task.Delay(100 * attempt);
                }
                catch (Exception ex) 
                { 
                    await transaction.RollbackAsync();
                }
            }
        }
    }
}
