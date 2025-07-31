using AutoMapper;
using AutoMapper.QueryableExtensions;
using EasyStock.API.Common;
using EasyStock.API.Data;
using EasyStock.API.Extensions;
using EasyStock.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace EasyStock.API.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Supplier> _dbSet;

        public SupplierRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<Supplier>();
        }

        public async Task<List<Supplier>> GetByIds(List<int> ids)
        {
            var suppliers = await _context.Suppliers.Where(s => ids.Contains(s.Id)).ToListAsync();
            return suppliers;
        }

        public async Task<PaginationResult<Supplier>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination)
        {
            var query = _dbSet.AsQueryable();

            if (filters != null && filters.Count > 0)
            {
                // Custom filters
                if (filters.Any(f => f.Field == "ProductId"))
                {
                    var productFilter = filters.First(f => f.Field == "ProductId");
                    var parsedValue = int.Parse(productFilter.Value);

                    query = productFilter.Operator switch
                    {
                        "equals" or "=" => query.Where(s => s.Products.Any(p => p.Id == parsedValue)),
                        "notequals" or "<>" or "!=" => query.Where(s => !s.Products.Any(p => p.Id == parsedValue)),
                        _ => throw new NotSupportedException($"Operator is not supported.")
                    };
                }

                if (filters.Any(f => f.Field == "PurchaseOrderId"))
                {
                    var poFilter = filters.First(f => f.Field == "PurchaseOrderId");
                    var parsedValue = int.Parse(poFilter.Value);

                    query = poFilter.Operator switch
                    {
                        "equals" or "=" => query.Where(s => s.PurchaseOrders.Any(p => p.Id == parsedValue)),
                        "notequals" or "<>" or "!=" => query.Where(s => !s.PurchaseOrders.Any(p => p.Id == parsedValue)),
                        _ => throw new NotSupportedException($"Operator is not supported.")
                    };
                }

                // Regular Filters
                query = query.ApplyFilters(filters.Where(f => f.Field != "ProductId" && f.Field != "PurchaseOrderId").ToList());
            }

            // Sorting
            if (sorting != null && sorting.Count > 0)
            {
                var first = sorting.First();
                query = query.OrderBy($"{first.Field} {(first.Direction == "asc" ? "ascending" : "descending")}");
                foreach (var s in sorting.Skip(1))
                {
                    query = ((IOrderedQueryable<Supplier>)query).ThenBy($"{s.Field} {(s.Direction == "asc" ? "ascending" : "descending")}");
                }
            }
            else
            {
                query = query.OrderBy("Id");
            }

            var data = new List<Supplier>();
            var totalCount = 0;

            try
            {
                totalCount = await query.CountAsync();
                // Pagination
                if (pagination != null)
                {
                    data = await query
                    .Skip((pagination.PageNumber) * pagination.PageSize)
                    .Take(pagination.PageSize)
                    .ToListAsync();
                }
                else
                {
                    data = await query.ToListAsync();
                }

            }
            catch (Exception ex)
            {
                var x = ex;
                throw;
            }

           

            return new PaginationResult<Supplier>
            {
                TotalCount = totalCount,
                Data = data
            };
        }

        public async Task AddProductAsync(int id, int productId)
        {
            var supplier = _context.Suppliers
                .Include(p => p.Products)
                .FirstOrDefault(s => s.Id == id);
            if (supplier == null) throw new Exception("Supplier not found when attempting to add product.");

            var product = _context.Products
                .FirstOrDefault(s => s.Id == productId);
            if (product == null) throw new Exception("Product not found when attempting to add to supplier.");

            if (!supplier.Products.Contains(product))
            {
                supplier.Products.Add(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveProductAsync(int id, int productId)
        {
            var supplier = _context.Suppliers
                .Include(p => p.Products)
                .FirstOrDefault(s => s.Id == id);
            if (supplier == null) throw new Exception("Supplier not found when attempting to remove product.");

            var product = _context.Products
                .FirstOrDefault(s => s.Id == productId);
            if (product == null) throw new Exception("Product not found when attempting to remove from supplier.");

            if (supplier.Products.Contains(product))
            {
                supplier.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }
    }
}
