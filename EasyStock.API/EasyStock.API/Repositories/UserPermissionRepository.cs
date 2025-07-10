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
    public class UserPermissionRepository : IUserPermissionRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<UserPermission> _dbSet;
        private readonly IMapper _mapper;

        public UserPermissionRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _dbSet = context.Set<UserPermission>();
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserPermissionOverview>> GetAllAsync()
        {
            return await _dbSet
                .ProjectTo<UserPermissionOverview>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<PaginationResult<UserPermissionOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
        {
            var query = _dbSet.AsQueryable();

            if (filters != null && filters.Count > 0)
            {
                // Custom filters
                if (filters.Any(f => f.Field == "UserName"))
                {
                    var userNameFilter = filters.First(f => f.Field == "UserName");

                    query = userNameFilter.Operator switch
                    {
                        "contains" => query.Where(p => p.User.UserName.Contains(userNameFilter.Value)),
                        "startswith" => query.Where(p => p.User.UserName.StartsWith(userNameFilter.Value)),
                        "endswith" => query.Where(p => p.User.UserName.EndsWith(userNameFilter.Value)),
                        _ => throw new NotSupportedException($"Operator is not supported for strings.")
                    };
                }

                // Regular Filters
                query = query.ApplyFilters(filters.Where(f => f.Field != "UserName").ToList());
            }

            // Sorting
            if (sorting != null && sorting.Count > 0)
            {
                var first = sorting.First();
                query = query.OrderBy($"{first.Field} {(first.Direction == "asc" ? "ascending" : "descending")}");
                foreach (var s in sorting.Skip(1))
                {
                    query = ((IOrderedQueryable<UserPermission>)query).ThenBy($"{s.Field} {(s.Direction == "asc" ? "ascending" : "descending")}");
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
                .ProjectTo<UserPermissionOverview>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new PaginationResult<UserPermissionOverview>
            {
                TotalCount = totalCount,
                Data = data
            };
        }

        public async Task<List<UserPermission>> GetPermissionsForUser(string userName)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);
            if (user == null) throw new Exception($"user {userName} not found.");

            return await _context.UserPermissions.Where(p => p.UserId == user.Id).ToListAsync();
        }
    }
}
