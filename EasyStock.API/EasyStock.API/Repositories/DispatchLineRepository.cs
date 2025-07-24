using AutoMapper;
using AutoMapper.QueryableExtensions;
using EasyStock.API.Common;
using EasyStock.API.Data;
using EasyStock.API.Extensions;
using EasyStock.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EasyStock.API.Repositories
{
    public class DispatchLineRepository : IDispatchLineRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<DispatchLine> _dbSet;
        private readonly IMapper _mapper;

        public DispatchLineRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _dbSet = context.Set<DispatchLine>();
            _mapper = mapper;
        }

        public async Task<IEnumerable<DispatchLineOverview>> GetAllAsync()
        {
            return await _dbSet
                .ProjectTo<DispatchLineOverview>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<PaginationResult<DispatchLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination)
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

                if (filters.Any(f => f.Field == "DispatchNumber"))
                {
                    var dispatchNumberFilter = filters.First(f => f.Field == "DispatchNumber");

                    query = dispatchNumberFilter.Operator switch
                    {
                        "contains" => query.Where(p => p.Dispatch.DispatchNumber.Contains(dispatchNumberFilter.Value)),
                        "startswith" => query.Where(p => p.Dispatch.DispatchNumber.StartsWith(dispatchNumberFilter.Value)),
                        "endswith" => query.Where(p => p.Dispatch.DispatchNumber.EndsWith(dispatchNumberFilter.Value)),
                        _ => throw new NotSupportedException($"Operator is not supported for strings.")
                    };
                }

                // Regular Filters
                query = query.ApplyFilters(filters.Where(f => f.Field != "ProductName" && f.Field != "DispatchNumber").ToList());
            }

            // Sorting
            if (sorting != null && sorting.Count > 0)
            {
                var first = sorting.First();
                query = query.OrderBy($"{first.Field} {(first.Direction == "asc" ? "ascending" : "descending")}");
                foreach (var s in sorting.Skip(1))
                {
                    query = ((IOrderedQueryable<DispatchLine>)query).ThenBy($"{s.Field} {(s.Direction == "asc" ? "ascending" : "descending")}");
                }
            }
            else
            {
                query = query.OrderBy("Id");
            }

            var data = new List<DispatchLineOverview>();
            var totalCount = await query.CountAsync();
            // Pagination
            if (pagination != null)
            {
                data = await query
                .Skip((pagination.PageNumber) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ProjectTo<DispatchLineOverview>(_mapper.ConfigurationProvider)
                .ToListAsync();
            }
            else
            {
                data = await query.ProjectTo<DispatchLineOverview>(_mapper.ConfigurationProvider).ToListAsync();
            }

            return new PaginationResult<DispatchLineOverview>
            {
                TotalCount = totalCount,
                Data = data
            };
        }
    }
}
