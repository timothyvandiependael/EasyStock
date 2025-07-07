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
                    entity.CrDate = DateTime.UtcNow;
                    entity.LcDate = entity.CrDate;
                    entity.CrUserId = userName;
                    entity.LcUserId = userName;
                    entity.Status = OrderStatus.Open;
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
                    throw;
                }
            }
        }

        public async Task UpdateAsync(PurchaseOrderLine entity, string userName)
        {
            var attempt = 1;
            while (attempt <= 3)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    entity.LcDate = DateTime.UtcNow;
                    entity.LcUserId = userName;
                    await _repository.AddAsync(entity);

                    var oldRecord = await _repository.GetByIdAsync(entity.Id);
                    if (oldRecord.Quantity != entity.Quantity)
                    {
                        if (entity.Status == OrderStatus.Open || entity.Status == OrderStatus.Partial)
                        {
                            var difference = entity.Quantity - oldRecord.Quantity;

                            var product = await _genericProductRepository.GetByIdAsync(entity.ProductId);
                            product.InboundStock += difference;
                            product.LcUserId = userName;
                            product.LcDate = DateTime.UtcNow;

                            await _context.SaveChangesAsync();
                        }
                    }

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
                    throw;
                }
            }
            

        }

        public async Task DeleteAsync(int id, string userName, bool manageTransaction = true)
        {
            if (manageTransaction)
            {
                var attempt = 1;
                while (attempt <= 3)
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        await DeleteInternalAsync(id, userName);

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
                        throw;
                    }
                }
            }
            else
            {
                await DeleteInternalAsync(id, userName);
            }
            
        }

        private async Task DeleteInternalAsync(int id, string userName)
        {
            var record = await _repository.GetByIdAsync(id);
            if (record.Status == OrderStatus.Open || record.Status == OrderStatus.Partial)
            {
                var product = await _genericProductRepository.GetByIdAsync(record.ProductId);
                product.InboundStock -= record.Quantity;
                product.LcUserId = userName;
                product.LcDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            await _repository.DeleteAsync(id);
        }

        public async Task BlockAsync(int id, string userName, bool manageTransaction = true)
        {
            if (manageTransaction)
            {
                var attempt = 1;
                while (attempt <= 3)
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        await BlockInternalAsync(id, userName);

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
                        throw;
                    }
                }
            }
            else
            {
                await BlockInternalAsync(id, userName);
            }
            
        }

        private async Task BlockInternalAsync(int id, string userName)
        {
            var record = await _repository.GetByIdAsync(id);
            record.BlDate = DateTime.UtcNow;
            record.BlUserId = userName;
            await _repository.UpdateAsync(record);

            if (record.Status == OrderStatus.Open || record.Status == OrderStatus.Partial)
            {
                var product = await _genericProductRepository.GetByIdAsync(record.ProductId);
                product.InboundStock -= record.Quantity;
                product.LcUserId = userName;
                product.LcDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UnblockAsync(int id, string userName, bool manageTransaction = true)
        {
            if (manageTransaction)
            {
                var attempt = 1;
                while (attempt <= 3)
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        await UnblockInternalAsync(id, userName);

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
                        throw;
                    }
                }
            }
            else
            {
                await UnblockInternalAsync(id, userName);
            }
            
        }

        private async Task UnblockInternalAsync(int id, string userName)
        {
            var record = await _repository.GetByIdAsync(id);
            record.BlDate = null;
            record.BlUserId = null;
            record.LcDate = DateTime.UtcNow;
            record.LcUserId = userName;

            await _repository.UpdateAsync(record);

            if (record.Status == OrderStatus.Open || record.Status == OrderStatus.Partial)
            {
                var product = await _genericProductRepository.GetByIdAsync(record.ProductId);
                product.InboundStock += record.Quantity;
                product.LcUserId = userName;
                product.LcDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
