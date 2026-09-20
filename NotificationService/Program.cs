
using NotificationService.Extensions;
using NotificationService.Shared.Interfaces;
using Scalar.AspNetCore;

namespace NotificationService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddInfrastructure(builder.Configuration);
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(option =>
            {
                option.Title = "Notifications API";
                option.Theme = ScalarTheme.BluePlanet;
                option.ShowSidebar = true;
                option.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
            });
        }
        var globalGroup = app.MapGroup("");

        var endpointDefinitions = typeof(Program).Assembly
            .GetTypes()
            .Where(t => typeof(IEndpoint).IsAssignableFrom(t) && !t.IsAbstract)
            .Select(Activator.CreateInstance)
            .Cast<IEndpoint>();

        foreach (var endpoint in endpointDefinitions)
        {
            endpoint.MapEndpoint(globalGroup);
        }
        app.ApplyDatabaseMigrations(app.Logger);
        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
