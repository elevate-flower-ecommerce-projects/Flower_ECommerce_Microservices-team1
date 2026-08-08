using Flower.Common.StandardizedResponse;
using Identity_service.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Identity_service.Features.Users.Login;

[ApiController]
[Route("auth")]
public sealed class LoginController(ISender sender, ILogger<LoginController> logger) : ControllerBase
{
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    [ProducesResponseType(typeof(OperationResult<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(OperationResult<LoginResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(OperationResult<LoginResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(OperationResult<LoginResponseDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(OperationResult<LoginResponseDto>), StatusCodes.Status423Locked)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await sender.Send(new LoginCommand(request.Email, request.Password), cancellationToken);
            return ToActionResult(result);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error while processing a login request");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new OperationResult(
                    Flower.Common.StandardizedResponse.StatusCode.InternalServerError,
                    "An unexpected error occurred while processing the login request.",
                    "An unexpected error occurred while processing the login request."));
        }
    }

    private IActionResult ToActionResult(Result<LoginResponseDto> result)
    {
        if (result.IsSuccess)
        {
            return Ok(OperationResultFactory.Success(result.Value, "Login successful.", "Login successful."));
        }

        var statusCode = result.Error.StatusCode ?? StatusCodes.Status500InternalServerError;
        return StatusCode(
            statusCode,
            OperationResultFactory.Error<LoginResponseDto>(
                message: result.Error.Description,
                messageLocalized: result.Error.Description,
                statusCode: (Flower.Common.StandardizedResponse.StatusCode)statusCode));
    }
}
