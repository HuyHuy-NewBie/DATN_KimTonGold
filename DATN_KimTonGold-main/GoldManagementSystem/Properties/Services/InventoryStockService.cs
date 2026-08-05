using GoldManagementSystem.Data;
using GoldManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GoldManagementSystem.Services
{
    public class InventoryStockService
    {
        private readonly ApplicationDbContext _context;

        public InventoryStockService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Chuẩn bị dữ liệu nhập kho từ một chi tiết
        /// phiếu nhận hàng nhà cung cấp đã được duyệt.
        ///
        /// Hàm này chưa gọi SaveChangesAsync.
        /// Controller duyệt nhập kho phải tự mở transaction
        /// và gọi SaveChangesAsync sau khi hoàn tất toàn bộ bước.
        /// </summary>
        public async Task<InventoryItem>
            PrepareSupplierReceiptEntryAsync(
                int receiptDetailId,
                int warehouseId,
                int acceptedQuantity,
                decimal acceptedWeight,
                decimal? acceptedCarat,
                decimal approvedUnitCost,
                string createdByUserId,
                string note = null)
        {
            if (receiptDetailId <= 0)
            {
                throw new InvalidOperationException(
                    "Chi tiết phiếu nhận hàng không hợp lệ.");
            }

            if (warehouseId <= 0)
            {
                throw new InvalidOperationException(
                    "Kho nhận hàng không hợp lệ.");
            }

            if (acceptedQuantity <= 0)
            {
                throw new InvalidOperationException(
                    "Số lượng đạt phải lớn hơn 0.");
            }

            if (acceptedWeight < 0)
            {
                throw new InvalidOperationException(
                    "Trọng lượng đạt không được nhỏ hơn 0.");
            }

            if (string.IsNullOrWhiteSpace(createdByUserId))
            {
                throw new InvalidOperationException(
                    "Không xác định được người thực hiện nhập kho.");
            }

            var receiptDetail =
                await _context.SupplierGoodsReceiptDetails
                    .Include(detail =>
                        detail.SupplierPurchaseOrderDetail)
                    .Include(detail =>
                        detail.SupplierGoodsReceipt)
                        .ThenInclude(receipt =>
                            receipt.SupplierPurchaseOrder)
                    .FirstOrDefaultAsync(detail =>
                        detail.Id == receiptDetailId);

            if (receiptDetail == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy chi tiết phiếu nhận hàng.");
            }

            var purchaseOrderDetail =
                receiptDetail.SupplierPurchaseOrderDetail;

            var receipt =
                receiptDetail.SupplierGoodsReceipt;

            var purchaseOrder =
                receipt?.SupplierPurchaseOrder;

            if (purchaseOrderDetail == null
                || receipt == null
                || purchaseOrder == null)
            {
                throw new InvalidOperationException(
                    "Phiếu nhận hàng chưa liên kết đầy đủ với đơn đặt hàng.");
            }

            if (receiptDetail.ReceivedQuantity > 0
                && acceptedQuantity
                    > receiptDetail.ReceivedQuantity)
            {
                throw new InvalidOperationException(
                    "Số lượng đạt không được lớn hơn số lượng đã nhận.");
            }

            var warehouse = await _context.Warehouses
                .Include(item => item.Branch)
                .FirstOrDefaultAsync(item =>
                    item.Id == warehouseId);

            if (warehouse == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy kho nhận hàng.");
            }

            if (!warehouse.IsActive)
            {
                throw new InvalidOperationException(
                    $"Kho {warehouse.Code} đang tạm ngưng.");
            }

            if (warehouse.LocationType
                != Warehouse.LocationTypeStorage)
            {
                throw new InvalidOperationException(
                    "Hàng từ nhà cung cấp chỉ được nhập vào kho lưu trữ, không được nhập thẳng vào quầy trưng bày.");
            }
            /*
             * Đơn đặt hàng của chi nhánh nào thì chỉ được
             * nhập vào kho thuộc đúng chi nhánh đó.
             */
            if (warehouse.BranchId
                != purchaseOrder.BranchId)
            {
                throw new InvalidOperationException(
                    "Kho được chọn không thuộc chi nhánh nhận hàng của đơn đặt hàng.");
            }

            var inventoryEntryExists =
                await _context.InventoryItems.AnyAsync(item =>
                    item.SupplierGoodsReceiptDetailId
                    == receiptDetailId);

            var pendingInventoryEntryExists =
                _context.ChangeTracker
                    .Entries<InventoryItem>()
                    .Any(entry =>
                        entry.State != EntityState.Deleted
                        && entry.Entity
                            .SupplierGoodsReceiptDetailId
                            == receiptDetailId);

            if (inventoryEntryExists
                || pendingInventoryEntryExists)
            {
                throw new InvalidOperationException(
                    "Chi tiết phiếu nhận hàng này đã được nhập kho trước đó.");
            }

            var resolvedUnitCost =
                approvedUnitCost > 0
                    ? approvedUnitCost
                    : receiptDetail.ActualUnitCost > 0
                        ? receiptDetail.ActualUnitCost
                        : purchaseOrderDetail.UnitCost;

            if (resolvedUnitCost <= 0)
            {
                throw new InvalidOperationException(
                    "Đơn giá nhập kho phải lớn hơn 0.");
            }

            var now = DateTime.UtcNow;

            var inventoryItem = new InventoryItem
            {
                StockCode = BuildStockCode(),

                WarehouseId = warehouse.Id,

                SupplierId =
                    purchaseOrder.SupplierId,

                SupplierPurchaseOrderId =
                    purchaseOrder.Id,

                SupplierGoodsReceiptDetailId =
                    receiptDetail.Id,

                ProductLine =
                    purchaseOrderDetail.ProductLine,

                Category =
                    purchaseOrderDetail.Category,

                ProductName =
                    purchaseOrderDetail.ProductName,

                MaterialType =
                    purchaseOrderDetail.GoldType,

                QuantityOnHand =
                    acceptedQuantity,

                WeightOnHand =
                    acceptedWeight,

                DiamondCarat =
                    acceptedCarat,

                CertificateCode =
                    !string.IsNullOrWhiteSpace(
                        receiptDetail.ActualDiamondCertificate)
                        ? receiptDetail.ActualDiamondCertificate
                        : purchaseOrderDetail.DiamondCertificate,

                UnitCost =
                    resolvedUnitCost,

                Status =
                    InventoryItem.StatusAvailable,

                Note =
                    NormalizeNote(note),

                CreatedAt = now,
                UpdatedAt = now
            };

            var inventoryTransaction =
                new InventoryTransaction
                {
                    TransactionCode =
                        BuildTransactionCode(),

                    WarehouseId =
                        warehouse.Id,

                    /*
                     * Gán navigation property để EF tự tạo
                     * InventoryItem trước rồi gắn khóa ngoại.
                     */
                    InventoryItem =
                        inventoryItem,

                    TransactionType =
                        InventoryTransaction
                            .TypeSupplierReceipt,

                    QuantityChange =
                        acceptedQuantity,

                    WeightChange =
                        acceptedWeight,

                    QuantityAfter =
                        acceptedQuantity,

                    WeightAfter =
                        acceptedWeight,

                    ReferenceType =
                        "Phiếu nhận hàng NCC",

                    ReferenceId =
                        receipt.Id,

                    Note =
                        NormalizeNote(note)
                        ?? $"Nhập kho từ phiếu {receipt.ReceiptCode}.",

                    CreatedByUserId =
                        createdByUserId,

                    CreatedAt = now
                };

            _context.InventoryItems.Add(
                inventoryItem);

            _context.InventoryTransactions.Add(
                inventoryTransaction);

            return inventoryItem;
        }

        private static string BuildStockCode()
        {
            var suffix = Guid.NewGuid()
                .ToString("N")
                .Substring(0, 6)
                .ToUpperInvariant();

            return $"INV-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{suffix}";
        }

        private static string BuildTransactionCode()
        {
            var suffix = Guid.NewGuid()
                .ToString("N")
                .Substring(0, 6)
                .ToUpperInvariant();

            return $"ITX-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{suffix}";
        }

        private static string NormalizeNote(
            string value)
        {
            value = value?.Trim();

            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Length <= 500
                ? value
                : value.Substring(0, 500);
        }
    }
}