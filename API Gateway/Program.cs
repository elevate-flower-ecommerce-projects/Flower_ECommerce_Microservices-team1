using API_Gateway.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddGatewayReverseProxy(builder.Configuration);

var app = builder.Build();

app.UseCors("AllowAll");

// Service-to-service endpoints live under /internal on each service. The routes below are
// catch-alls, so without this they would be reachable from the internet; the services also
// require a shared secret, and this keeps the gateway from offering them at all.
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    if (path.StartsWithSegments("/internal", StringComparison.OrdinalIgnoreCase)
        || path.Value?.Contains("/internal/", StringComparison.OrdinalIgnoreCase) == true)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    await next();
});

// Configure the HTTP request pipeline.
app.UseSwaggerDocumentation();

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = "/catalog"
});

app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "Flower E-Commerce API Gateway" }));

app.MapControllers();
app.MapGatewayReverseProxy();

app.Run();
