using Identity_service;
using Identity_service.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDependencies(builder.Configuration);

builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

//await app.MigrateAndSeedIdentityDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerDocumentation();
}

app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapCarter();

app.UseExceptionHandler();

app.Run();
