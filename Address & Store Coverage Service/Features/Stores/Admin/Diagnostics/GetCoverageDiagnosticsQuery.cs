using Address___Store_Coverage_Service.Contracts.Stores;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Address___Store_Coverage_Service.Features.Stores.Admin.Diagnostics;

public sealed record GetCoverageDiagnosticsQuery : IRequest<OperationResult<CoverageDiagnosticsResponse>>;
