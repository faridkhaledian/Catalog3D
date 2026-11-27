using Catalog3D.Application.Common;
using Catalog3D.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Catalog3D.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");
        var configuration = provider.GetRequiredService<IConfiguration>();

        // 1) اعمال Migrationهای معوق (اگر دیتابیس نبود، ساخته می‌شود)
        var db = provider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        // 2) ساخت نقش Admin
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync(Roles.Admin))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
            EnsureSucceeded(roleResult, "ساخت نقش Admin");
        }

        // 3) ساخت کاربر ادمین از تنظیمات (User Secrets یا متغیر محیطی؛ هرگز در کد نوشته نمی‌شود)
        var email = configuration["AdminUser:Email"];
        var password = configuration["AdminUser:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("AdminUser:Email یا AdminUser:Password تنظیم نشده است؛ کاربر ادمین ساخته نشد.");
            return;
        }

        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, password);
            EnsureSucceeded(createResult, "ساخت کاربر ادمین");

            logger.LogInformation("کاربر ادمین ساخته شد.");
        }

        if (!await userManager.IsInRoleAsync(user, Roles.Admin))
        {
            var addRoleResult = await userManager.AddToRoleAsync(user, Roles.Admin);
            EnsureSucceeded(addRoleResult, "افزودن کاربر به نقش Admin");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (result.Succeeded)
            return;

        var errors = string.Join("; ", result.Errors.Select(e => e.Description));
        throw new InvalidOperationException($"{operation} ناموفق بود: {errors}");
    }
}