﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using EasyStock.API.Common;
using EasyStock.API.Data;
using EasyStock.API.Extensions;
using EasyStock.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace EasyStock.API.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Product> _dbSet;
        private readonly IMapper _mapper;

        public ProductRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _dbSet = context.Set<Product>();
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductOverview>> GetAllAsync()
        {
            return await _dbSet
                .ProjectTo<ProductOverview>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<PaginationResult<ProductOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination)
        {
            var query = _dbSet.AsQueryable();

            if (filters != null && filters.Count > 0)
            {
                // Custom filters
                if (filters.Any(f => f.Field == "AutoRestockSupplierName"))
                {
                    var autoRestockSupplierNameFilter = filters.First(f => f.Field == "AutoRestockSupplierName");

                    query = autoRestockSupplierNameFilter.Operator switch
                    {
                        "contains" => query.Where(p => p.AutoRestockSupplier != null && p.AutoRestockSupplier.Name.Contains(autoRestockSupplierNameFilter.Value)),
                        "startswith" => query.Where(p => p.AutoRestockSupplier != null && p.AutoRestockSupplier.Name.StartsWith(autoRestockSupplierNameFilter.Value)),
                        "endswith" => query.Where(p => p.AutoRestockSupplier != null && p.AutoRestockSupplier.Name.EndsWith(autoRestockSupplierNameFilter.Value)),
                        _ => throw new NotSupportedException($"Operator is not supported for strings.")
                    };
                }

                if (filters.Any(f => f.Field == "CategoryName"))
                {
                    var categoryNameFilter = filters.First(f => f.Field == "CategoryName");

                    query = categoryNameFilter.Operator switch
                    {
                        "contains" => query.Where(p => p.Category.Name.Contains(categoryNameFilter.Value)),
                        "startswith" => query.Where(p => p.Category.Name.StartsWith(categoryNameFilter.Value)),
                        "endswith" => query.Where(p => p.Category.Name.EndsWith(categoryNameFilter.Value)),
                        _ => throw new NotSupportedException($"Operator is not supported for strings.")
                    };
                }

                if (filters.Any(f => f.Field == "SupplierId"))
                {
                    var supplierFilter = filters.First(f => f.Field == "SupplierId");

                    query = supplierFilter.Operator switch
                    {
                        "equals" or "=" => query.Where(p => p.Suppliers.Any(s => s.Id == int.Parse(supplierFilter.Value))),
                        "notequals" or "<>" or "!=" => query.Where(p => !p.Suppliers.Any(p => p.Id == int.Parse(supplierFilter.Value))),
                        _ => throw new NotSupportedException($"Operator is not supported.")
                    };
                }

                // Regular Filters
                query = query.ApplyFilters(filters.Where(f => f.Field != "AutoRestockSupplierName" && f.Field != "CategoryName" && f.Field != "SupplierId").ToList());
            }

            // Sorting
            if (sorting != null && sorting.Count > 0)
            {
                var first = sorting.First();
                query = query.OrderBy($"{first.Field} {(first.Direction == "asc" ? "ascending" : "descending")}");
                foreach (var s in sorting.Skip(1))
                {
                    query = ((IOrderedQueryable<Product>)query).ThenBy($"{s.Field} {(s.Direction == "asc" ? "ascending" : "descending")}");
                }
            }
            else
            {
                query = query.OrderBy("Id");
            }

            var data = new List<ProductOverview>();
            var totalCount = await query.CountAsync();
            // Pagination
            if (pagination != null)
            {
                data = await query
                .Skip((pagination.PageNumber) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ProjectTo<ProductOverview>(_mapper.ConfigurationProvider)
                .ToListAsync();
            }
            else
            {
                data = await query.ProjectTo<ProductOverview>(_mapper.ConfigurationProvider).ToListAsync();
            }

            return new PaginationResult<ProductOverview>
            {
                TotalCount = totalCount,
                Data = data
            };
        }

        public async Task AddSupplierAsync(int id, int supplierId)
        {
            var product = _context.Products
                .Include(p => p.Suppliers)
                .FirstOrDefault(p => p.Id == id);
            if (product == null) throw new Exception("Product not found when attempting to add supplier.");

            var supplier = _context.Suppliers
                .FirstOrDefault(s => s.Id == supplierId);
            if (product == null) throw new Exception("Supplier not found when attempting to add to product.");

            if (!product.Suppliers.Contains(supplier))
            {
                product.Suppliers.Add(supplier);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveSupplierAsync(int id, int supplierId)
        {
            var product = _context.Products
                .Include(p => p.Suppliers)
                .FirstOrDefault(p => p.Id == id);
            if (product == null) throw new Exception("Product not found when attempting to remove supplier.");

            var supplier = _context.Suppliers
                .FirstOrDefault(s => s.Id == supplierId);
            if (product == null) throw new Exception("Supplier not found when attempting to remove from product.");

            if (product.Suppliers.Contains(supplier))
            {
                product.Suppliers.Remove(supplier);
                await _context.SaveChangesAsync();
            }
        }
    }
}
