using Harfi.API.Extensions;
using Harfi.API.Hubs;
using Harfi.API.Middleware;
using Harfi.Models.Entities;
using Harfi.Repositories.Data;
using Harfi.Repositories.Implementations;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Implementations;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════
//  SERVICES — via ServiceExtensions (one line per group)
// ═══════════════════════════════════════════════════════════
builder.Services
    .AddDatabase(builder.Configuration)
    .AddRepositories()
    .AddApplicationServices()
    .AddJwtAuthentication(builder.Configuration)
    .AddSwaggerWithJwt()
    .AddHarfiCors(builder.Configuration)
    .AddControllers();

// 1. تسجيل الـ Repository الخاص بالحرفيين
builder.Services.AddScoped<ICraftsmanRepository, CraftsmanRepository>();

// 2. تسجيل الـ Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICraftsmanService, CraftsmanService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// 3. Seeder
builder.Services.AddScoped<DataSeeder>();

var app = builder.Build();

// ═══════════════════════════════════════════════════════════
//  MIDDLEWARE PIPELINE — ORDER MATTERS
// ═══════════════════════════════════════════════════════════
app.UseMiddleware<GlobalExceptionMiddleware>(); // 1st — catches everything

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Harfi API v1");
        c.RoutePrefix = string.Empty;
    });
}
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseCors("HarfiCors");
app.UseAuthentication();  // must be before Authorization
app.UseAuthorization();
app.MapControllers();

// Ibrahim - Phase 5
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");

// Auto-migrate on startup (Development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Seed default data (admin + demo data)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();
        Console.WriteLine("✅ Seeding completed successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Seeding failed: {ex.Message}");
        Console.WriteLine(ex.InnerException?.Message);
    }
}

app.Run();
