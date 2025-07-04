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
    public class PurchaseOrderLineRepository : IPurchaseOrderLineRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<PurchaseOrderLine> _dbSet;
        private readonly IMapper _mapper;

        public PurchaseOrderLineRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _dbSet = context.Set<PurchaseOrderLine>();
            _mapper = mapper;
        }

        public async Task<IEnumerable<PurchaseOrderLineOverview>> GetAllAsync()
        {
            return await _dbSet
                .ProjectTo<PurchaseOrderLineOverview>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<PaginationResult<PurchaseOrderLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
        {
            var query = _dbSet.AsQueryable();

            if (filters != null && filters.Count > 0)
            {
                // Custom filters
                if (filters.Any(f => f.Field == "ProductName"))
                {
                    var productNameFilter = filters.First(f => f.Field == "ProductName");

                    query = productNameFilter.Operator switch
                    {
                        "contains" => query.Where(p => p.Product.Name.Contains(productNameFilter.Value)),
                        "startswith" => query.Where(p => p.Product.Name.StartsWith(productNameFilter.Value)),
                        "endswith" => query.Where(p => p.Product.Name.EndsWith(productNameFilter.Value)),
                        _ => throw new NotSupportedException($"Operator is not supported for strings.")
                    };
                }

                if (filters.Any(f => f.Field == "OrderNumber"))
                {
                    var orderNumberFilter = filters.First(f => f.Field == "OrderNumber");

                    query = orderNumberFilter.Operator switch
                    {
                        "contains" => query.Where(p => p.PurchaseOrder.OrderNumber.Contains(orderNumberFilter.Value)),
                        "startswith" => query.Where(p => p.PurchaseOrder.OrderNumber.StartsWith(orderNumberFilter.Value)),
                        "endswith" => query.Where(p => p.PurchaseOrder.OrderNumber.EndsWith(orderNumberFilter.Value)),
                        _ => throw new NotSupportedException($"Operator is not supported for strings.")
                    };
                }

                // Regular Filters
                query = query.ApplyFilters(filters.Where(f => f.Field != "ProductName" && f.Field != "OrderNumber").ToList());
            }

            // Sorting
            if (sorting != null && sorting.Count > 0)
            {
                var first = sorting.First();
                query = query.OrderBy($"{first.Field} {(first.Direction == "asc" ? "ascending" : "descending")}");
                foreach (var s in sorting.Skip(1))
                {
                    query = ((IOrderedQueryable<PurchaseOrderLine>)query).ThenBy($"{s.Field} {(s.Direction == "asc" ? "ascending" : "descending")}");
                }
            }
            else
            {
                query = query.OrderBy("Id");
            }

            // Pagination
            var totalCount = await query.CountAsync();
            var data = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ProjectTo<PurchaseOrderLineOverview>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new PaginationResult<PurchaseOrderLineOverview>
            {
                TotalCount = totalCount,
                Data = data
            };
        }
    }
}
