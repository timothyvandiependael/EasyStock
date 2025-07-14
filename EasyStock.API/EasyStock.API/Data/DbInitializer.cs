using EasyStock.API.Repositories;
using EasyStock.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using EasyStock.API.Services;

namespace EasyStock.API.Data
{
    public class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var appContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var authContext = scope.ServiceProvider.GetRequiredService<AuthContext>();

            var userAuthRepo = scope.ServiceProvider.GetRequiredService<IUserAuthRepository>();
            var userRepo = scope.ServiceProvider.GetRequiredService<IRepository<User>>();

            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

            await authContext.Database.MigrateAsync();
            await appContext.Database.MigrateAsync();

            var existingUser = await userAuthRepo.GetByUserNameAsync("tva");
            if (existingUser == null)
            {
                var newAuthUser = new UserAuth
                {
                    UserName = "tva",
                    Role = Common.UserRole.Admin,

                    CrDate = DateTime.UtcNow,
                    LcDate = DateTime.UtcNow,
                    CrUserId = "sys",
                    LcUserId = "sys",
                    PasswordHash = string.Empty,
                    MustChangePassword = true
                };

                var seedPassword = Environment.GetEnvironmentVariable("SEED_USER_PASSWORD") ?? "DefaultDemoPassword";

                newAuthUser.PasswordHash = authService.HashPassword(newAuthUser, seedPassword);

                await userAuthRepo.AddAsync(newAuthUser);

                var newUser = new User
                {
                    UserName = newAuthUser.UserName,
                    CrDate = DateTime.UtcNow,
                    LcDate = DateTime.UtcNow,
                    CrUserId = "sys",
                    LcUserId = "sys"
                };

                await userRepo.AddAsync(newUser);

                await appContext.SaveChangesAsync();
            }
        }
    }
}
