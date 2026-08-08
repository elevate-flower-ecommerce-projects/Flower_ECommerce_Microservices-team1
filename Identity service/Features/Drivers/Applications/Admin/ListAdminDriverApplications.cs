using Flower.Common.StandardizedResponse;
using Identity_service.Entities;
using Identity_service.Persistence;
using MediatR;
using Repository.Layer.Interfaces;
using System.Linq.Expressions;

namespace Identity_service.Features.Drivers.Applications.Admin;

public sealed record ListAdminDriverApplicationsQuery(
    DriverApplicationStatus? Status,
    int Page,
    int PageSize) : IRequest<OperationResult<object>>;

public sealed class ListAdminDriverApplicationsHandler(IUnitOfWork<ApplicationDbContext> unitOfWork)
    : IRequestHandler<ListAdminDriverApplicationsQuery, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(
        ListAdminDriverApplicationsQuery request,
        CancellationToken cancellationToken)
    {
        #region Normalize paging

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize is <= 0 or > 100 ? 20 : request.PageSize;

        #endregion

        #region Build filter

        Expression<Func<DriverApplication, bool>>? predicate = request.Status is null
            ? null
            : driverApplication => driverApplication.Status == request.Status;

        #endregion

        #region Get projected page from repository

        var paged = await unitOfWork.Repository<DriverApplication, Guid>()
            .GetPageSelectAsync(
                page,
                pageSize,
                predicate,
                driverApplication => new AdminDriverApplicationSummaryResponse(
                driverApplication.Id,
                driverApplication.Status,
                $"{driverApplication.User!.FirstName} {driverApplication.User.LastName}",
                driverApplication.User.Email!,
                driverApplication.User.PhoneNumber,
                driverApplication.User.DriverProfile!.NationalId,
                driverApplication.User.DriverProfile.VehicleType,
                driverApplication.User.DriverProfile.PlateNumber,
                driverApplication.Documents.Count,
                driverApplication.SubmittedAt,
                driverApplication.ReviewedBy,
                driverApplication.ReviewedAt),
                query => query.OrderByDescending(driverApplication => driverApplication.SubmittedAt));

        #endregion

        return OperationResultFactory.Success<object>(
            new PagedResponse<AdminDriverApplicationSummaryResponse>(page, pageSize, paged.TotalCount, paged.Items));
    }
}
