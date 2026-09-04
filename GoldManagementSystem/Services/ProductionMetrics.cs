using GoldManagementSystem.Models;

namespace GoldManagementSystem.Services
{
    public static class ProductionMetrics
    {
        public static bool IsLate(ProductionWorkOrder order, DateTime nowUtc)
        {
            return order.PlannedEndAt.HasValue
                && order.PlannedEndAt.Value < nowUtc
                && order.Status != ProductionWorkOrder.StatusClosed
                && order.Status != ProductionWorkOrder.StatusCancelled;
        }

        public static bool IsOverTolerance(ProductionLossRecord loss) => loss.IsOverTolerance && loss.Status != ProductionLossRecord.StatusRejected;

        public static decimal WipWeight(ProductionWorkOrder order)
        {
            return order.Status is ProductionWorkOrder.StatusClosed or ProductionWorkOrder.StatusCancelled
                ? 0
                : order.ActualOutputWeight > 0 ? order.ActualOutputWeight : order.IssuedMaterialWeight;
        }
    }
}
