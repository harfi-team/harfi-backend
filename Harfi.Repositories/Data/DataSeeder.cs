using Harfi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Harfi.Repositories.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Only seed if no admin exists yet
        bool adminExists = await context.Users
            .AnyAsync(u => u.Role == "admin");

        if (adminExists) return;

        var admin = new User
        {
            Name = "Harfi Admin",
            Email = "admin@harfi.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@1234"),
            Role = "admin",
            Phone = "01000000000",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Users.AddAsync(admin);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Admin seeded: admin@harfi.com / Admin@1234");
    }
}