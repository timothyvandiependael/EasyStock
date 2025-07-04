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
    public class DispatchRepository : IDispatchRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Dispatch> _dbSet;
        private readonly IMapper _mapper;

        public DispatchRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _dbSet = context.Set<Dispatch>();
            _mapper = mapper;
        }

        public async Task<IEnumerable<DispatchOverview>> GetAllAsync()
        {
            return await _dbSet
                .ProjectTo<DispatchOverview>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<PaginationResult<DispatchOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
        {
            var query = _dbSet.AsQueryable();

            if (filters != null && filters.Count > 0)
            {
                // Custom filters
                if (filters.Any(f => f.Field == "ClientName"))
                {
                    var clientNameFilter = filters.First(f => f.Field == "ClientName");

                    query = clientNameFilter.Operator switch
                    {
                        "contains" => query.Where(p => p.Client.Name.Contains(clientNameFilter.Value)),
                        "startswith" => query.Where(p => p.Client.Name.StartsWith(clientNameFilter.Value)),
                        "endswith" => query.Where(p => p.Client.Name.EndsWith(clientNameFilter.Value)),
                        _ => throw new NotSupportedException($"Operator is not supported for strings.")
                    };
                }

                // Regular Filters
                query = query.ApplyFilters(filters.Where(f => f.Field != "ClientName").ToList());
            }

            // Sorting
            if (sorting != null && sorting.Count > 0)
            {
                var first = sorting.First();
                query = query.OrderBy($"{first.Field} {(first.Direction == "asc" ? "ascending" : "descending")}");
                foreach (var s in sorting.Skip(1))
                {
                    query = ((IOrderedQueryable<Dispatch>)query).ThenBy($"{s.Field} {(s.Direction == "asc" ? "ascending" : "descending")}");
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
                .ProjectTo<DispatchOverview>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new PaginationResult<DispatchOverview>
            {
                TotalCount = totalCount,
                Data = data
            };
        }
    }
}
