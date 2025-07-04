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
    public class ReceptionRepository : IReceptionRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Reception> _dbSet;
        private readonly IMapper _mapper;

        public ReceptionRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _dbSet = context.Set<Reception>();
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReceptionOverview>> GetAllAsync()
        {
            return await _dbSet
                .ProjectTo<ReceptionOverview>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<PaginationResult<ReceptionOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
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
                    query = ((IOrderedQueryable<Reception>)query).ThenBy($"{s.Field} {(s.Direction == "asc" ? "ascending" : "descending")}");
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
                .ProjectTo<ReceptionOverview>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new PaginationResult<ReceptionOverview>
            {
                TotalCount = totalCount,
                Data = data
            };
        }
    }
}
