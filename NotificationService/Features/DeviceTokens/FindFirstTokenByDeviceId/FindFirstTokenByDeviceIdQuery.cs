using Microsoft.EntityFrameworkCore;
using NotificationService.Domain;
using NotificationService.Infrastructure.Persistence.Repositories;
using NotificationService.Shared.Interfaces;
using NotificationService.Shared.Response;

namespace NotificationService.Features.DeviceTokens.FindFirstTokenByDeviceId;

public record FindFirstTokenByDeviceIdQuery
(
    string DeviceId, string UserId
) : IQuery<FindByDeviceIdResponse>;

public record FindByDeviceIdResponse
(
   Guid Id 
);  

public class FindFirstTokenByDeviceIdQueryHandler(Repository<DeviceToken> repository)
    : IQueryHandler<FindFirstTokenByDeviceIdQuery, FindByDeviceIdResponse>
{
    public async Task<RequestResult<FindByDeviceIdResponse>> Handle(FindFirstTokenByDeviceIdQuery request, CancellationToken cancellationToken)
    {
        var deviceToken = await repository.Get(dt => dt.DeviceId == request.DeviceId 
        && dt.UserId == request.UserId)
            .Select(dt => new FindByDeviceIdResponse(
                dt.Id
            )).FirstOrDefaultAsync(cancellationToken);
        if (deviceToken == null)
            return RequestResult<FindByDeviceIdResponse>.Failure(ResultCode.DeviceTokenNotFound);

        return RequestResult<FindByDeviceIdResponse>.succeeded(deviceToken, ResultCode.DeviceTokenFoundSuccessfully);
    }
}