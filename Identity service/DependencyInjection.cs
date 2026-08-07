using Carter;
using Identity_service.Abstractions;
using Identity_service.Entities;
using Identity_service.Features.Drivers.Applications.Submit;
using Identity_service.Infrastructure;
using Identity_service.Persistence;
using Identity_service.Services;
using Identity_service.Settings;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repository.Layer;
using Repository.Layer.Interfaces;
using System.Text;

namespace Identity_service;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
           options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure()));

        services.Configure<DriverDocumentStorageOptions>(
            configuration.GetSection(DriverDocumentStorageOptions.SectionName));
        services.Configure<EmailOptions>(
            configuration.GetSection(EmailOptions.SectionName));

        services.AddScoped(typeof(IUnitOfWork<ApplicationDbContext>), typeof(UnitOfWork<ApplicationDbContext>));
        services.AddScoped<IDriverDocumentStorage, LocalDriverDocumentStorage>();
        services.AddScoped<IApplicantNotificationService, SmtpApplicantNotificationService>();
        services.AddScoped<IDriverApplicationValidator, DriverApplicationValidator>();
        services.AddScoped<IDriverLoginStatusGuard, DriverLoginStatusGuard>();
        services.AddScoped<IIdentityDataSeeder, IdentityDataSeeder>();

        services.AddIdentityConfig();

        services.AddOpenApi();
        services.AddCarter();
        services.AddAuthenticationConfig(configuration);
        services.AddAuthorization();

        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        var mappingConfiguration = TypeAdapterConfig.GlobalSettings;
        mappingConfiguration.Scan(assembly);

        services.AddSingleton<IMapper>(new Mapper(mappingConfiguration));

        return services;
    }

    private static IServiceCollection AddIdentityConfig(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.User.RequireUniqueEmail = true;

            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;

            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }

    private static IServiceCollection AddAuthenticationConfig(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT settings are not configured properly.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(o =>
        {
            o.SaveToken = true;
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings!.Key)),
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
            };
        });

        return services;
    }
}
