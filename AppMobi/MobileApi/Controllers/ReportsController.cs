using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileApi.Contracts;
using MobileApi.Data;
using MobileApi.Models;
using MobileApi.Services;
using System.Globalization;

namespace MobileApi.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Policy = Policies.ReportsRead)]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly PermissionService _permissionService;

    public ReportsController(ApplicationDbContext context, PermissionService permissionService)
    {
        _context = context;
        _permissionService = permissionService;
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<RevenueSummaryDto>> GetRevenue(DateTime? from = null, DateTime? to = null, string? month = null, int? branchId = null, string bucket = "day")
    {
        var actor = await _permissionService.GetActorAsync(User);
        if (actor == null)
        {
            return Unauthorized(new ApiError("Tài khoản không còn hợp lệ."));
        }

        if (!TryResolveRange(from, to, month, out var start, out var endExclusive, out var rangeError))
        {
            return BadRequest(rangeError);
        }

        var normalizedBucket = NormalizeBucket(bucket);
        if (normalizedBucket == null)
        {
            return BadRequest(new ApiError("Kiểu tổng hợp doanh thu không hợp lệ. Chỉ hỗ trợ day hoặc month."));
        }

        var query = _context.Orders.AsNoTracking().AsQueryable();
        query = _permissionService.ApplyOrderScope(query, actor);
        query = query.Where(order => order.OrderDate >= start && order.OrderDate < endExclusive);

        if (branchId.HasValue)
        {
            query = query.Where(order => order.BranchId == branchId.Value);
        }

        var orders = await query.ToListAsync();
        return BuildRevenueSummary(orders, start, endExclusive, normalizedBucket);
    }

    [HttpGet("revenue/day")]
    public Task<ActionResult<RevenueSummaryDto>> GetRevenueByDay(DateTime? date = null, int? branchId = null)
    {
        var day = (date ?? DateTime.Today).Date;
        return GetRevenue(day, day, null, branchId, "day");
    }

    [HttpGet("revenue/month")]
    public Task<ActionResult<RevenueSummaryDto>> GetRevenueByMonth(string? month = null, int? branchId = null, string bucket = "day")
    {
        var requestedMonth = string.IsNullOrWhiteSpace(month)
            ? DateTime.Today.ToString("yyyy-MM", CultureInfo.InvariantCulture)
            : month.Trim();

        return GetRevenue(null, null, requestedMonth, branchId, bucket);
    }

    [HttpGet("revenue/range")]
    public Task<ActionResult<RevenueSummaryDto>> GetRevenueByRange(DateTime from, DateTime to, int? branchId = null, string bucket = "day")
    {
        return GetRevenue(from, to, null, branchId, bucket);
    }

    private static RevenueSummaryDto BuildRevenueSummary(IReadOnlyList<Order> orders, DateTime start, DateTime endExclusive, string bucket)
    {
        var completed = orders.Where(order => order.Status == OrderStatusOptions.Completed).ToList();
        var daily = completed
            .GroupBy(order => order.OrderDate.Date)
            .OrderBy(group => group.Key)
            .Select(group => new RevenueBucketDto(group.Key, group.Sum(order => order.TotalAmount), group.Count()))
            .ToList();
        var monthly = completed
            .GroupBy(order => new DateTime(order.OrderDate.Year, order.OrderDate.Month, 1))
            .OrderBy(group => group.Key)
            .Select(group => new RevenueMonthBucketDto(
                group.Key.ToString("yyyy-MM", CultureInfo.InvariantCulture),
                group.Key,
                group.Key.AddMonths(1).AddTicks(-1),
                group.Sum(order => order.TotalAmount),
                group.Count()))
            .ToList();
        var byStatus = orders
            .GroupBy(order => order.Status)
            .OrderByDescending(group => group.Sum(order => order.TotalAmount))
            .Select(group => new StatusRevenueDto(group.Key, group.Sum(order => order.TotalAmount), group.Count()))
            .ToList();

        return new(
            start,
            endExclusive.AddTicks(-1),
            bucket,
            completed.Sum(order => order.TotalAmount),
            orders.Sum(order => order.TotalAmount),
            orders.Count,
            completed.Count,
            orders.Count(order => order.Status == OrderStatusOptions.PendingApproval),
            orders.Count(order => order.Status == OrderStatusOptions.Cancelled),
            daily,
            monthly,
            byStatus);
    }

    private static bool TryResolveRange(DateTime? from, DateTime? to, string? month, out DateTime start, out DateTime endExclusive, out ApiError? error)
    {
        if (!string.IsNullOrWhiteSpace(month)
            && !DateTime.TryParseExact(month.Trim(), "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            start = default;
            endExclusive = default;
            error = new ApiError("Định dạng tháng không hợp lệ. Vui lòng dùng yyyy-MM, ví dụ 2026-06.");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(month))
        {
            var monthStart = DateTime.ParseExact(month.Trim(), "yyyy-MM", CultureInfo.InvariantCulture);
            start = monthStart.Date;
            endExclusive = monthStart.Date.AddMonths(1);
            error = null;
            return true;
        }

        start = (from ?? to ?? DateTime.Today).Date;
        var end = (to ?? from ?? DateTime.Today).Date;
        if (end < start)
        {
            endExclusive = default;
            error = new ApiError("Khoảng ngày không hợp lệ: ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
            return false;
        }

        endExclusive = end.AddDays(1);
        error = null;
        return true;
    }

    private static string? NormalizeBucket(string? bucket)
    {
        if (string.IsNullOrWhiteSpace(bucket))
        {
            return "day";
        }

        return bucket.Trim().ToLowerInvariant() switch
        {
            "day" or "daily" => "day",
            "month" or "monthly" => "month",
            _ => null
        };
    }
}
