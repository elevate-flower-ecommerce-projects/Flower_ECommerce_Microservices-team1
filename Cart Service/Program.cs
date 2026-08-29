using System.Reflection;
using System.Security.Claims;
using System.Text;
using Cart_Service.Extensions;
using Cart_Service.Features.Cart;
using Cart_Service.Infrastructure.Catalog;
using Cart_Service.Persistence;
using Cart_Service.Settings;
using Carter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repository.Layer;
using Repository.Layer.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<CartDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<CatalogOptions>(builder.Configuration.GetSection(CatalogOptions.SectionName));
builder.Services.AddScoped(typeof(IUnitOfWork<CartDbContext>), typeof(UnitOfWork<CartDbContext>));
builder.Services.AddScoped<ICartResponseBuilder, CartResponseBuilder>();

var catalogOptions = builder.Configuration.GetSection(CatalogOptions.SectionName).Get<CatalogOptions>()
    ?? throw new InvalidOperationException("Catalog settings are not configured properly.");

builder.Services.AddHttpClient<ICatalogClient, CatalogClient>(client =>
{
    client.BaseAddress = new Uri(catalogOptions.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(catalogOptions.TimeoutSeconds);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCarter();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

var jwtSettings = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwtSettings.Key))
{
    jwtSettings.Key = builder.Configuration["Jwt:Key"] 
                   ?? builder.Configuration["JwtSettings:Secret"] 
                   ?? builder.Configuration["Jwt:Secret"] 
                   ?? "YOUR_SUPER_SECRET_KEY_CHANGE_IN_PRODUCTION_MIN_32_CHARS";
}
if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
{
    jwtSettings.Issuer = builder.Configuration["Jwt:Issuer"] 
                    ?? builder.Configuration["JwtSettings:Issuer"] 
                    ?? "FlowersAuth";
}
if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
{
    jwtSettings.Audience = builder.Configuration["Jwt:Audience"] 
                      ?? builder.Configuration["JwtSettings:Audience"] 
                      ?? "FlowersApp";
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.Key);
        var signingKey = new SymmetricSecurityKey(keyBytes);

        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
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

        options.Events = new JwtBearerEvents
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

builder.Services.AddAuthorization();
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

await app.MigrateCartDatabaseAsync();

app.UseSwaggerDocumentation();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "Flower E-Commerce Cart Service" }));

app.MapCarter();

app.Run();
