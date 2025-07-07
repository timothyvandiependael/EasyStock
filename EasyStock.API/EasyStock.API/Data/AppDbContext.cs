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
        public DbSet<StockMovement> StockMovements => Set<StockMovement>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Dispatch> Dispatches => Set<Dispatch>(); 
        public DbSet<DispatchLine> DispatchLines => Set<DispatchLine>();
        public DbSet<Reception> Receptions => Set<Reception>();
        public DbSet<ReceptionLine> ReceptionLines => Set<ReceptionLine>();
        public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
        public DbSet<OrderNumberCounter> OrderNumberCounters => Set<OrderNumberCounter>();


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
            modelBuilder.Entity<Dispatch>().HasKey(o => o.Id);
            modelBuilder.Entity<DispatchLine>().HasKey(o => o.Id);
            modelBuilder.Entity<Reception>().HasKey(o => o.Id);
            modelBuilder.Entity<ReceptionLine>().HasKey(o => o.Id);
            modelBuilder.Entity<UserPermission>().HasKey(o => o.Id);
            modelBuilder.Entity<OrderNumberCounter>().HasKey(o => o.Id);

        }
    }
}
