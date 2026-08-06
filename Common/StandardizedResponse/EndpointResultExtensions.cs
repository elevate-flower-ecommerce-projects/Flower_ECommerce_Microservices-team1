namespace Flower.Common.StandardizedResponse;

public static class EndpointResultExtensions
{
    public static IResult ToHttpResult(this OperationResult result)
        => Results.Json(result, statusCode: (int)result.StatusCode);
}
