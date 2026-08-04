namespace Flower.Common.StandardizedResponse;

public static class OperationResultFactory
{
    public static OperationResult Success(
        string message = APIConstants.APIMessages.Success,
        string messageLocalized = APIConstants.APIMessages.SuccessLocalized,
        StatusCode statusCode = StatusCode.Success)
        => new(statusCode, message, messageLocalized);

    public static OperationResult<T> Success<T>(
        T data = default!,
        string message = APIConstants.APIMessages.Success,
        string messageLocalized = APIConstants.APIMessages.SuccessLocalized,
        StatusCode statusCode = StatusCode.Success)
        => new(statusCode, message, messageLocalized, data);

    public static OperationResult<T> Created<T>(
        T data,
        string message = APIConstants.APIMessages.Created,
        string messageLocalized = APIConstants.APIMessages.CreatedLocalized)
        => new(StatusCode.Created, message, messageLocalized, data);

    public static OperationResult NoContent(
        string message = APIConstants.APIMessages.NoContent,
        string messageLocalized = APIConstants.APIMessages.NoContentLocalized)
        => new(StatusCode.NoContent, message, messageLocalized);

    public static OperationResult<T> NoContent<T>(
        T data = default!,
        string message = APIConstants.APIMessages.NoContent,
        string messageLocalized = APIConstants.APIMessages.NoContentLocalized)
        => new(StatusCode.NoContent, message, messageLocalized, data);

    public static OperationResult Error(
        string message = APIConstants.APIMessages.Error,
        string messageLocalized = APIConstants.APIMessages.ErrorLocalized,
        StatusCode statusCode = StatusCode.InternalServerError)
        => new(statusCode, message, messageLocalized);

    public static OperationResult<T> Error<T>(
        T data = default!,
        string message = APIConstants.APIMessages.Error,
        string messageLocalized = APIConstants.APIMessages.ErrorLocalized,
        StatusCode statusCode = StatusCode.InternalServerError)
        => new(statusCode, message, messageLocalized, data);

    public static OperationResult BadRequest(
        string message = APIConstants.APIMessages.BadRequest,
        string messageLocalized = APIConstants.APIMessages.BadRequestLocalized)
        => new(StatusCode.BadRequest, message, messageLocalized);

    public static OperationResult<T> BadRequest<T>(
        T data = default!,
        string message = APIConstants.APIMessages.BadRequest,
        string messageLocalized = APIConstants.APIMessages.BadRequestLocalized)
        => new(StatusCode.BadRequest, message, messageLocalized, data);

    public static OperationResult<T> Validation<T>(
        T errors,
        string message = APIConstants.APIMessages.ValidationError,
        string messageLocalized = APIConstants.APIMessages.ValidationErrorLocalized)
        => new(StatusCode.ValidationError, message, messageLocalized, errors);

    public static OperationResult NotFound(
        string message = APIConstants.APIMessages.NotFound,
        string messageLocalized = APIConstants.APIMessages.NotFoundLocalized)
        => new(StatusCode.NotFound, message, messageLocalized);

    public static OperationResult<T> NotFound<T>(
        T data = default!,
        string message = APIConstants.APIMessages.NotFound,
        string messageLocalized = APIConstants.APIMessages.NotFoundLocalized)
        => new(StatusCode.NotFound, message, messageLocalized, data);

    public static OperationResult UnAuthorized(
        string message = APIConstants.UserMessages.InvalidPassword,
        string messageLocalized = APIConstants.UserMessages.InvalidPasswordLocalized)
        => new(StatusCode.Unauthorized, message, messageLocalized);

    public static OperationResult<T> UnAuthorized<T>(
        T data = default!,
        string message = APIConstants.UserMessages.InvalidPassword,
        string messageLocalized = APIConstants.UserMessages.InvalidPasswordLocalized)
        => new(StatusCode.Unauthorized, message, messageLocalized, data);

    public static OperationResult Forbidden(
        string message = APIConstants.APIMessages.Forbidden,
        string messageLocalized = APIConstants.APIMessages.ForbiddenLocalized)
        => new(StatusCode.Forbidden, message, messageLocalized);

    public static OperationResult<T> Forbidden<T>(
        T data = default!,
        string message = APIConstants.APIMessages.Forbidden,
        string messageLocalized = APIConstants.APIMessages.ForbiddenLocalized)
        => new(StatusCode.Forbidden, message, messageLocalized, data);

    public static OperationResult Conflict(
        string message = APIConstants.APIMessages.Conflict,
        string messageLocalized = APIConstants.APIMessages.ConflictLocalized)
        => new(StatusCode.Conflict, message, messageLocalized);

    public static OperationResult<T> Conflict<T>(
        T data = default!,
        string message = APIConstants.APIMessages.Conflict,
        string messageLocalized = APIConstants.APIMessages.ConflictLocalized)
        => new(StatusCode.Conflict, message, messageLocalized, data);
}
