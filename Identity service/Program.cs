using Identity_service;
using Identity_service.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDependencies(builder.Configuration);

builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

await app.MigrateAndSeedIdentityDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapCarter();
app.MapControllers();

app.UseExceptionHandler();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "Identity Service", timestamp = DateTime.UtcNow }));

app.Run();
