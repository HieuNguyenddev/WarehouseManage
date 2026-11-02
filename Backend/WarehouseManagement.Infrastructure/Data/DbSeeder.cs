
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManagement.Application.Interfaces;
using WarehouseManagement.Domain.Entities;

namespace WarehouseManagement.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var context =
            services.GetRequiredService<ApplicationDbContext>();

        var hasher =
            services.GetRequiredService<IPasswordHasher>();

        var configuration =
            services.GetRequiredService<IConfiguration>();

        // 1. Tạo Role mặc định
        var adminRole = await EnsureRoleAsync(
            context,
            "Admin",
            "Quản trị hệ thống");

        await EnsureRoleAsync(
            context,
            "Staff",
            "Nhân viên");

        // 2. Đọc thông tin Admin từ appsettings.json
        var email = configuration["SeedAdmin:Email"]
            ?.Trim()
            .ToLowerInvariant();

        var password = configuration["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        // 3. Kiểm tra Admin đã tồn tại
        var adminExists = await context.Users
            .AnyAsync(x => x.Email == email);

        if (adminExists)
        {
            return;
        }

        // 4. Tạo tài khoản Admin
        var admin = new User
        {
            FullName = "Administrator",
            Email = email,
            PasswordHash = hasher.Hash(password),
            RoleId = adminRole.Id,
            IsActive = true
        };

        context.Users.Add(admin);

        await context.SaveChangesAsync();
    }

    private static async Task<Role> EnsureRoleAsync(
        ApplicationDbContext context,
        string name,
        string description)
    {
        var role = await context.Roles
            .FirstOrDefaultAsync(x => x.Name == name);

        if (role != null)
        {
            return role;
        }

        role = new Role
        {
            Name = name,
            Description = description,
            IsActive = true
        };

        context.Roles.Add(role);

        await context.SaveChangesAsync();

        return role;
    }
}
