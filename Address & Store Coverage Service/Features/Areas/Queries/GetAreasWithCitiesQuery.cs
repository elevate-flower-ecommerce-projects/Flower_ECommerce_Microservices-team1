using Address___Store_Coverage_Service.Features.Areas.DTOs;
using Flower.Common.StandardizedResponse;
using MediatR;

namespace Address___Store_Coverage_Service.Features.Areas.Queries;

public sealed record GetAreasWithCitiesQuery : IRequest<OperationResult<IReadOnlyList<AreaWithCitiesDto>>>;
