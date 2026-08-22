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

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT settings are not configured properly.");

        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.NameIdentifier
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
