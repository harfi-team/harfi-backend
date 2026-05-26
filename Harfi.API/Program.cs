using Harfi.API.Extensions;
using Harfi.API.Middleware;
using Harfi.Repositories.Data;
using Harfi.Repositories.Implementations;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Implementations;
using Harfi.Services.Interfaces;
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

// 2. تسجيل الـ Services الثلاثة الخاصة بكِ
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICraftsmanService, CraftsmanService>();
builder.Services.AddScoped<IAdminService, AdminService>();

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

app.UseHttpsRedirection();
app.UseCors("HarfiCors");
app.UseAuthentication();  // must be before Authorization
app.UseAuthorization();
app.MapControllers();

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
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DataSeeder.SeedAsync(db);
}
app.MapControllers();

app.Run();
