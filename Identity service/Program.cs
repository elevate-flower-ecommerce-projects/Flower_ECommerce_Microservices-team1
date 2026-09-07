using Identity_service;
using Identity_service.Extensions;

// The web root is resolved while the builder is created, and it stays null when wwwroot is
// missing, which would make UseStaticFiles serve nothing. Creating it first keeps avatars
// reachable on a fresh checkout.
Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));

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

// Serves avatars from wwwroot. Driver documents stay private and are downloaded through an
// authorized endpoint instead.
app.UseStaticFiles();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapCarter();
app.MapControllers();

app.UseExceptionHandler();

app.Run();
