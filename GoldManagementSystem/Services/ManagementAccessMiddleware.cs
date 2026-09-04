using GoldManagementSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace GoldManagementSystem.Services
{
    public sealed class ManagementAccessMiddleware
    {
        private readonly RequestDelegate _next;

        public ManagementAccessMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, IManagementPermissionService permissions, UserManager<AppUser> userManager)
        {
            var feature = ResolveFeature(context.Request.Path.Value ?? string.Empty);
            if (feature == null || context.User.Identity?.IsAuthenticated != true || context.User.IsInRole(RoleCatalog.Admin))
            {
                await _next(context);
                return;
            }

            var user = await userManager.GetUserAsync(context.User);
            var explicitBranchId = await TryGetBranchIdAsync(context);
            var branchId = explicitBranchId
                ?? (feature == ManagementFeatureCatalog.WarehouseSuppliers ? null : user?.BranchId);
            if (!await permissions.CanAsync(context.User, feature, branchId))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Ban khong co quyen truy cap chuc nang nay.");
                return;
            }

            await _next(context);
        }

        private static async Task<int?> TryGetBranchIdAsync(HttpContext context)
        {
            if (int.TryParse(context.Request.Query["branchId"], out var queryId)) return queryId;
            if (int.TryParse(context.Request.RouteValues["branchId"]?.ToString(), out var routeId)) return routeId;
            if (context.Request.HasFormContentType)
            {
                var form = await context.Request.ReadFormAsync();
                if (int.TryParse(form["branchId"], out var formId)) return formId;
                if (int.TryParse(form["BranchId"], out formId)) return formId;
            }
            return null;
        }

        private static string ResolveFeature(string rawPath)
        {
            var path = rawPath.ToLowerInvariant();
            if (path.StartsWith("/management")) return null; // Controller kiểm tra theo dashboard cụ thể.
            if (path.StartsWith("/production"))
            {
                if (path.Contains("activate")
                    || path.Contains("release")
                    || path.Contains("reviewloss")
                    || path.Contains("updateworkshop")
                    || path.Contains("setworkshopactive")
                    || path.Contains("updatelosspolicy")
                    || path.Contains("activatelosspolicy")
                    || path.Contains("quality")
                    || path.Contains("close"))
                {
                    return ManagementFeatureCatalog.ProductionApprove;
                }

                if (path.Contains("customerjob") || path.Contains("recordcustomermaterialissue"))
                    return ManagementFeatureCatalog.ProductionCustomerJobs;

                if (path is "/production" or "/production/index")
                    return ManagementFeatureCatalog.ProductionView;

                return ManagementFeatureCatalog.ProductionOperate;
            }
            if (path.StartsWith("/pricing"))
                return path.Contains("/approve") || path.Contains("/expire")
                    ? ManagementFeatureCatalog.PriceApprove
                    : ManagementFeatureCatalog.PriceManage;
            if (path.StartsWith("/goldbar")) return ManagementFeatureCatalog.GoldBarCompliance;
            if (path.StartsWith("/aftersales"))
                return path.Contains("approve") || path.Contains("pay") || path.Contains("processrefund")
                    ? ManagementFeatureCatalog.AfterSalesApprove
                    : ManagementFeatureCatalog.AfterSalesManage;
            if (path.Contains("/admin/usermanagement") || path.Contains("/admin/createuser") || path.Contains("/admin/updateuser") || path.Contains("/admin/deleteuser") || path.Contains("/admin/toggleuser")) return ManagementFeatureCatalog.SystemUsers;
            if (path.Contains("/admin/branchmanagement") || path.Contains("/admin/createbranch") || path.Contains("/admin/togglebranch")) return ManagementFeatureCatalog.SystemBranches;
            if (path.Contains("/admin/branchteam") || path.Contains("/admin/addexistingmember") || path.Contains("/admin/createbranchmember") || path.Contains("/admin/removebranchmember")) return ManagementFeatureCatalog.PeopleView;
            if (path.Contains("/admin/inventory") || path.Contains("purchaseorder") || path.Contains("goodsreceipt") || path.Contains("warehouse")) return ManagementFeatureCatalog.WarehouseReceipts;
            if (path == "/admin/suppliermanagement" || path == "/admin/createsupplier" || path == "/admin/updatesupplier" || path == "/admin/deletesupplier" || path.Contains("togglesupplier")) return ManagementFeatureCatalog.WarehouseSuppliers;
            if (path.Contains("/products/admin")) return ManagementFeatureCatalog.ProductsView;
            if (path.Contains("/products/create") || path.Contains("/products/edit") || path.Contains("/products/delete") || path.Contains("/products/priority")) return ManagementFeatureCatalog.ProductsEdit;
            return null;
        }
    }
}
