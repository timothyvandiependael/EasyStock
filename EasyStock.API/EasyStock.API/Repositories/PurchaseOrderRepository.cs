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
    public class PurchaseOrderRepository : IPurchaseOrderRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<PurchaseOrder> _dbSet;
        private readonly IMapper _mapper;

        public PurchaseOrderRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _dbSet = context.Set<PurchaseOrder>();
            _mapper = mapper;
        }

        public async Task<IEnumerable<PurchaseOrderOverview>> GetAllAsync()
        {
            return await _dbSet
                .ProjectTo<PurchaseOrderOverview>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<PaginationResult<PurchaseOrderOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination)
        {
            var query = _dbSet.AsQueryable();

            if (filters != null && filters.Count > 0)
            {
                // Custom filters
                if (filters.Any(f => f.Field == "SupplierName"))
                {
                    var supplierNameFilter = filters.First(f => f.Field == "SupplierName");

                    query = supplierNameFilter.Operator switch
                    {
                        "contains" => query.Where(p => p.Supplier.Name.Contains(supplierNameFilter.Value)),
                        "startswith" => query.Where(p => p.Supplier.Name.StartsWith(supplierNameFilter.Value)),
                        "endswith" => query.Where(p => p.Supplier.Name.EndsWith(supplierNameFilter.Value)),
                        _ => throw new NotSupportedException($"Operator is not supported for strings.")
                    };
                }

                // Regular Filters
                query = query.ApplyFilters(filters.Where(f => f.Field != "SupplierName").ToList());
            }

            // Sorting
            if (sorting != null && sorting.Count > 0)
            {
                var first = sorting.First();
                query = query.OrderBy($"{first.Field} {(first.Direction == "asc" ? "ascending" : "descending")}");
                foreach (var s in sorting.Skip(1))
                {
                    query = ((IOrderedQueryable<PurchaseOrder>)query).ThenBy($"{s.Field} {(s.Direction == "asc" ? "ascending" : "descending")}");
                }
            }
            else
            {
                query = query.OrderBy("Id");
            }

            var data = new List<PurchaseOrderOverview>();
            var totalCount = await query.CountAsync();
            // Pagination
            if (pagination != null)
            {
                data = await query
                .Skip((pagination.PageNumber) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ProjectTo<PurchaseOrderOverview>(_mapper.ConfigurationProvider)
                .ToListAsync();
            }
            else
            {
                data = await query.ProjectTo<PurchaseOrderOverview>(_mapper.ConfigurationProvider).ToListAsync();
            }

            return new PaginationResult<PurchaseOrderOverview>
            {
                TotalCount = totalCount,
                Data = data
            };
        }
    }
}
