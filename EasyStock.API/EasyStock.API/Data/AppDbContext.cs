using Microsoft.EntityFrameworkCore;
using EasyStock.API.Models;

namespace EasyStock.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
        public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();
        public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
        public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
        public DbSet<StockMovement> StockMovement => Set<StockMovement>();
        public DbSet<User> Users => Set<User>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>().HasKey(o => o.Id);
            modelBuilder.Entity<Supplier>().HasKey(o => o.Id);
            modelBuilder.Entity<Category>().HasKey(o => o.Id);
            modelBuilder.Entity<Client>().HasKey(o => o.Id);
            modelBuilder.Entity<SalesOrder>().HasKey(o => o.Id);
            modelBuilder.Entity<SalesOrderLine>().HasKey(o => o.Id);
            modelBuilder.Entity<PurchaseOrder>().HasKey(o => o.Id);
            modelBuilder.Entity<PurchaseOrderLine>().HasKey(o => o.Id);
            modelBuilder.Entity<StockMovement>().HasKey(o => o.Id);
            modelBuilder.Entity<User>().HasKey(o => o.Id);
            
        }
    }
}
