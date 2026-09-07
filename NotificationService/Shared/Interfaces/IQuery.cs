using MediatR;
using NotificationService.Shared.Response;

namespace NotificationService.Shared.Interfaces;

public interface IQuery<TResult> : IRequest<RequestResult<TResult>>
{
}

public interface IQueryHandler<TQuery, TResult> : IRequestHandler<TQuery, RequestResult<TResult>>
    where TQuery : IQuery<TResult>
{
}
