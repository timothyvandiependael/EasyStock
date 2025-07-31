using AutoMapper;
using AutoMapper.QueryableExtensions;
using EasyStock.API.Common;
using EasyStock.API.Data;
using EasyStock.API.Extensions;
using EasyStock.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection.Metadata;

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

        public async Task<PaginationResult<SalesOrderLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination)
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
                        "contains" => query.Where(p => p.SalesOrder.OrderNumber.Contains(orderNumberFilter.Value)),
                        "startswith" => query.Where(p => p.SalesOrder.OrderNumber.StartsWith(orderNumberFilter.Value)),
                        "endswith" => query.Where(p => p.SalesOrder.OrderNumber.EndsWith(orderNumberFilter.Value)),
                        _ => throw new NotSupportedException($"Operator is not supported for strings.")
                    };
                }

                if (filters.Any(f => f.Field == "ClientId"))
                {
                    var clientIdFilter = filters.First(f => f.Field == "ClientId");

                    query = clientIdFilter.Operator switch
                    {
                        "equals" or "=" => query.Where(p => p.SalesOrder.ClientId.Equals(int.Parse(clientIdFilter.Value))),
                        _ => throw new NotSupportedException($"Operator is not supported.")
                    };
                }

                if (filters.Any(f => f.Field == "DispatchLineId"))
                {
                    var dlFilter = filters.First(f => f.Field == "DispatchLineId");

                    query = dlFilter.Operator switch
                    {
                        "equals" or "=" => query.Where(p => p.DispatchLines != null && p.DispatchLines.Any(d => d.Id == int.Parse(dlFilter.Value))),
                        "notequals" or "<>" or "!=" => query.Where(p => p.DispatchLines != null && !p.DispatchLines.Any(p => p.Id == int.Parse(dlFilter.Value))),
                        _ => throw new NotSupportedException($"Operator is not supported.")
                    };
                }

                // Regular Filters
                query = query.ApplyFilters(filters.Where(f => f.Field != "ProductName" && f.Field != "OrderNumber" && f.Field != "ClientId" && f.Field != "DispatchLineId").ToList());
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

            var data = new List<SalesOrderLineOverview>();
            var totalCount = await query.CountAsync();
            // Pagination
            if (pagination != null)
            {
                data = await query
                .Skip((pagination.PageNumber) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ProjectTo<SalesOrderLineOverview>(_mapper.ConfigurationProvider)
                .ToListAsync();
            }
            else
            {
                data = await query.ProjectTo<SalesOrderLineOverview>(_mapper.ConfigurationProvider).ToListAsync();
            }

            return new PaginationResult<SalesOrderLineOverview>
            {
                TotalCount = totalCount,
                Data = data
            };
        }
    }
}
