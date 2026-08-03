using Carter;
using Identity_service;
using Identity_service.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDependencies(builder.Configuration);

builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerDocumentation();
}

app.UseHttpsRedirection();


app.UseAuthentication();

app.UseAuthorization();

app.MapCarter();


app.MapGet("/", () => "Identity service is running...");

//app.UseExceptionHandler();
app.Run();