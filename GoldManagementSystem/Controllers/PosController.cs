using System.Security.Cryptography;
using System.Data;
using GoldManagementSystem.Data;
using GoldManagementSystem.Models;
using GoldManagementSystem.Models.ViewModels;
using GoldManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GoldManagementSystem.Controllers
{
    [Authorize(Roles = RoleCatalog.ManagementRoles)]
    public sealed class PosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _users;
        private readonly IManagementPermissionService _permissions;
        private readonly IPricingService _pricing;

        public PosController(ApplicationDbContext context, UserManager<AppUser> users, IManagementPermissionService permissions, IPricingService pricing)
        { _context = context; _users = users; _permissions = permissions; _pricing = pricing; }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuote(CreatePosQuoteInput input)
        {
            if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.PriceManage, input.BranchId)) return Forbid();
            if (!ModelState.IsValid || input.Lines == null || input.Lines.Count == 0) return BadRequest("Báo giá phải có dòng hàng hợp lệ.");
            if (!await _context.Branches.AnyAsync(branch => branch.Id == input.BranchId && branch.IsActive)) return BadRequest("Chi nhánh không còn hoạt động.");

            var requestedLines = input.Lines
                .GroupBy(item => item.ProductId)
                .Select(group => new { ProductId = group.Key, Quantity = group.Sum(item => item.Quantity) })
                .ToList();
            if (requestedLines.Any(line => line.ProductId <= 0 || line.Quantity is <= 0 or > 999)) return BadRequest("Số lượng sản phẩm không hợp lệ.");

            var products = await _context.Products
                .Where(product => requestedLines.Select(line => line.ProductId).Contains(product.Id))
                .ToDictionaryAsync(product => product.Id);
            if (products.Count != requestedLines.Count) return BadRequest("Có sản phẩm không tồn tại.");

            foreach (var requestedLine in requestedLines)
            {
                var product = products[requestedLine.ProductId];
                if (product.BranchId != input.BranchId || product.Status is "Hết hàng" or "Đã bán")
                    return BadRequest($"Sản phẩm #{requestedLine.ProductId} không còn khả dụng tại chi nhánh này.");
            }

            var userId = _users.GetUserId(User);
            var quote = new PosQuote { QuoteNumber = $"Q-{DateTime.UtcNow:yyyyMMddHHmmssfff}", BranchId = input.BranchId, CustomerName = input.CustomerName.Trim(), CustomerPhone = input.CustomerPhone.Trim(), CreatedByUserId = userId, CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddHours(Math.Clamp(input.ValidHours, 1, 168)) };
            foreach (var requestLine in requestedLines)
            {
                var price = await _pricing.GetPublishedPriceAsync(requestLine.ProductId, input.BranchId);
                if (price == null) return BadRequest($"Sản phẩm #{requestLine.ProductId} chưa có giá công bố.");
                quote.Lines.Add(new PosQuoteLine { ProductId = requestLine.ProductId, Quantity = requestLine.Quantity, PriceBookId = price.Book.Id, PriceVersionId = price.Version.Id, UnitPrice = price.Line.SellUnitPrice, ProcessingFee = price.Line.ProcessingFee, MaxDiscountRate = price.Line.MaxDiscountRate });
            }
            quote.Subtotal = quote.Lines.Sum(line => (line.UnitPrice + line.ProcessingFee) * line.Quantity);
            quote.TotalAmount = quote.Subtotal;
            _context.PosQuotes.Add(quote);
            await _context.SaveChangesAsync();
            return Ok(new { quote.Id, quote.QuoteNumber, quote.TotalAmount, quote.ExpiresAt });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestDiscount(RequestDiscountInput input)
        {
            var quote = await _context.PosQuotes.Include(item => item.Lines).FirstOrDefaultAsync(item => item.Id == input.QuoteId);
            if (quote == null) return NotFound();
            if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.PriceManage, quote.BranchId)) return Forbid();
            if (quote.Status is PosDocumentStatusOptions.Converted or PosDocumentStatusOptions.Expired or PosDocumentStatusOptions.Rejected) return Conflict("Báo giá này không còn có thể sửa.");
            if (!ModelState.IsValid || input.RequestedRate <= 0) return BadRequest("Mức giảm không hợp lệ.");
            var amount = Math.Round(quote.Subtotal * input.RequestedRate / 100m, 2);
            if (input.RequestedRate > quote.Lines.Min(line => line.MaxDiscountRate)) return BadRequest("Mức giảm vượt giới hạn của bảng giá.");
            if (await _context.DiscountApprovals.AnyAsync(item => item.PosQuoteId == quote.Id && item.Status == DiscountApprovalStatusOptions.Pending)) return Conflict("Báo giá đã có yêu cầu giảm giá đang chờ duyệt.");
            _context.DiscountApprovals.Add(new DiscountApproval { PosQuoteId = quote.Id, RequestedAmount = amount, RequestedRate = input.RequestedRate, Reason = input.Reason.Trim(), RequestedByUserId = _users.GetUserId(User) });
            quote.DiscountAmount = 0; quote.TotalAmount = quote.Subtotal;
            await _context.SaveChangesAsync();
            return Ok(new { quote.Id, quote.TotalAmount, Status = DiscountApprovalStatusOptions.Pending });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveDiscount(int id)
        {
            var approval = await _context.DiscountApprovals.Include(item => item.PosQuote).FirstOrDefaultAsync(item => item.Id == id);
            if (approval?.PosQuote == null) return NotFound();
            if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.PriceApprove, approval.PosQuote.BranchId)) return Forbid();
            if (approval.Status != DiscountApprovalStatusOptions.Pending || approval.PosQuote.Status is PosDocumentStatusOptions.Converted or PosDocumentStatusOptions.Expired or PosDocumentStatusOptions.Rejected) return Conflict("Yêu cầu giảm giá không còn chờ duyệt.");
            var actor = _users.GetUserId(User);
            if (approval.RequestedByUserId == actor) return BadRequest("Người yêu cầu không được tự duyệt giảm giá.");
            approval.Status = DiscountApprovalStatusOptions.Approved; approval.ApprovedByUserId = actor; approval.ApprovedAt = DateTime.UtcNow;
            approval.PosQuote.DiscountAmount = approval.RequestedAmount;
            approval.PosQuote.TotalAmount = approval.PosQuote.Subtotal - approval.RequestedAmount;
            await _context.SaveChangesAsync(); return Ok(new { approval.Id, approval.Status });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ConvertQuoteToOrder(int id, string paymentMethod)
        {
            if (!IsSupportedPaymentMethod(paymentMethod)) return BadRequest("Hình thức thanh toán không hợp lệ.");

            try
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
                var quote = await _context.PosQuotes
                    .Include(item => item.Lines)
                    .Include(item => item.DiscountApprovals)
                    .FirstOrDefaultAsync(item => item.Id == id);
                if (quote == null) return NotFound();
                if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.ProductsEdit, quote.BranchId)) return Forbid();
                if (quote.Status != PosDocumentStatusOptions.Accepted) return Conflict("Chỉ báo giá đã được chấp nhận mới có thể chuyển thành đơn hàng.");
                if (quote.ExpiresAt <= DateTime.UtcNow)
                {
                    quote.Status = PosDocumentStatusOptions.Expired;
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return BadRequest("Báo giá đã hết hạn.");
                }
                if (quote.DiscountApprovals.Any(item => item.Status == DiscountApprovalStatusOptions.Pending)) return Conflict("Báo giá còn yêu cầu giảm giá chờ duyệt.");
                if (await _context.Orders.AsNoTracking().AnyAsync(item => item.PosQuoteId == quote.Id)) return Conflict("Báo giá này đã được chuyển thành đơn hàng.");

                var userId = _users.GetUserId(User);
                if (string.IsNullOrWhiteSpace(userId)) return Forbid();
                var productIds = quote.Lines.Select(line => line.ProductId).Distinct().ToList();
                var products = await _context.Products
                    .Where(product => productIds.Contains(product.Id))
                    .ToDictionaryAsync(product => product.Id);
                if (products.Count != productIds.Count || products.Values.Any(product => product.BranchId != quote.BranchId || product.Status is "Hết hàng" or "Đã bán"))
                    return Conflict("Một hoặc nhiều sản phẩm không còn khả dụng tại chi nhánh này.");

                // POS only reserves stock explicitly mapped to the catalog product and held
                // at an active display counter of the same branch. This intentionally rejects
                // legacy inventory that has not been reconciled to a ProductId yet.
                var inventoryItems = await _context.InventoryItems
                    .Include(item => item.Warehouse)
                    .Where(item => item.ProductId.HasValue
                        && productIds.Contains(item.ProductId.Value)
                        && item.Warehouse.BranchId == quote.BranchId
                        && item.Warehouse.IsActive
                        && item.Warehouse.LocationType == Warehouse.LocationTypeDisplay
                        && item.Status == InventoryItem.StatusAvailable
                        && item.QuantityOnHand > 0)
                    .OrderBy(item => item.CreatedAt)
                    .ThenBy(item => item.Id)
                    .ToListAsync();
                if (inventoryItems.Count == 0) return Conflict("Không có tồn quầy đã liên kết với sản phẩm để giữ hàng.");

                var inventoryIds = inventoryItems.Select(item => item.Id).ToList();
                var existingReservedQuantities = await _context.PosInventoryReservations
                    .Where(reservation => inventoryIds.Contains(reservation.InventoryItemId)
                        && reservation.Status == PosInventoryReservationStatusOptions.Reserved
                        && (reservation.Order.Status == Order.StatusAwaitingDepositPayment
                            || reservation.Order.Status == Order.StatusUnpaidDeposit
                            || reservation.Order.Status == Order.StatusPendingConfirmation))
                    .GroupBy(reservation => reservation.InventoryItemId)
                    .Select(group => new { InventoryItemId = group.Key, Quantity = group.Sum(item => item.Quantity) })
                    .ToDictionaryAsync(item => item.InventoryItemId, item => item.Quantity);

                var allocations = new List<PosInventoryAllocation>();
                var newlyReservedQuantities = new Dictionary<int, int>();
                foreach (var line in quote.Lines)
                {
                    var remaining = line.Quantity;
                    foreach (var inventoryItem in inventoryItems.Where(item => item.ProductId == line.ProductId))
                    {
                        var alreadyReserved = existingReservedQuantities.GetValueOrDefault(inventoryItem.Id)
                            + newlyReservedQuantities.GetValueOrDefault(inventoryItem.Id);
                        var availableQuantity = inventoryItem.QuantityOnHand - alreadyReserved;
                        if (availableQuantity <= 0) continue;

                        var quantity = Math.Min(remaining, availableQuantity);
                        var reservedWeight = CalculateReservedWeight(inventoryItem, quantity);
                        allocations.Add(new PosInventoryAllocation(inventoryItem, quantity, reservedWeight));
                        newlyReservedQuantities[inventoryItem.Id] = newlyReservedQuantities.GetValueOrDefault(inventoryItem.Id) + quantity;
                        remaining -= quantity;
                        if (remaining == 0) break;
                    }

                    if (remaining > 0) return Conflict($"Sản phẩm #{line.ProductId} không còn đủ tồn quầy để giữ hàng.");
                }

                var now = DateTime.UtcNow;
                var depositRate = paymentMethod == Order.PaymentMethodOnlineFull ? 100m : 10m;
                var order = new Order
                {
                    UserId = userId,
                    BranchId = quote.BranchId,
                    CustomerName = quote.CustomerName,
                    CustomerPhone = quote.CustomerPhone,
                    TotalAmount = quote.TotalAmount,
                    NetAmount = quote.TotalAmount,
                    DiscountAmount = quote.DiscountAmount,
                    DepositAmount = Math.Round(quote.TotalAmount * depositRate / 100m, 0),
                    DepositRate = depositRate,
                    PaymentMethod = paymentMethod,
                    Status = paymentMethod == Order.PaymentMethodCashDeposit ? Order.StatusUnpaidDeposit : Order.StatusAwaitingDepositPayment,
                    OrderDate = now,
                    DepositDueAt = now.AddMinutes(90),
                    PosQuoteId = quote.Id
                };
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var line in quote.Lines)
                {
                    var snapshot = new PriceSnapshot
                    {
                        OrderId = order.Id,
                        ProductId = line.ProductId,
                        PriceBookId = line.PriceBookId,
                        PriceVersionId = line.PriceVersionId,
                        SellUnitPrice = line.UnitPrice,
                        BuyUnitPrice = 0,
                        ProcessingFee = line.ProcessingFee,
                        MaxDiscountRate = line.MaxDiscountRate,
                        CapturedByUserId = userId,
                        CapturedAt = now
                    };
                    _context.OrderDetails.Add(new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = line.ProductId,
                        PriceSnapshot = snapshot,
                        UnitPrice = line.UnitPrice,
                        ProcessingFee = line.ProcessingFee,
                        DiscountAmount = line.DiscountAmount,
                        Quantity = line.Quantity
                    });
                }

                foreach (var allocation in allocations)
                {
                    _context.PosInventoryReservations.Add(new PosInventoryReservation
                    {
                        OrderId = order.Id,
                        InventoryItemId = allocation.InventoryItem.Id,
                        Quantity = allocation.Quantity,
                        ReservedWeight = allocation.ReservedWeight,
                        Status = PosInventoryReservationStatusOptions.Reserved,
                        CreatedByUserId = userId,
                        ReservedAt = now
                    });
                }

                quote.Status = PosDocumentStatusOptions.Converted;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { order.Id, order.OrderNumber, order.Status });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Báo giá hoặc tồn kho vừa thay đổi. Vui lòng tải lại và thử lại.");
            }
            catch (DbUpdateException)
            {
                // The filtered unique index on Orders.PosQuoteId is the final guard
                // against duplicate conversion across application instances.
                return Conflict("Không thể chuyển báo giá do dữ liệu vừa thay đổi. Vui lòng tải lại.");
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptQuote(int id)
        {
            var quote = await _context.PosQuotes.Include(item => item.DiscountApprovals).FirstOrDefaultAsync(item => item.Id == id);
            if (quote == null) return NotFound();
            if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.ProductsEdit, quote.BranchId)) return Forbid();
            if (quote.Status is PosDocumentStatusOptions.Converted or PosDocumentStatusOptions.Expired or PosDocumentStatusOptions.Rejected) return Conflict("Báo giá này không còn có thể chấp nhận.");
            if (quote.ExpiresAt <= DateTime.UtcNow) { quote.Status = PosDocumentStatusOptions.Expired; await _context.SaveChangesAsync(); return BadRequest("Báo giá đã hết hạn."); }
            if (quote.DiscountApprovals.Any(item => item.Status == DiscountApprovalStatusOptions.Pending)) return BadRequest("Còn yêu cầu giảm giá chờ duyệt.");
            quote.Status = PosDocumentStatusOptions.Accepted; await _context.SaveChangesAsync(); return Ok(new { quote.Id, quote.Status });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDelivery(CreateDeliveryInput input)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(item => item.Id == input.OrderId);
            if (order == null) return NotFound();
            if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.ProductsEdit, order.BranchId)) return Forbid();
            if (!ModelState.IsValid || await _context.OrderDeliveries.AnyAsync(item => item.OrderId == order.Id)) return BadRequest("Thông tin giao hàng không hợp lệ hoặc đã tồn tại.");
            _context.OrderDeliveries.Add(new OrderDelivery { OrderId = order.Id, RecipientName = input.RecipientName.Trim(), RecipientPhone = input.RecipientPhone.Trim(), Address = input.Address.Trim(), Carrier = input.Carrier?.Trim(), TrackingNumber = input.TrackingNumber?.Trim() });
            await _context.SaveChangesAsync(); return Ok();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDeliveryEvidence(int deliveryId, IFormFile file)
        {
            var delivery = await _context.OrderDeliveries.Include(item => item.Order).FirstOrDefaultAsync(item => item.Id == deliveryId);
            if (delivery == null) return NotFound();
            if (!await _permissions.CanAsync(User, ManagementFeatureCatalog.ProductsEdit, delivery.Order.BranchId)) return Forbid();
            if (file == null || file.Length == 0 || file.Length > 10 * 1024 * 1024) return BadRequest("File bằng chứng không hợp lệ.");
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension is not ".jpg" and not ".jpeg" and not ".png" and not ".webp") return BadRequest("Chỉ chấp nhận ảnh.");
            var name = $"delivery-{Guid.NewGuid():N}{extension}"; var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "delivery"); Directory.CreateDirectory(folder); var path = Path.Combine(folder, name);
            await using (var stream = System.IO.File.Create(path)) await file.CopyToAsync(stream);
            await using var hashStream = System.IO.File.OpenRead(path); var hash = Convert.ToHexString(SHA256.HashData(hashStream));
            _context.DeliveryEvidences.Add(new DeliveryEvidence { OrderDeliveryId = delivery.Id, FileUrl = $"/uploads/delivery/{name}", FileHash = hash, UploadedByUserId = _users.GetUserId(User) });
            delivery.Status = DeliveryStatusOptions.Delivered; delivery.DeliveredAt = DateTime.UtcNow; await _context.SaveChangesAsync(); return Ok();
        }

        private static bool IsSupportedPaymentMethod(string paymentMethod)
            => paymentMethod == Order.PaymentMethodCashDeposit
                || paymentMethod == Order.PaymentMethodOnlineDeposit
                || paymentMethod == Order.PaymentMethodOnlineFull;

        private static decimal CalculateReservedWeight(InventoryItem inventoryItem, int quantity)
        {
            if (quantity <= 0 || inventoryItem.QuantityOnHand <= 0 || inventoryItem.WeightOnHand <= 0) return 0m;
            if (quantity >= inventoryItem.QuantityOnHand) return inventoryItem.WeightOnHand;
            return Math.Min(
                inventoryItem.WeightOnHand,
                Math.Round(inventoryItem.WeightOnHand * quantity / inventoryItem.QuantityOnHand, 2, MidpointRounding.AwayFromZero));
        }

        private sealed record PosInventoryAllocation(InventoryItem InventoryItem, int Quantity, decimal ReservedWeight);
    }
}
