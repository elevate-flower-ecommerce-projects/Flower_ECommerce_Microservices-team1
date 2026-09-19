using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Persistence.Repositories;
using NotificationService.Shared.Models;
using NotificationService.Shared.Services;

namespace NotificationService.Extensions;

public static class RegisterInfrastructure
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(cfg =>
        {
            cfg.AddConsumers(typeof(RegisterInfrastructure).Assembly);
            cfg.UsingRabbitMq((context, cfg) =>
            {
                // 2. Configure RabbitMQ connection
                var rabbitMqSettings = configuration.GetSection("RabbitMQ").Get<RabbitMQSettings>();
                var rabbitMqHost = rabbitMqSettings?.HostName ?? "localhost";
                var rabbitMqUser = rabbitMqSettings?.UserName ?? "guest";
                var rabbitMqPass = rabbitMqSettings?.Password ?? "guest";

                cfg.Host(rabbitMqHost, "/", hostConfigurator => {
                    hostConfigurator.Username(rabbitMqUser);
                    hostConfigurator.Password(rabbitMqPass);
                });

                // 3. Configure retry policy for resilience
                cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

                // 4. Configure endpoints (this will auto-configure queues for your consumers)
                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddHttpClient<AuthServiceClient>(client =>
        {
            client.BaseAddress = new Uri("http://identityservice:8080");
        });

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterInfrastructure).Assembly));

        services.AddDbContext<NotificationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("NotificationDb");
            options.UseSqlServer(connectionString);
        });
        var credentialsPath = configuration["Firebase:CredentialsPath"]
          ?? throw new InvalidOperationException("Firebase:CredentialsPath is not configured.");

        // FirebaseApp.Create must run exactly once per process, before any
        // FirebaseMessaging.DefaultInstance call. Guard against double-init
        // (hot reload, multiple calls to this method, etc.).
        if (FirebaseApp.DefaultInstance is null)
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(credentialsPath)
            });
        }

        services.AddScoped<FirebaseFcmSender>();
        services.AddScoped(typeof(Repository<>));
        return services;
    }
}
