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
    public class ClientRepository : IClientRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Client> _dbSet;

        public ClientRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<Client>();
        }

        public async Task<PaginationResult<Client>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination)
        {
            var query = _dbSet.AsQueryable();

            if (filters != null && filters.Count > 0)
            {
                // Custom filters
                if (filters.Any(f => f.Field == "SalesOrderId"))
                {
                    var soFilter = filters.First(f => f.Field == "SalesOrderId");
                    var parsedValue = int.Parse(soFilter.Value);

                    query = soFilter.Operator switch
                    {
                        "equals" or "=" => query.Where(p => p.SalesOrders.Any(p => p.Id == parsedValue)),
                        "notequals" or "<>" or "!=" => query.Where(p => !p.SalesOrders.Any(p => p.Id == parsedValue)),
                        _ => throw new NotSupportedException($"Operator is not supported.")
                    };
                }

                // Regular Filters
                query = query.ApplyFilters(filters.Where(f => f.Field != "SalesOrderId").ToList());
            }

            // Sorting
            if (sorting != null && sorting.Count > 0)
            {
                var first = sorting.First();
                query = query.OrderBy($"{first.Field} {(first.Direction == "asc" ? "ascending" : "descending")}");
                foreach (var s in sorting.Skip(1))
                {
                    query = ((IOrderedQueryable<Client>)query).ThenBy($"{s.Field} {(s.Direction == "asc" ? "ascending" : "descending")}");
                }
            }
            else
            {
                query = query.OrderBy("Id");
            }

            var data = new List<Client>();
            var totalCount = await query.CountAsync();
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

            return new PaginationResult<Client>
            {
                TotalCount = totalCount,
                Data = data
            };
        }
    }
}
