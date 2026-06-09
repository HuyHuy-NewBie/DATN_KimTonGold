using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileApi.Contracts;
using MobileApi.Data;
using MobileApi.Models;
using MobileApi.Services;

namespace MobileApi.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize(Policy = Policies.OrderRead)]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly PermissionService _permissionService;

    public OrdersController(ApplicationDbContext context, PermissionService permissionService)
    {
        _context = context;
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetOrders(
        DateTime? from = null,
        DateTime? to = null,
        string? status = null,
        int? branchId = null,
        int take = 200)
    {
        var actor = await _permissionService.GetActorAsync(User);
        if (actor == null)
        {
            return Unauthorized(new ApiError("Tài khoản không còn hợp lệ."));
        }

        if (!TryResolveRange(from, to, out var start, out var endExclusive, out var rangeError))
        {
            return BadRequest(rangeError);
        }

        var query = IncludeOrderGraph(_context.Orders.AsNoTracking());
        query = _permissionService.ApplyOrderScope(query, actor);
        query = query.Where(order => order.OrderDate >= start && order.OrderDate < endExclusive);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = NormalizeStatus(status);
            if (normalizedStatus == null)
            {
                return BadRequest(new ApiError("Trạng thái đơn hàng không hợp lệ."));
            }

            query = query.Where(order => order.Status == normalizedStatus);
        }

        if (branchId.HasValue)
        {
            query = query.Where(order => order.BranchId == branchId.Value);
        }

        var orders = await query
            .OrderByDescending(order => order.OrderDate)
            .ThenByDescending(order => order.Id)
            .Take(Math.Clamp(take, 1, 500))
            .ToListAsync();

        return orders.Select(DtoMapper.ToOrderDto).ToList();
    }

    [HttpGet("today")]
    public Task<ActionResult<IReadOnlyList<OrderDto>>> GetTodayOrders(string? status = null, int? branchId = null, int take = 200)
    {
        var today = DateTime.Today;
        return GetOrders(today, today, status, branchId, take);
    }

    [HttpGet("history")]
    public Task<ActionResult<IReadOnlyList<OrderDto>>> GetOrderHistory(
        DateTime? date = null,
        DateTime? from = null,
        DateTime? to = null,
        string? status = null,
        int? branchId = null,
        int take = 200)
    {
        var start = from ?? date;
        var end = to ?? date ?? from;
        return GetOrders(start, end, status, branchId, take);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        var actor = await _permissionService.GetActorAsync(User);
        if (actor == null)
        {
            return Unauthorized(new ApiError("Tài khoản không còn hợp lệ."));
        }

        var order = await _permissionService
            .ApplyOrderScope(IncludeOrderGraph(_context.Orders.AsNoTracking()), actor)
            .FirstOrDefaultAsync(item => item.Id == id);

        return order == null
            ? NotFound(new ApiError("Không tìm thấy đơn hàng."))
            : DtoMapper.ToOrderDto(order);
    }

    [HttpGet("pending-approval")]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetPendingApproval(DateTime? from = null, DateTime? to = null, int? branchId = null, int take = 100)
    {
        var actor = await _permissionService.GetActorAsync(User);
        if (actor == null)
        {
            return Unauthorized(new ApiError("Tài khoản không còn hợp lệ."));
        }

        var query = IncludeOrderGraph(_context.Orders.AsNoTracking())
            .Where(order => order.Status == OrderStatusOptions.PendingApproval);
        query = _permissionService.ApplyOrderScope(query, actor);

        if (from.HasValue || to.HasValue)
        {
            if (!TryResolveRange(from, to, out var start, out var endExclusive, out var rangeError))
            {
                return BadRequest(rangeError);
            }

            query = query.Where(order => order.OrderDate >= start && order.OrderDate < endExclusive);
        }

        if (branchId.HasValue)
        {
            query = query.Where(order => order.BranchId == branchId.Value);
        }

        var orders = await query
            .OrderByDescending(order => order.OrderDate)
            .ThenByDescending(order => order.Id)
            .Take(Math.Clamp(take, 1, 300))
            .ToListAsync();

        return orders.Select(DtoMapper.ToOrderDto).ToList();
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Policy = Policies.OrderManage)]
    public async Task<ActionResult<OrderDto>> UpdateStatus(int id, UpdateOrderStatusRequest request)
    {
        var normalizedStatus = NormalizeStatus(request.Status);
        if (normalizedStatus == null)
        {
            return BadRequest(new ApiError("Trạng thái đơn hàng không hợp lệ."));
        }

        var actor = await _permissionService.GetActorAsync(User);
        if (actor == null)
        {
            return Unauthorized(new ApiError("Tài khoản không còn hợp lệ."));
        }

        var order = await _permissionService
            .ApplyOrderScope(IncludeOrderGraph(_context.Orders), actor)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (order == null)
        {
            return NotFound(new ApiError("Không tìm thấy đơn hàng."));
        }

        order.Status = normalizedStatus;
        await _context.SaveChangesAsync();
        return DtoMapper.ToOrderDto(order);
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Policy = Policies.OrderManage)]
    public Task<ActionResult<OrderDto>> Approve(int id)
    {
        return DecidePendingOrderAsync(id, OrderStatusOptions.Processing);
    }

    [HttpPost("{id:int}/cancel")]
    [Authorize(Policy = Policies.OrderManage)]
    public Task<ActionResult<OrderDto>> Cancel(int id)
    {
        return DecidePendingOrderAsync(id, OrderStatusOptions.Cancelled);
    }

    [HttpPost("{id:int}/reject")]
    [Authorize(Policy = Policies.OrderManage)]
    public Task<ActionResult<OrderDto>> Reject(int id)
    {
        return Cancel(id);
    }

    private async Task<ActionResult<OrderDto>> DecidePendingOrderAsync(int id, string nextStatus)
    {
        var actor = await _permissionService.GetActorAsync(User);
        if (actor == null)
        {
            return Unauthorized(new ApiError("Tài khoản không còn hợp lệ."));
        }

        var order = await _permissionService
            .ApplyOrderScope(IncludeOrderGraph(_context.Orders), actor)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (order == null)
        {
            return NotFound(new ApiError("Không tìm thấy đơn hàng."));
        }

        if (!string.Equals(order.Status, OrderStatusOptions.PendingApproval, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new ApiError("Chỉ đơn hàng đang chờ phê duyệt mới có thể duyệt hoặc hủy từ hàng chờ."));
        }

        order.Status = nextStatus;
        await _context.SaveChangesAsync();
        return DtoMapper.ToOrderDto(order);
    }

    private static IQueryable<Order> IncludeOrderGraph(IQueryable<Order> query)
    {
        return query
            .Include(order => order.User)
            .Include(order => order.Branch)
            .Include(order => order.OrderDetails)
            .ThenInclude(detail => detail.Product);
    }

    private static bool TryResolveRange(DateTime? from, DateTime? to, out DateTime start, out DateTime endExclusive, out ApiError? error)
    {
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

    private static string? NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        var value = status.Trim();
        var exactMatch = OrderStatusOptions.All.FirstOrDefault(item => string.Equals(item, value, StringComparison.OrdinalIgnoreCase));
        if (exactMatch != null)
        {
            return exactMatch;
        }

        return value.Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .ToLowerInvariant() switch
            {
                "pending" or "pendingapproval" or "awaitingapproval" => OrderStatusOptions.PendingApproval,
                "processing" => OrderStatusOptions.Processing,
                "shipping" => OrderStatusOptions.Shipping,
                "completed" or "complete" => OrderStatusOptions.Completed,
                "cancelled" or "canceled" or "cancel" or "reject" or "rejected" => OrderStatusOptions.Cancelled,
                _ => null
            };
    }
}
