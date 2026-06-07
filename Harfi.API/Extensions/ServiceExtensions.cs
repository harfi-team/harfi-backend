using Harfi.Models.Entities;
using Harfi.Repositories.Data;
using Harfi.Repositories.Implementations;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Implementations;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Harfi.API.Extensions;

/// <summary>
/// Keeps Program.cs clean — all DI registration lives here.
/// Each team member adds their services in their own extension method.
/// </summary>
public static class ServiceExtensions
{
    // ── DATABASE ──────────────────────────────────────────────
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                config.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly("Harfi.Repositories").UseCompatibilityLevel(110)
            )
        );
        return services;
    }

    // ── REPOSITORIES ─────────────────────────────────────────
    public static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {
        // Generic — covers all entities automatically
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IEmailService, EmailService>();

        // TODO (Hadeer - Phase 2): add ICraftsmanRepository
        // TODO (Habiba - Phase 3): add IJobRepository
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        // TODO (Mazen  - Phase 4): add IReviewRepository
        // ── Repositories ──────────────────────────────────────────────────────
        // Scoped = one instance per HTTP request
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IJobFeedbackRepository, JobFeedbackRepository>();
        // ── Services ──────────────────────────────────────────────────────────
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IJobFeedbackService, JobFeedbackService>();

        // Ibrahim - Phase 5
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        return services;
    }

    // ── SERVICES ──────────────────────────────────────────────
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // Phase 1 — Auth (Esraa)
        services.AddScoped<IAuthService, AuthService>();

        // TODO (Hadeer - Phase 2): services.AddScoped<ICraftsmanService, CraftsmanService>();
        // TODO (Habiba - Phase 3): services.AddScoped<IJobService, JobService>();
        services.AddScoped<IJobService, JobService>();
        // TODO (Mazen  - Phase 4): services.AddScoped<IReviewService, ReviewService>();

        // Ibrahim - Phase 5
        services.AddScoped<IConversationService, ConversationService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAdminConversationService, AdminConversationService>();
        services.AddSignalR();
        services.AddHttpContextAccessor();
        services.AddScoped<IImageservice, Imageservice>();

        // TODO (Ahmed  - Phase 6): services.AddScoped<IAIAgentService, AIAgentService>();

        return services;
    }

    // ── AUTHENTICATION — Identity + JWT ───────────────────────
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration config)
    {
        var jwtSettings = config.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]
            ?? throw new InvalidOperationException(
                "JwtSettings:SecretKey is missing from appsettings.json");

        // ── ASP.NET Core Identity (no cookie auth) ────────────
        services
            .AddIdentityCore<User>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddSignInManager<SignInManager<User>>();

        // ── JWT Bearer ────────────────────────────────────────
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                                                  Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };

                // Support JWT from SignalR query string (Ibrahim - Phase 5)
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        var accessToken = ctx.Request.Query["access_token"];
                        var path = ctx.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/hubs"))
                        {
                            ctx.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();
        return services;
    }

    // ── SWAGGER ───────────────────────────────────────────────
    public static IServiceCollection AddSwaggerWithJwt(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "🔨 Harfi API — حرفي",
                Version = "v1",
                Description = "منصة حرفي — ربط العملاء المصريين بالحرفيين الموثقين"
            });

            // Enable JWT in Swagger UI
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Enter your token only — no need to type Bearer",
                Name = "Bearer",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id   = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }



    // ── CORS ──────────────────────────────────────────────────
    public static IServiceCollection AddHarfiCors(
        this IServiceCollection services,
        IConfiguration config)
    {
        var origins = config.GetSection("AllowedOrigins").Get<string[]>()
                      ?? new[] { "http://localhost:4200" };

        services.AddCors(opt => opt.AddPolicy("HarfiCors", policy =>
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials())); // required for SignalR
        services.AddRagHttpClients(config);


        return services;
    }
}
