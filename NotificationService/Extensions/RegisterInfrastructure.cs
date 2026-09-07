using MassTransit;
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
                var rabbitMqHost = rabbitMqSettings?.Host ?? "localhost";
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
        return services;
    }
}
