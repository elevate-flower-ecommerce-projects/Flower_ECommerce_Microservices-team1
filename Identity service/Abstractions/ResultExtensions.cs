namespace Identity_service.Abstractions;

public static class ResultExtensions
{
    public static IResult ToHandleResult(this Result result)
    {
        if (result.IsSuccess)
            return Results.Ok(CreateSuccess(null));

        return CreateFailure(result.Error);
    }

    public static IResult ToHandleResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return Results.Ok(CreateSuccess(result.Value));

        return CreateFailure(result.Error);
    }

    private static ApiResponse CreateSuccess(object? data)
      => new(
          IsSuccess: true,
          Message: "Operation completed successfully.",
          Data: data,
          Errors: [],
          StatusCode: StatusCodes.Status200OK,
          Timestamp: DateTime.UtcNow
      );

    private static IResult CreateFailure(Error error)
    {
        return Results.Json(
            new ApiResponse(
                IsSuccess: false,
                Message: "Operation is failure.",
                Data: null,
                Errors: [new Error(error.Code, error.Description, error.StatusCode)],
                StatusCode: error.StatusCode,
                Timestamp: DateTime.UtcNow
            ),
            statusCode: error.StatusCode
        );
    }
}