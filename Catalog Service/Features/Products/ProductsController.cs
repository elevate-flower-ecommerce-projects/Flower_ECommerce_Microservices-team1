using Flower.Common.StandardizedResponse;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog_Service.Features.Products;

[ApiController]
[Route("products")]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(OperationResult<PagedResponse<ProductSummaryResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? occasionId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetProductsQuery(page, pageSize, occasionId), cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }
}
