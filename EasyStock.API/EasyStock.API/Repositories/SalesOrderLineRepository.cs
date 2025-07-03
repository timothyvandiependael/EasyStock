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
    public class SalesOrderLineRepository : ISalesOrderLineRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<SalesOrderLine> _dbSet;
        private readonly IMapper _mapper;

        public SalesOrderLineRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _dbSet = context.Set<SalesOrderLine>();
            _mapper = mapper;
        }

        public async Task<IEnumerable<SalesOrderLineOverview>> GetAllAsync()
        {
            return await _dbSet
                .ProjectTo<SalesOrderLineOverview>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<PaginationResult<SalesOrderLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
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
                        "endswith" => query.Where(p => p.Product.Name.EndsWith(productNameFilter.Value))
                    };
                }

                // Regular Filters
                query = query.ApplyFilters(filters.Where(f => f.Field != "ProductName").ToList());
            }

            // Sorting
            if (sorting != null && sorting.Count > 0)
            {
                var first = sorting.First();
                query = query.OrderBy($"{first.Field} {(first.Direction == "asc" ? "ascending" : "descending")}");
                foreach (var s in sorting.Skip(1))
                {
                    query = ((IOrderedQueryable<SalesOrderLine>)query).ThenBy($"{s.Field} {(s.Direction == "asc" ? "ascending" : "descending")}");
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
                .ProjectTo<SalesOrderLineOverview>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new PaginationResult<SalesOrderLineOverview>
            {
                TotalCount = totalCount,
                Data = data
            };
        }
    }
}
