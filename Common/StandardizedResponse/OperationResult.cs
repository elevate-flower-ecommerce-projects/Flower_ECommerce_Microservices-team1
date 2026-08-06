namespace Flower.Common.StandardizedResponse;

public class OperationResult(StatusCode statusCode, string message, string messageLocalized)
{
    public StatusCode StatusCode { get; set; } = statusCode;
    public bool Success => (int)StatusCode >= 200 && (int)StatusCode < 300;
    public string Message { get; set; } = message;
    public string MessageLocalized { get; set; } = messageLocalized;
}

public class OperationResult<T>(
    StatusCode statusCode,
    string message,
    string messageLocalized,
    T data = default!) : OperationResult(statusCode, message, messageLocalized)
{
    public T Data { get; set; } = data;
}
