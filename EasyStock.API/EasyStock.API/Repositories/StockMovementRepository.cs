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
    public class StockMovementRepository : IStockMovementRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<StockMovement> _dbSet;
        private readonly IMapper _mapper;

        public StockMovementRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _dbSet = context.Set<StockMovement>();
            _mapper = mapper;
        }

        public async Task<IEnumerable<StockMovementOverview>> GetAllAsync()
        {
            return await _dbSet
                .ProjectTo<StockMovementOverview>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<PaginationResult<StockMovementOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
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
                    query = ((IOrderedQueryable<StockMovement>)query).ThenBy($"{s.Field} {(s.Direction == "asc" ? "ascending" : "descending")}");
                }
            }
            else
            {
                query = query.OrderBy("Id");
            }

            // Pagination
            var totalCount = await query.CountAsync();
            var data = await query
                .Skip((pagination.PageNumber) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ProjectTo<StockMovementOverview>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new PaginationResult<StockMovementOverview>
            {
                TotalCount = totalCount,
                Data = data
            };
        }
    }
}
