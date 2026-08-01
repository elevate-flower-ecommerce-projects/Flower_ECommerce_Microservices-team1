namespace API_Gateway.Extensions;

public static class ReverseProxyExtensions
{
    public static IServiceCollection AddGatewayReverseProxy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"));

        return services;
    }

    public static WebApplication MapGatewayReverseProxy(this WebApplication app)
    {
        app.MapReverseProxy();

        return app;
    }
}
