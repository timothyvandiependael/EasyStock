using Microsoft.EntityFrameworkCore;
using EasyStock.API.Models;

namespace EasyStock.API.Data
{
    public class AuthContext : DbContext
    {
        public AuthContext(DbContextOptions<AuthContext> options) : base(options) { }

        public DbSet<UserAuth> UserAuths => Set<UserAuth>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserAuth>().HasKey(o => o.Id);

        }
    }
}
