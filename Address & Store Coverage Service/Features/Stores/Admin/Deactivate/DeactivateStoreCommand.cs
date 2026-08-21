using Flower.Common.StandardizedResponse;
using MediatR;

namespace Address___Store_Coverage_Service.Features.Stores.Admin.Deactivate;

public sealed record DeactivateStoreCommand(Guid StoreId) : IRequest<OperationResult<object>>;
