using GoldManagementSystem.Data;
using GoldManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace GoldManagementSystem.Services
{
    public sealed class AuditTrailMiddleware
    {
        private readonly RequestDelegate _next;

        public AuditTrailMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, IServiceScopeFactory scopeFactory)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var isManagement = path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/Management", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/Production", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/Products/Admin", StringComparison.OrdinalIgnoreCase);
            var isAjax = string.Equals(context.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
            var isInteractiveSalesPage = HttpMethods.IsGet(context.Request.Method)
                && !isAjax
                && !path.StartsWith("/Notifications", StringComparison.OrdinalIgnoreCase)
                && !path.StartsWith("/Chat", StringComparison.OrdinalIgnoreCase)
                && !path.StartsWith("/Account", StringComparison.OrdinalIgnoreCase);
            var shouldLog = context.User.Identity?.IsAuthenticated == true
                && ((isManagement && (!HttpMethods.IsGet(context.Request.Method) || !isAjax))
                    || (!isManagement && (!HttpMethods.IsGet(context.Request.Method) || isInteractiveSalesPage)));

            await _next(context);
            if (!shouldLog) return;

            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var userId = users.GetUserId(context.User);
                int? branchId = int.TryParse(context.Request.Query["branchId"], out var id) ? id : null;
                if (!branchId.HasValue && !string.IsNullOrWhiteSpace(userId))
                    branchId = (await users.FindByIdAsync(userId))?.BranchId;

                db.ManagementAuditLogs.Add(new ManagementAuditLog
                {
                    UserId = userId,
                    UserName = context.User.Identity?.Name,
                    Area = isManagement ? "Management" : "Sales",
                    HttpMethod = context.Request.Method,
                    Action = path,
                    EntityType = context.Request.RouteValues["controller"]?.ToString(),
                    EntityId = context.Request.RouteValues["id"]?.ToString(),
                    BranchId = branchId,
                    Details = context.Request.QueryString.Value,
                    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                    Succeeded = context.Response.StatusCode < 400
                });
                if (path.StartsWith("/production", StringComparison.OrdinalIgnoreCase) && !HttpMethods.IsGet(context.Request.Method) && !string.IsNullOrWhiteSpace(userId))
                {
                    var form = context.Request.HasFormContentType ? await context.Request.ReadFormAsync() : null;
                    if (!branchId.HasValue && form != null && int.TryParse(form["BranchId"].ToString(), out var formBranchId)) branchId = formBranchId;
                    var snapshot = form == null
                        ? context.Request.QueryString.Value ?? string.Empty
                        : JsonSerializer.Serialize(form.Keys.Where(key => !key.Equals("__RequestVerificationToken", StringComparison.OrdinalIgnoreCase)).ToDictionary(key => key, key => form[key].ToString()));
                    db.ProductionAuditLogs.Add(new ProductionAuditLog
                    {
                        Action = path,
                        EntityType = context.Request.RouteValues["action"]?.ToString() ?? "Production",
                        EntityId = int.TryParse(context.Request.RouteValues["id"]?.ToString(), out var entityId) ? entityId : null,
                        BranchId = branchId,
                        ActorUserId = userId,
                        Snapshot = snapshot.Length > 2000 ? snapshot[..2000] : snapshot,
                        Succeeded = context.Response.StatusCode < 400
                    });
                }
                await db.SaveChangesAsync();
            }
            catch
            {
                // Nhật ký không được làm gián đoạn nghiệp vụ chính.
            }
        }
    }
}
