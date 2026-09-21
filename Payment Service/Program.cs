using System.Reflection;
using System.Security.Claims;
using System.Text;
using Carter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Payment_Service.Extensions;
using Payment_Service.Features.Payments;
using Payment_Service.Infrastructure.Clients;
using Payment_Service.Infrastructure.Stripe;
using Payment_Service.Persistence;
using Payment_Service.Settings;
using Repository.Layer;
using Repository.Layer.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection(StripeOptions.SectionName));
builder.Services.Configure<InternalApiOptions>(builder.Configuration.GetSection(InternalApiOptions.SectionName));
builder.Services.Configure<DownstreamServicesOptions>(builder.Configuration.GetSection(DownstreamServicesOptions.SectionName));
builder.Services.AddScoped(typeof(IUnitOfWork<PaymentDbContext>), typeof(UnitOfWork<PaymentDbContext>));
builder.Services.AddSingleton<IStripeGateway, StripeGateway>();
builder.Services.AddScoped<IPaymentSettlement, PaymentSettlement>();

var downstream = builder.Configuration.GetSection(DownstreamServicesOptions.SectionName).Get<DownstreamServicesOptions>()
    ?? throw new InvalidOperationException("Downstream service URLs (Services section) are not configured.");

builder.Services.AddTransient<InternalSecretHandler>();
builder.Services.AddHttpClient<IOrderClient, OrderClient>(client =>
{
    client.BaseAddress = new Uri(downstream.OrderBaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(downstream.TimeoutSeconds);
}).AddHttpMessageHandler<InternalSecretHandler>();
builder.Services.AddHttpClient<ICartClient, CartClient>(client =>
{
    client.BaseAddress = new Uri(downstream.CartBaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(downstream.TimeoutSeconds);
}).AddHttpMessageHandler<InternalSecretHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCarter();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

var jwtSettings = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwtSettings.Key))
    jwtSettings.Key = builder.Configuration["Jwt:Key"] ?? builder.Configuration["JwtSettings:Secret"] ?? builder.Configuration["Jwt:Secret"] ?? "YOUR_SUPER_SECRET_KEY_CHANGE_IN_PRODUCTION_MIN_32_CHARS";
if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
    jwtSettings.Issuer = builder.Configuration["Jwt:Issuer"] ?? builder.Configuration["JwtSettings:Issuer"] ?? "FlowersAuth";
if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
    jwtSettings.Audience = builder.Configuration["Jwt:Audience"] ?? builder.Configuration["JwtSettings:Audience"] ?? "FlowersApp";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            IssuerSigningKey = signingKey,
            IssuerSigningKeys = [signingKey],
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.NameIdentifier,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

await app.MigratePaymentDatabaseAsync();

app.UseSwaggerDocumentation();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "Flower E-Commerce Payment Service" }));
app.MapCarter();

app.Run();
