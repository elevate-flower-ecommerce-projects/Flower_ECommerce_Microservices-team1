using System.Security.Claims;
using Carter;
using Flower.Common.StandardizedResponse;
using Identity_service.Abstractions;
using Identity_service.Abstractions.Seeding;
using Identity_service.Entities;
using Identity_service.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;

namespace Identity_service.Features.Drivers.Applications.Admin;

/// <summary>
/// Backend endpoints for admin review of prospective driver applications.
/// </summary>
public sealed class AdminDriverApplicationsEndpoint : ICarterModule
{
    #region Routes

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin/drivers/applications")
            .WithTags("Admin Driver Applications")
            .RequireAuthorization(policy => policy.RequireRole(DefaultRoles.Admin.Name));

        group.MapGet("/", ListAsync)
            .Produces<OperationResult<PagedResponse<AdminDriverApplicationSummaryResponse>>>();

        group.MapGet("/{id:guid}", GetDetailAsync)
            .Produces<OperationResult<AdminDriverApplicationDetailResponse>>()
            .Produces<OperationResult>(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/documents/{documentId:guid}/download", DownloadDocumentAsync)
            .Produces(StatusCodes.Status200OK)
            .Produces<OperationResult>(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/approve", ApproveAsync)
            .Produces<OperationResult<ReviewDriverApplicationResponse>>()
            .Produces<OperationResult>(StatusCodes.Status404NotFound)
            .Produces<OperationResult>(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/reject", RejectAsync)
            .Produces<OperationResult<ReviewDriverApplicationResponse>>()
            .Produces<OperationResult<Dictionary<string, string[]>>>(StatusCodes.Status422UnprocessableEntity)
            .Produces<OperationResult>(StatusCodes.Status404NotFound)
            .Produces<OperationResult>(StatusCodes.Status409Conflict);
    }

    #endregion

    #region Endpoint handlers

    private static async Task<IResult> ListAsync(
        DriverApplicationStatus? status,
        int? page,
        int? pageSize,
        ISender sender,
        CancellationToken cancellationToken)
        => (await sender.Send(new ListAdminDriverApplicationsQuery(status, page ?? 1, pageSize ?? 20), cancellationToken))
            .ToHttpResult();

    private static async Task<IResult> GetDetailAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
        => (await sender.Send(new GetAdminDriverApplicationDetailQuery(id), cancellationToken))
            .ToHttpResult();

    private static async Task<IResult> ApproveAsync(
        Guid id,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken cancellationToken)
        => (await sender.Send(new ApproveDriverApplicationCommand(id, ResolveReviewer(user)), cancellationToken))
            .ToHttpResult();

    private static async Task<IResult> RejectAsync(
        Guid id,
        RejectDriverApplicationRequest request,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken cancellationToken)
        => (await sender.Send(new RejectDriverApplicationCommand(id, ResolveReviewer(user), request.RejectionReason), cancellationToken))
            .ToHttpResult();

    private static async Task<IResult> DownloadDocumentAsync(
        Guid id,
        Guid documentId,
        IUnitOfWork<ApplicationDbContext> unitOfWork,
        IDriverDocumentStorage documentStorage,
        CancellationToken cancellationToken)
    {
        var document = await unitOfWork.Repository<DriverDocument, Guid>()
            .Query()
            .Where(driverDocument => driverDocument.ApplicationId == id && driverDocument.Id == documentId)
            .Select(driverDocument => new
            {
                driverDocument.FileUrl,
                driverDocument.OriginalFileName,
                driverDocument.ContentType
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (document is null)
            return OperationResultFactory.NotFound(message: "Driver document was not found.").ToHttpResult();

        var stream = await documentStorage.OpenReadAsync(document.FileUrl, cancellationToken);
        return stream is null
            ? OperationResultFactory.NotFound(message: "Driver document file was not found.").ToHttpResult()
            : Results.File(stream, document.ContentType, document.OriginalFileName);
    }

    #endregion

    #region Helpers

    private static string ResolveReviewer(ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.Identity?.Name
            ?? "PendingAuthAdmin";

    #endregion
}
