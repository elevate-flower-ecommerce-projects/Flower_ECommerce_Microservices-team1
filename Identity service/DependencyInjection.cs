using Identity_service.Exceptions;
using System.Reflection;

namespace Identity_service;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new NullReferenceException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
           options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure()));

        services.Configure<DriverDocumentStorageOptions>(
            configuration.GetSection(DriverDocumentStorageOptions.SectionName));

        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance =
                    context.HttpContext.Request.Path;

                context.ProblemDetails.Extensions["traceId"] =
                    context.HttpContext.TraceIdentifier;
            };
        });

        services.AddScoped(typeof(IUnitOfWork<ApplicationDbContext>), typeof(UnitOfWork<ApplicationDbContext>));
        services.AddScoped<IDriverDocumentStorage, LocalDriverDocumentStorage>();
        services.AddScoped<IDriverApplicationValidator, DriverApplicationValidator>();
        services.AddScoped<IDriverLoginStatusGuard, DriverLoginStatusGuard>();
        services.AddScoped<IIdentityDataSeeder, IdentityDataSeeder>();
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<IAdminSecurityAudit, AdminSecurityAuditWriter>();
        services.AddSingleton<IAdminLoginAttemptGuard, AdminLoginAttemptGuard>();

        services.AddIdentityConfig();

        services.AddOpenApi();
        services.AddCarter();
        services.AddAuthenticationConfig(configuration);
        services.AddAuthorization();

        services.AddSingleton<IAuthorizationMiddlewareResultHandler, AdminAuthorizationMiddlewareResultHandler>();
        services.AddProblemDetails();

        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

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

            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;

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

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

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
