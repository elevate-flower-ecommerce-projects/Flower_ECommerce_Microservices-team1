using Flower.Common.StandardizedResponse;
using Identity_service.Exceptions;
using System.Reflection;
using System.Threading.RateLimiting;

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
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<PasswordResetOptions>(configuration.GetSection(PasswordResetOptions.SectionName));
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));

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
        services.AddScoped<IApplicantNotificationService, SmtpApplicantNotificationService>();
        services.AddScoped<IDriverApplicationValidator, DriverApplicationValidator>();
        services.AddScoped<IRegisterCustomerValidator, RegisterCustomerValidator>();
        services.AddScoped<IDriverLoginStatusGuard, DriverLoginStatusGuard>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IIdentityDataSeeder, IdentityDataSeeder>();
        services.AddScoped<PasswordResetOtpService>();
        services.AddScoped<PasswordResetEmailService>();
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<IAdminSecurityAudit, AdminSecurityAuditWriter>();
        services.AddSingleton<IAdminLoginAttemptGuard, AdminLoginAttemptGuard>();

        services.AddIdentityConfig();

        services.AddCarter();
        services.AddControllers();
        services.AddLoginRateLimiting();
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

    private static IServiceCollection AddLoginRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.OnRejected = (context, cancellationToken) =>
            {
                var httpContext = context.HttpContext;
                var logger = httpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("LoginRateLimiter");
                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                logger.LogWarning("Login request rate limited for IP address {IpAddress}", ipAddress);

                httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                return new ValueTask(httpContext.Response.WriteAsJsonAsync(
                    new OperationResult(
                        Flower.Common.StandardizedResponse.StatusCode.TooManyRequests,
                        "Too many login attempts. Please try again later.",
                        "Too many login attempts. Please try again later."),
                    cancellationToken));
            };

            options.AddPolicy("login", httpContext =>
            {
                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(
                    ipAddress,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });
        });

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
        var jwtSettings = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        if (string.IsNullOrWhiteSpace(jwtSettings.Key))
        {
            jwtSettings.Key = configuration["Jwt:Key"] 
                           ?? configuration["JwtSettings:Secret"] 
                           ?? configuration["Jwt:Secret"] 
                           ?? "YOUR_SUPER_SECRET_KEY_CHANGE_IN_PRODUCTION_MIN_32_CHARS";
        }
        if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
        {
            jwtSettings.Issuer = configuration["Jwt:Issuer"] 
                            ?? configuration["JwtSettings:Issuer"] 
                            ?? "FlowersAuth";
        }
        if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
        {
            jwtSettings.Audience = configuration["Jwt:Audience"] 
                              ?? configuration["JwtSettings:Audience"] 
                              ?? "FlowersApp";
        }

        services.Configure<JwtOptions>(options =>
        {
            options.Key = jwtSettings.Key;
            options.Issuer = jwtSettings.Issuer;
            options.Audience = jwtSettings.Audience;
            options.ExpiryMinutes = jwtSettings.ExpiryMinutes > 0 ? jwtSettings.ExpiryMinutes : 60;
            options.RefreshTokenExpiryDays = jwtSettings.RefreshTokenExpiryDays > 0 ? jwtSettings.RefreshTokenExpiryDays : 7;
        });

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(o =>
        {
            var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.Key);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            o.SaveToken = true;
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                IssuerSigningKey = signingKey,
                IssuerSigningKeys = new[] { signingKey },
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.NameIdentifier,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            o.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var authorization = context.Request.Headers.Authorization.ToString();
                    if (!string.IsNullOrWhiteSpace(authorization))
                    {
                        var token = authorization.Trim();
                        if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            token = token["Bearer ".Length..].Trim();
                        }
                        var match = System.Text.RegularExpressions.Regex.Match(token, @"[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+");
                        if (match.Success)
                        {
                            context.Token = match.Value;
                        }
                    }
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }
}
