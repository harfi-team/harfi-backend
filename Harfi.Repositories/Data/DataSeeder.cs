using Harfi.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace Harfi.Repositories.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(UserManager<User> userManager)
    {
        // Only seed if no admin exists yet
        var adminExists = await userManager.FindByEmailAsync("admin@harfi.com");

        if (adminExists is not null) return;

        var admin = new User
        {
            UserName = "admin@harfi.com",
            Name = "Harfi Admin",
            Email = "admin@harfi.com",
            Role = "admin",
            Phone = "01000000000",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(admin, "Admin@1234");

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            Console.WriteLine($"⚠ Failed to seed admin: {errors}");
            return;
        }

        Console.WriteLine("✅ Admin seeded: admin@harfi.com / Admin@1234");
    }
}