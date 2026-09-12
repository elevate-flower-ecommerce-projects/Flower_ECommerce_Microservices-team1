using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Identity_service.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Identity Service API",
                Version = "v1"
            });

            options.CustomSchemaIds(type => type.FullName?.Replace('+', '.') ?? type.Name);
            options.OperationFilter<DriverApplicationUploadOperationFilter>();
            options.OperationFilter<FlattenFormRequestOperationFilter>();

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
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Service API v1");
        });

        app.MapGet("/", () => Results.Redirect("/swagger"));

        return app;
    }
}

/// <summary>
/// Minimal APIs describe a <c>[FromForm]</c> DTO as a single "request" object, which Swagger UI
/// renders as one JSON text box that the server cannot bind. This spreads the DTO into separate
/// form fields and lists enums by name, since form binding accepts the names.
/// </summary>
internal sealed class FlattenFormRequestOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.RequestBody is null
            || !operation.RequestBody.Content.TryGetValue("multipart/form-data", out var media)
            || media.Schema?.Properties is not { Count: 1 } wrapper)
        {
            return;
        }

        var (parameterName, wrappedSchema) = wrapper.Single();
        if (wrappedSchema.Reference is null
            || !context.SchemaRepository.Schemas.TryGetValue(wrappedSchema.Reference.Id, out var dtoSchema))
        {
            return;
        }

        var dtoType = context.ApiDescription.ParameterDescriptions
            .FirstOrDefault(parameter => string.Equals(parameter.Name, parameterName, StringComparison.OrdinalIgnoreCase))
            ?.Type;

        var flattened = new OpenApiSchema { Type = "object" };
        foreach (var (fieldName, fieldSchema) in dtoSchema.Properties)
        {
            var clrType = dtoType?.GetProperty(fieldName, System.Reflection.BindingFlags.IgnoreCase
                | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance)?.PropertyType;
            var enumType = clrType is null ? null : Nullable.GetUnderlyingType(clrType) ?? clrType;

            flattened.Properties[fieldName] = enumType is { IsEnum: true }
                ? new OpenApiSchema
                {
                    Type = "string",
                    Enum = Enum.GetNames(enumType)
                        .Select(name => (Microsoft.OpenApi.Any.IOpenApiAny)new Microsoft.OpenApi.Any.OpenApiString(name))
                        .ToList()
                }
                : fieldSchema;
        }

        media.Schema = flattened;
        media.Encoding.Clear();
    }
}

internal sealed class DriverApplicationUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!string.Equals(context.ApiDescription.HttpMethod, HttpMethods.Post, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(context.ApiDescription.RelativePath?.TrimEnd('/'), "drivers/applications", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        operation.RequestBody = new OpenApiRequestBody
        {
            Required = true,
            Content =
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Required = new HashSet<string>
                        {
                            "country",
                            "firstLegalName",
                            "secondLegalName",
                            "phone",
                            "email",
                            "nationalId",
                            "vehicleType",
                            "vehiclePlateNumber",
                            "gender",
                            "password",
                            "confirmPassword",
                            "documents"
                        },
                        Properties =
                        {
                            ["country"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("Egypt") },
                            ["firstLegalName"] = new OpenApiSchema { Type = "string" },
                            ["secondLegalName"] = new OpenApiSchema { Type = "string" },
                            ["fullName"] = new OpenApiSchema { Type = "string", Description = "Backward-compatible full name field. New clients should send firstLegalName and secondLegalName." },
                            ["phone"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("01020000001") },
                            ["email"] = new OpenApiSchema { Type = "string", Format = "email" },
                            ["nationalId"] = new OpenApiSchema { Type = "string" },
                            ["vehicleType"] = new OpenApiSchema
                            {
                                Type = "integer",
                                Format = "int32",
                                Description = "0 = Motorcycle, 1 = Car"
                            },
                            ["vehiclePlateNumber"] = new OpenApiSchema { Type = "string" },
                            ["gender"] = new OpenApiSchema { Type = "string", Description = "Male or Female" },
                            ["password"] = new OpenApiSchema { Type = "string", Format = "password" },
                            ["confirmPassword"] = new OpenApiSchema { Type = "string", Format = "password" },
                            ["documents"] = new OpenApiSchema
                            {
                                Type = "array",
                                Items = new OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "binary"
                                },
                                Description = "Upload at least one PDF, JPG, or PNG identity/license document."
                            }
                        }
                    },
                    Encoding =
                    {
                        ["documents"] = new OpenApiEncoding
                        {
                            Style = ParameterStyle.Form,
                            Explode = true
                        }
                    }
                }
            }
        };
    }
}
