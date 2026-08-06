namespace Flower.Common.StandardizedResponse;

public static partial class APIConstants
{
    public static class APIMessages
    {
        public const string Success = "The operation was successful.";
        public const string SuccessLocalized = "The operation was successful.";
        public const string Created = "The resource was created successfully.";
        public const string CreatedLocalized = "The resource was created successfully.";
        public const string NoContent = "No data found.";
        public const string NoContentLocalized = "No data found.";
        public const string Error = "The operation failed.";
        public const string ErrorLocalized = "The operation failed.";
        public const string BadRequest = "Invalid request.";
        public const string BadRequestLocalized = "Invalid request.";
        public const string ValidationError = "One or more validation errors occurred.";
        public const string ValidationErrorLocalized = "One or more validation errors occurred.";
        public const string NotFound = "No data found.";
        public const string NotFoundLocalized = "No data found.";
        public const string Forbidden = "You do not have permission to perform this operation.";
        public const string ForbiddenLocalized = "You do not have permission to perform this operation.";
        public const string Conflict = "The request conflicts with existing data.";
        public const string ConflictLocalized = "The request conflicts with existing data.";
        public const string DataCorruption = "The submitted data is inconsistent.";
        public const string DataCorruptionLocalized = "The submitted data is inconsistent.";
    }

    public static class UserMessages
    {
        public const string NotFound = "User not found.";
        public const string NotFoundLocalized = "User not found.";
        public const string InvalidPassword = "Please enter the password again.";
        public const string InvalidPasswordLocalized = "Please enter the password again.";
        public const string NotVerified = "Your account is not verified yet.";
        public const string NotVerifiedLocalized = "Your account is not verified yet.";
        public const string Suspended = "Too many requests. Your account is temporarily suspended.";
        public const string SuspendedLocalized = "Too many requests. Your account is temporarily suspended.";
    }
}
