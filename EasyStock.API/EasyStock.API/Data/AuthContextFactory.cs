using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.IO;

namespace EasyStock.API.Data
{
    public class AuthContextFactory : IDesignTimeDbContextFactory<AuthContext>
    {
        public AuthContext CreateDbContext(string[] args)
        {
            // Assumes appsettings.json is in the current directory or root of the API project
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AuthContext>();
            var connectionString = config.GetConnectionString("AuthConnection");

            optionsBuilder.UseNpgsql(connectionString); // or UseNpgsql, UseSqlite, etc.

            return new AuthContext(optionsBuilder.Options);
        }
    }
}