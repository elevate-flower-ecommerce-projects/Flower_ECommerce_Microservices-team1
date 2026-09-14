using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Order___Fulfillment_Service.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Order & Fulfillment Service API",
                Version = "v1"
            });

            options.OperationFilter<CheckoutExamplesOperationFilter>();

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    []
                }
            });
        });

        return services;
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }
}

/// <summary>
/// The generated example fills every field, so "Try it out" would send addressId and gift together
/// with placeholder strings and always get a 422. These examples work as-is with the seeded test
/// customer (scrum23.addresses@flower.local), whose ready cart totals 1600.
/// </summary>
internal sealed class CheckoutExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var endpointName = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<IEndpointNameMetadata>()
            .FirstOrDefault()?.EndpointName;

        IOpenApiAny? example = endpointName switch
        {
            // An empty object is dropped from the document, and a null addressId also means "default address".
            "PreviewCheckout" => new OpenApiObject { ["addressId"] = new OpenApiNull() },
            "PlaceOrder" => new OpenApiObject
            {
                ["paymentMethod"] = new OpenApiInteger(1),
                ["expectedTotal"] = new OpenApiDouble(1600)
            },
            _ => null
        };

        if (example is null)
            return;

        if (operation.RequestBody is not null && operation.RequestBody.Content.TryGetValue("application/json", out var media))
            media.Example = example;

        var idempotencyKey = operation.Parameters?.FirstOrDefault(parameter => parameter.Name == "Idempotency-Key");
        if (idempotencyKey is not null)
        {
            idempotencyKey.Description = "A new UUID for each checkout attempt (reuse it only to retry the same attempt). "
                + "Payment method: 1 Cash on Delivery, 2 Card.";
        }
    }
}
