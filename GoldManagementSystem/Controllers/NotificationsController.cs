using GoldManagementSystem.Data;
using GoldManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GoldManagementSystem.Controllers
{
    [Authorize]
    public sealed class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public NotificationsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var userId = _userManager.GetUserId(User);
            var items = await _context.SystemNotifications.AsNoTracking()
                .Where(item => item.UserId == userId)
                .OrderByDescending(item => item.CreatedAt).Take(50)
                .Select(item => new { item.Id, item.Title, Body = item.Message, item.Link, item.Type, item.IsRead, item.CreatedAt })
                .ToListAsync();
            return Json(items);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Read(int id)
        {
            var userId = _userManager.GetUserId(User);
            var item = await _context.SystemNotifications.FirstOrDefaultAsync(notification => notification.Id == id && notification.UserId == userId);
            if (item == null) return NotFound();
            item.IsRead = true;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ReadAll()
        {
            var userId = _userManager.GetUserId(User);
            var items = await _context.SystemNotifications.Where(notification => notification.UserId == userId).ToListAsync();
            _context.SystemNotifications.RemoveRange(items);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}
