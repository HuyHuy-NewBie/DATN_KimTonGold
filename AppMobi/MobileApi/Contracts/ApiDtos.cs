namespace MobileApi.Contracts;

public record ApiError(string Message);

public record UserProfileDto(
    string Id,
    string FullName,
    string? Email,
    string? PhoneNumber,
    int? BranchId,
    string? BranchName,
    bool IsActive,
    IReadOnlyList<string> Roles,
    string HighestRole,
    IReadOnlyList<string> Permissions);

public record LoginRequest(string Identifier, string Password, string DeviceId, bool RememberDevice);
public record RefreshRequest(string RefreshToken, string DeviceId);
public record LogoutRequest(string? RefreshToken, string? DeviceId);
public record AuthResponse(string AccessToken, DateTime AccessTokenExpiresAt, string RefreshToken, DateTime RefreshTokenExpiresAt, UserProfileDto User);

public record RegisterDeviceRequest(string DeviceId, string ExpoPushToken, string? Platform);

public record BranchDto(int Id, string BranchName, string? Address, string? PhoneNumber, bool IsActive);
public record CreateBranchRequest(string BranchName, string? Address, string? PhoneNumber);
public record UpdateBranchStatusRequest(bool IsActive);

public record ProductDto(
    int Id,
    string Name,
    string Category,
    string GoldType,
    string ProductLine,
    string CatalogMode,
    IReadOnlyList<string> AssignedProductLines,
    decimal Weight,
    decimal ProcessingFee,
    decimal SellPrice,
    decimal BuyPrice,
    int BranchId,
    string? BranchName,
    IReadOnlyList<string> Images,
    string? Description,
    string Status,
    DateTime CreatedAt,
    string? DiamondShape,
    string? DiamondCut,
    string? DiamondColor,
    string? DiamondClarity,
    decimal? DiamondCarat,
    decimal? DiamondSize,
    string? DiamondCertificate);

public record ProductUpsertRequest(
    string Name,
    string Category,
    string GoldType,
    string ProductLine,
    string CatalogMode,
    IReadOnlyList<string>? AssignedProductLines,
    decimal Weight,
    decimal ProcessingFee,
    decimal SellPrice,
    decimal BuyPrice,
    int BranchId,
    IReadOnlyList<string>? Images,
    string? Description,
    string? Status,
    string? DiamondShape,
    string? DiamondCut,
    string? DiamondColor,
    string? DiamondClarity,
    decimal? DiamondCarat,
    decimal? DiamondSize,
    string? DiamondCertificate);

public record OrderDetailDto(int Id, int ProductId, string? ProductName, decimal UnitPrice, int Quantity);

public record OrderDto(
    int Id,
    string OrderNumber,
    string? CustomerName,
    string? CustomerPhone,
    decimal TotalAmount,
    string Status,
    DateTime OrderDate,
    int BranchId,
    string? BranchName,
    string? StaffName,
    IReadOnlyList<OrderDetailDto> Details);

public record UpdateOrderStatusRequest(string Status);

public record RevenueSummaryDto(
    DateTime From,
    DateTime To,
    string Bucket,
    decimal Revenue,
    decimal GrossAmount,
    int OrderCount,
    int CompletedCount,
    int PendingApprovalCount,
    int CancelledCount,
    IReadOnlyList<RevenueBucketDto> Daily,
    IReadOnlyList<RevenueMonthBucketDto> Monthly,
    IReadOnlyList<StatusRevenueDto> ByStatus);

public record RevenueBucketDto(DateTime Date, decimal Revenue, int OrderCount);
public record RevenueMonthBucketDto(string Month, DateTime From, DateTime To, decimal Revenue, int OrderCount);
public record StatusRevenueDto(string Status, decimal Amount, int OrderCount);

public record CreateUserRequest(string FullName, string Email, string Password, string Role, int? BranchId);
public record UpdateUserRoleRequest(string Role);
public record UpdateUserStatusRequest(bool IsActive);

public record CatalogMetadataDto(
    IReadOnlyList<string> ProductLines,
    IReadOnlyList<string> CatalogModes,
    IReadOnlyList<string> ProductStatuses,
    IReadOnlyList<string> OrderStatuses,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> GoldCategories,
    IReadOnlyList<string> SilverCategories,
    IReadOnlyList<string> DiamondCategories,
    IReadOnlyList<string> GoldMaterials,
    IReadOnlyList<string> SilverMaterials,
    IReadOnlyList<string> DiamondMaterials,
    IReadOnlyList<string> DiamondShapes,
    IReadOnlyList<string> DiamondCuts,
    IReadOnlyList<string> DiamondColors,
    IReadOnlyList<string> DiamondClarities,
    IReadOnlyList<string> DiamondCertificates);
