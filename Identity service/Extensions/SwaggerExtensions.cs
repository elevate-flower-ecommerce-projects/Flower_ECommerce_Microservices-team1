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
            options.CustomSchemaIds(type => type.FullName?.Replace('+', '.') ?? type.Name);
            options.OperationFilter<DriverApplicationUploadOperationFilter>();
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Identity Service API",
                Version = "v1"
            });

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
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            });
        }

        return app;
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
                            "fullName",
                            "phone",
                            "email",
                            "nationalId",
                            "vehicleType",
                            "vehiclePlateNumber",
                            "password",
                            "confirmPassword",
                            "documents"
                        },
                        Properties =
                        {
                            ["fullName"] = new OpenApiSchema { Type = "string" },
                            ["phone"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("01020000001") },
                            ["email"] = new OpenApiSchema { Type = "string", Format = "email" },
                            ["nationalId"] = new OpenApiSchema { Type = "string" },
                            ["vehicleType"] = new OpenApiSchema
                            {
                                Type = "integer",
                                Format = "int32",
                                Description = "0 = Motorcycle, 1 = Car, 2 = Van"
                            },
                            ["vehiclePlateNumber"] = new OpenApiSchema { Type = "string" },
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
