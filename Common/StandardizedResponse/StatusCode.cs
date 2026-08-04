namespace Flower.Common.StandardizedResponse;

public enum StatusCode
{
    Success = 200,
    Created = 201,
    NoContent = 204,
    Redirect = 303,
    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    Conflict = 409,
    Gone = 410,
    ValidationError = 422,
    Locked = 423,
    TooManyRequests = 429,
    InternalServerError = 500,
    NotImplemented = 501,
    ServiceUnavailable = 503,
    GeneralError = 550,
    DataCorruption = 590
}
