using GoldManagementSystem.Models;
using GoldManagementSystem.Services;

var now = DateTime.UtcNow;
var lateOrder = new ProductionWorkOrder { PlannedEndAt = now.AddMinutes(-1), Status = ProductionWorkOrder.StatusInProgress };
var closedOrder = new ProductionWorkOrder { PlannedEndAt = now.AddDays(-1), Status = ProductionWorkOrder.StatusClosed };
var wipOrder = new ProductionWorkOrder { Status = ProductionWorkOrder.StatusInProgress, IssuedMaterialWeight = 12.5m, ActualOutputWeight = 8.25m };
var rejectedLoss = new ProductionLossRecord { IsOverTolerance = true, Status = ProductionLossRecord.StatusRejected };

Require(ProductionMetrics.IsLate(lateOrder, now), "late order alert");
Require(!ProductionMetrics.IsLate(closedOrder, now), "closed order exclusion");
Require(ProductionMetrics.WipWeight(wipOrder) == 8.25m, "wip calculation");
Require(!ProductionMetrics.IsOverTolerance(rejectedLoss), "rejected loss exclusion");
Console.WriteLine("ProductionMetrics self-tests passed.");

static void Require(bool condition, string name)
{
    if (!condition) throw new InvalidOperationException($"Self-test failed: {name}");
}
