using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog_Service.Features.Occasions;

[ApiController]
[Route("occasions")]
public sealed class OccasionsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(OperationResult<IReadOnlyList<OccasionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOccasionsQuery(), cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }
}
