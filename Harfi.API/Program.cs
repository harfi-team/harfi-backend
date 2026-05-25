using Harfi.API.Extensions;
using Harfi.API.Hubs;
using Harfi.API.Middleware;
using Harfi.Models.Entities;
using Harfi.Repositories.Data;
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

// Seed default admin
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    await DataSeeder.SeedAsync(userManager);
}

app.Run();
