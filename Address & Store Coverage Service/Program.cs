using System.Reflection;
using System.Security.Claims;
using System.Text;
using Address___Store_Coverage_Service.Extensions;
using Address___Store_Coverage_Service.Features.Addresses.Create;
using Address___Store_Coverage_Service.Persistence;
using Address___Store_Coverage_Service.Services.GeoLookup;
using Address___Store_Coverage_Service.Settings;
using Carter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repository.Layer;
using Repository.Layer.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<AddressDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddScoped(typeof(IUnitOfWork<AddressDbContext>), typeof(UnitOfWork<AddressDbContext>));
builder.Services.AddScoped<IAddressDataSeeder, AddressDataSeeder>();
builder.Services.AddScoped<IGeoLookupService, StoreCoverageGeoLookupService>();
builder.Services.AddScoped<ICreateAddressValidator, CreateAddressValidator>();
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

await app.InitializeAddressDatabaseAsync();

app.UseSwaggerDocumentation();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "Flower E-Commerce Address & Store Coverage Service" }));

app.MapCarter();

app.Run();
