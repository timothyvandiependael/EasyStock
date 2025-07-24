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
    public class SalesOrderRepository : ISalesOrderRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<SalesOrder> _dbSet;
        private readonly IMapper _mapper;

        public SalesOrderRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _dbSet = context.Set<SalesOrder>();
            _mapper = mapper;
        }

        public async Task<IEnumerable<SalesOrderOverview>> GetAllAsync()
        {
            return await _dbSet
                .ProjectTo<SalesOrderOverview>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<PaginationResult<SalesOrderOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination)
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
                query = query.ApplyFilters(filters.Where(f => f.Field != "SupplierName").ToList());
            }

            // Sorting
            if (sorting != null && sorting.Count > 0)
            {
                var first = sorting.First();
                query = query.OrderBy($"{first.Field} {(first.Direction == "asc" ? "ascending" : "descending")}");
                foreach (var s in sorting.Skip(1))
                {
                    query = ((IOrderedQueryable<SalesOrder>)query).ThenBy($"{s.Field} {(s.Direction == "asc" ? "ascending" : "descending")}");
                }
            }
            else
            {
                query = query.OrderBy("Id");
            }

            var data = new List<SalesOrderOverview>();
            var totalCount = await query.CountAsync();
            // Pagination
            if (pagination != null)
            {
                data = await query
                .Skip((pagination.PageNumber) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ProjectTo<SalesOrderOverview>(_mapper.ConfigurationProvider)
                .ToListAsync();
            }
            else
            {
                data = await query.ProjectTo<SalesOrderOverview>(_mapper.ConfigurationProvider).ToListAsync();
            }

            return new PaginationResult<SalesOrderOverview>
            {
                TotalCount = totalCount,
                Data = data
            };
        }
    }
}
