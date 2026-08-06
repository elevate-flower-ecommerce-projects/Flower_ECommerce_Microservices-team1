using Carter;
using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Identity_service.Features.Drivers.Applications.Submit;

/// <summary>
/// HTTP endpoint for prospective drivers submitting onboarding applications.
/// </summary>
public sealed class SubmitDriverApplicationEndpoint : ICarterModule
{
    #region Routes

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/drivers/applications")
            .WithTags("Driver Applications");

        group.MapPost("/", SubmitAsync)
            .Accepts<SubmitDriverApplicationRequest>("multipart/form-data")
            .Produces<OperationResult<SubmitDriverApplicationResponse>>(StatusCodes.Status201Created)
            .Produces<OperationResult<Dictionary<string, string[]>>>(StatusCodes.Status422UnprocessableEntity)
            .AllowAnonymous()
            .DisableAntiforgery();
    }

    #endregion

    #region Handlers

    private static async Task<IResult> SubmitAsync(
        [FromForm] SubmitDriverApplicationRequest request,
        HttpRequest httpRequest,
        ISender sender,
        CancellationToken cancellationToken)
    {
        // Swagger/minimal APIs may not bind file collections to the DTO consistently,
        // so the raw multipart files are used as a fallback.
        var documents = request.Documents ?? [];
        if (documents.Count == 0 && httpRequest.HasFormContentType)
        {
            var form = await httpRequest.ReadFormAsync(cancellationToken);
            documents = [.. form.Files];
        }

        var result = await sender.Send(new SubmitDriverApplicationCommand(
            request.FullName,
            request.Phone,
            request.Email,
            request.NationalId,
            request.VehicleType,
            request.VehiclePlateNumber,
            request.Password,
            request.ConfirmPassword,
            documents), cancellationToken);

        return result.ToHttpResult();
    }

    #endregion
}
