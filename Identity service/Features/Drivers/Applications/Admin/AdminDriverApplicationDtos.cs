using Identity_service.Entities;

namespace Identity_service.Features.Drivers.Applications.Admin;

public sealed record PagedResponse<T>(
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<T> Items);

public sealed record AdminDriverApplicationSummaryResponse(
    Guid ApplicationId,
    DriverApplicationStatus Status,
    string FullName,
    string Email,
    string? Phone,
    string NationalId,
    VehicleType? VehicleType,
    string? PlateNumber,
    int DocumentCount,
    DateTime SubmittedAt,
    string? ReviewedBy,
    DateTime? ReviewedAt);

public sealed record AdminDriverApplicationDetailResponse(
    Guid ApplicationId,
    DriverApplicationStatus Status,
    string FullName,
    string Email,
    string? Phone,
    string NationalId,
    VehicleType? VehicleType,
    string? PlateNumber,
    string? RejectionReason,
    string? ReviewedBy,
    DateTime? ReviewedAt,
    DateTime SubmittedAt,
    IReadOnlyList<AdminDriverDocumentResponse> Documents);

public sealed record AdminDriverDocumentResponse(
    Guid DocumentId,
    string DocType,
    string OriginalFileName,
    string ContentType,
    long SizeInBytes,
    DateTime UploadedAt,
    string DownloadUrl);

public sealed record RejectDriverApplicationRequest(string RejectionReason);
