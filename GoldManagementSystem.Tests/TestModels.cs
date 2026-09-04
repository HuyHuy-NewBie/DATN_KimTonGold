namespace GoldManagementSystem.Models;

public sealed class ProductionWorkOrder
{
    public const string StatusClosed = "Closed";
    public const string StatusCancelled = "Cancelled";
    public const string StatusInProgress = "InProgress";
    public DateTime? PlannedEndAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal ActualOutputWeight { get; set; }
    public decimal IssuedMaterialWeight { get; set; }
}

public sealed class ProductionLossRecord
{
    public const string StatusRejected = "Rejected";
    public bool IsOverTolerance { get; set; }
    public string Status { get; set; } = string.Empty;
}
