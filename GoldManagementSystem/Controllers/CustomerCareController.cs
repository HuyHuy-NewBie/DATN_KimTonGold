using GoldManagementSystem.Data;
using GoldManagementSystem.Hubs;
using GoldManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GoldManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerCareController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IHubContext<SupportChatHub> _hubContext;

        public CustomerCareController(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            IHubContext<SupportChatHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        private async Task ConsolidateDuplicateSessionsAsync()
        {
            try
            {
                // 1. Phân nhóm theo CustomerId (dành cho người dùng đã đăng nhập)
                var customerIdsWithDuplicates = await _context.SupportChatSessions
                    .Where(s => s.CustomerId != null)
                    .GroupBy(s => s.CustomerId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToListAsync();

                foreach (var customerId in customerIdsWithDuplicates)
                {
                    var sessions = await _context.SupportChatSessions
                        .Include(s => s.Messages)
                        .Where(s => s.CustomerId == customerId)
                        .OrderByDescending(s => s.UpdatedAt)
                        .ToListAsync();

                    if (sessions.Count <= 1) continue;

                    var primary = sessions.First();
                    var duplicates = sessions.Skip(1).ToList();

                    foreach (var dup in duplicates)
                    {
                        foreach (var msg in dup.Messages.ToList())
                        {
                            msg.SupportChatSessionId = primary.Id;
                        }
                        primary.UnreadByStaffCount += dup.UnreadByStaffCount;
                        if (string.IsNullOrWhiteSpace(primary.LastMessage)) primary.LastMessage = dup.LastMessage;
                        _context.SupportChatSessions.Remove(dup);
                    }
                }

                // 2. Phân nhóm theo CustomerEmail (dành cho khách chưa đăng nhập hoặc trùng email)
                var emailsWithDuplicates = await _context.SupportChatSessions
                    .Where(s => s.CustomerId == null && !string.IsNullOrWhiteSpace(s.CustomerEmail))
                    .GroupBy(s => s.CustomerEmail)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToListAsync();

                foreach (var email in emailsWithDuplicates)
                {
                    var sessions = await _context.SupportChatSessions
                        .Include(s => s.Messages)
                        .Where(s => s.CustomerId == null && s.CustomerEmail == email)
                        .OrderByDescending(s => s.UpdatedAt)
                        .ToListAsync();

                    if (sessions.Count <= 1) continue;

                    var primary = sessions.First();
                    var duplicates = sessions.Skip(1).ToList();

                    foreach (var dup in duplicates)
                    {
                        foreach (var msg in dup.Messages.ToList())
                        {
                            msg.SupportChatSessionId = primary.Id;
                        }
                        primary.UnreadByStaffCount += dup.UnreadByStaffCount;
                        if (string.IsNullOrWhiteSpace(primary.LastMessage)) primary.LastMessage = dup.LastMessage;
                        _context.SupportChatSessions.Remove(dup);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch
            {
                // Tránh throw ngoại lệ nếu có xung đột nhỏ
            }
        }

        // ==========================================
        // DÀNH CHO KHÁCH HÀNG (CLIENT SIDE)
        // ==========================================

        [HttpPost("start-session")]
        public async Task<IActionResult> StartSession([FromBody] StartSessionRequest model)
        {
            AppUser currentUser = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                currentUser = await _userManager.GetUserAsync(User);
            }

            var customerName = currentUser?.FullName ?? model?.CustomerName;
            if (string.IsNullOrWhiteSpace(customerName)) customerName = "Khách hàng";

            var customerEmail = currentUser?.Email ?? model?.CustomerEmail;
            var customerPhone = currentUser?.PhoneNumber ?? model?.CustomerPhone;

            SupportChatSession session = null;

            // Tự động gộp dữ liệu cũ nếu phát sinh trùng lặp trước đó
            await ConsolidateDuplicateSessionsAsync();

            // 1. Nếu người dùng ĐÃ ĐĂNG NHẬP -> Tìm cuộc trò chuyện duy nhất theo CustomerId hoặc Email
            if (currentUser != null)
            {
                session = await _context.SupportChatSessions
                    .Include(s => s.Messages)
                    .OrderByDescending(s => s.UpdatedAt)
                    .FirstOrDefaultAsync(s => s.CustomerId == currentUser.Id);

                if (session == null && !string.IsNullOrWhiteSpace(currentUser.Email))
                {
                    session = await _context.SupportChatSessions
                        .Include(s => s.Messages)
                        .OrderByDescending(s => s.UpdatedAt)
                        .FirstOrDefaultAsync(s => s.CustomerEmail == currentUser.Email);

                    if (session != null)
                    {
                        session.CustomerId = currentUser.Id; // Gán CustomerId cho session cũ
                    }
                }
            }
            // 2. Khách chưa đăng nhập -> Tìm theo SessionCode gửi từ client hoặc Email
            else
            {
                if (!string.IsNullOrWhiteSpace(model?.SessionCode))
                {
                    session = await _context.SupportChatSessions
                        .Include(s => s.Messages)
                        .FirstOrDefaultAsync(s => s.SessionCode == model.SessionCode);
                }

                if (session == null && !string.IsNullOrWhiteSpace(customerEmail))
                {
                    session = await _context.SupportChatSessions
                        .Include(s => s.Messages)
                        .OrderByDescending(s => s.UpdatedAt)
                        .FirstOrDefaultAsync(s => s.CustomerEmail == customerEmail);
                }
            }

            bool isNewSession = false;

            // Nếu ĐÃ CÓ SESSION -> DÙNG LẠI SESSION DUY NHẤT NÀY!
            if (session != null)
            {
                session.CustomerName = customerName;
                if (!string.IsNullOrWhiteSpace(customerEmail)) session.CustomerEmail = customerEmail;
                if (!string.IsNullOrWhiteSpace(customerPhone)) session.CustomerPhone = customerPhone;

                // Nếu phiên chat từng bị Closed, tự mở lại để tiếp nhận tin nhắn mới
                if (session.Status == "Closed")
                {
                    session.Status = string.IsNullOrWhiteSpace(session.AssignedStaffId) ? "Waiting" : "Active";
                }
                session.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            else
            {
                // Chỉ tạo mới 1 lần duy nhất nếu tài khoản chưa từng có cuộc trò chuyện nào
                isNewSession = true;
                var code = "CHAT_" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                session = new SupportChatSession
                {
                    SessionCode = code,
                    CustomerId = currentUser?.Id,
                    CustomerName = customerName,
                    CustomerEmail = customerEmail,
                    CustomerPhone = customerPhone,
                    Status = "Waiting",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var welcomeMsg = new SupportChatMessage
                {
                    SenderName = "Hệ thống CSKH",
                    SenderRole = "System",
                    Message = "Xin chào! Nhân viên hỗ trợ sẽ kết nối với bạn trong giây lát. Bạn có thể gửi câu hỏi ngay tại đây ạ.",
                    CreatedAt = DateTime.UtcNow
                };
                session.Messages.Add(welcomeMsg);

                _context.SupportChatSessions.Add(session);
                await _context.SaveChangesAsync();
            }

            if (isNewSession)
            {
                await _hubContext.Clients.Group("role:cskh_staff").SendAsync("NewWaitingSession", new
                {
                    sessionId = session.Id,
                    sessionCode = session.SessionCode,
                    customerName = session.CustomerName,
                    customerEmail = session.CustomerEmail,
                    createdAt = session.CreatedAt.ToString("HH:mm dd/MM")
                });
            }

            return Ok(new
            {
                success = true,
                sessionCode = session.SessionCode,
                sessionId = session.Id,
                status = session.Status,
                assignedStaffName = session.AssignedStaffName,
                messages = session.Messages.OrderBy(m => m.CreatedAt).Select(m => new
                {
                    id = m.Id,
                    senderName = m.SenderName,
                    senderRole = m.SenderRole,
                    message = m.Message,
                    createdAt = m.CreatedAt.ToString("HH:mm")
                })
            });
        }

        [HttpGet("session/{sessionCode}/messages")]
        public async Task<IActionResult> GetSessionMessages(string sessionCode)
        {
            var session = await _context.SupportChatSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.SessionCode == sessionCode);

            if (session == null && User.Identity?.IsAuthenticated == true)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    session = await _context.SupportChatSessions
                        .Include(s => s.Messages)
                        .OrderByDescending(s => s.UpdatedAt)
                        .FirstOrDefaultAsync(s => s.CustomerId == currentUser.Id);
                }
            }

            if (session == null) return NotFound(new { message = "Không tìm thấy phiên chat" });

            return Ok(new
            {
                success = true,
                sessionCode = session.SessionCode,
                status = session.Status,
                assignedStaffName = session.AssignedStaffName,
                messages = session.Messages.OrderBy(m => m.CreatedAt).Select(m => new
                {
                    id = m.Id,
                    senderName = m.SenderName,
                    senderRole = m.SenderRole,
                    message = m.Message,
                    createdAt = m.CreatedAt.ToString("HH:mm")
                })
            });
        }

        [HttpPost("send-message")]
        public async Task<IActionResult> CustomerSendMessage([FromBody] SendMessageRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.Message))
                return BadRequest(new { message = "Nội dung không hợp lệ" });

            SupportChatSession session = null;

            if (!string.IsNullOrWhiteSpace(model.SessionCode))
            {
                session = await _context.SupportChatSessions
                    .Include(s => s.Messages)
                    .FirstOrDefaultAsync(s => s.SessionCode == model.SessionCode);
            }

            if (session == null && User.Identity?.IsAuthenticated == true)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    session = await _context.SupportChatSessions
                        .Include(s => s.Messages)
                        .OrderByDescending(s => s.UpdatedAt)
                        .FirstOrDefaultAsync(s => s.CustomerId == currentUser.Id);
                }
            }

            if (session == null) return NotFound(new { message = "Không tìm thấy phiên chat" });

            if (session.Status == "Closed")
            {
                session.Status = string.IsNullOrWhiteSpace(session.AssignedStaffId) ? "Waiting" : "Active";
            }

            var senderName = session.CustomerName;
            var chatMsg = new SupportChatMessage
            {
                SupportChatSessionId = session.Id,
                SenderId = session.CustomerId,
                SenderName = senderName,
                SenderRole = "Customer",
                Message = model.Message.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            session.LastMessage = model.Message.Trim();
            session.UnreadByStaffCount += 1;
            session.UpdatedAt = DateTime.UtcNow;

            _context.SupportChatMessages.Add(chatMsg);
            await _context.SaveChangesAsync();

            var msgPayload = new
            {
                id = chatMsg.Id,
                sessionId = session.Id,
                sessionCode = session.SessionCode,
                senderName = chatMsg.SenderName,
                senderRole = chatMsg.SenderRole,
                message = chatMsg.Message,
                createdAt = chatMsg.CreatedAt.ToString("HH:mm")
            };

            // Broadcast to session SignalR group and staff group
            await _hubContext.Clients.Group($"session:{session.SessionCode}").SendAsync("ReceiveMessage", msgPayload);
            await _hubContext.Clients.Group("role:cskh_staff").SendAsync("StaffSessionUpdated", new
            {
                sessionId = session.Id,
                sessionCode = session.SessionCode,
                lastMessage = session.LastMessage,
                unreadByStaff = session.UnreadByStaffCount,
                updatedAt = session.UpdatedAt.ToString("HH:mm")
            });

            return Ok(new { success = true, data = msgPayload });
        }

        [HttpPost("submit-feedback")]
        public async Task<IActionResult> SubmitFeedback([FromBody] SubmitFeedbackRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Content))
                return BadRequest(new { message = "Vui lòng nhập nội dung đánh giá" });

            AppUser currentUser = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                currentUser = await _userManager.GetUserAsync(User);
            }

            var customerName = currentUser?.FullName ?? model.CustomerName;
            if (string.IsNullOrWhiteSpace(customerName)) customerName = "Khách hàng";

            var feedback = new CustomerFeedback
            {
                CustomerId = currentUser?.Id,
                CustomerName = customerName,
                CustomerEmail = currentUser?.Email ?? model.CustomerEmail,
                CustomerPhone = currentUser?.PhoneNumber ?? model.CustomerPhone,
                BranchId = model.BranchId,
                Rating = Math.Clamp(model.Rating, 1, 5),
                Category = string.IsNullOrWhiteSpace(model.Category) ? "Sản phẩm" : model.Category,
                ProductId = model.ProductId,
                ProductName = model.ProductName,
                Content = model.Content.Trim(),
                Status = "Chờ xử lý",
                CreatedAt = DateTime.UtcNow
            };

            _context.CustomerFeedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            // Notify staff
            await _hubContext.Clients.Group("role:cskh_staff").SendAsync("NewFeedbackSubmitted", new
            {
                id = feedback.Id,
                customerName = feedback.CustomerName,
                rating = feedback.Rating,
                category = feedback.Category,
                content = feedback.Content,
                createdAt = feedback.CreatedAt.ToString("HH:mm dd/MM")
            });

            return Ok(new { success = true, message = "Cảm ơn bạn đã gửi đánh giá! Chúng tôi đã ghi nhận phản hồi của bạn." });
        }

        // ==========================================
        // DÀNH CHO QUẢN TRỊ / CSKH STAFF (ADMIN SIDE)
        // ==========================================

        [Authorize]
        [HttpGet("admin/sessions")]
        public async Task<IActionResult> GetAdminSessions([FromQuery] string type = "waiting")
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            // Tự động gộp các đoạn chat bị lặp trước đó của cùng một tài khoản
            await ConsolidateDuplicateSessionsAsync();

            var query = _context.SupportChatSessions.AsQueryable();

            if (type == "my")
            {
                query = query.Where(s => s.AssignedStaffId == currentUser.Id && s.Status != "Closed");
            }
            else // "waiting" — bao gồm tất cả các cuộc trò chuyện chưa bị đóng bởi nhân viên
            {
                query = query.Where(s => s.Status != "Closed");
            }


            var rawSessions = await query
                .OrderByDescending(s => s.UpdatedAt)
                .Select(s => new
                {
                    id = s.Id,
                    sessionCode = s.SessionCode,
                    customerName = s.CustomerName,
                    customerEmail = s.CustomerEmail,
                    customerPhone = s.CustomerPhone,
                    status = s.Status,
                    assignedStaffId = s.AssignedStaffId,
                    assignedStaffName = s.AssignedStaffName,
                    lastMessage = s.LastMessage,
                    unreadByStaff = s.UnreadByStaffCount,
                    updatedAt = s.UpdatedAt
                })
                .ToListAsync();

            var sessions = rawSessions.Select(s => new
            {
                id = s.id,
                sessionCode = s.sessionCode,
                customerName = s.customerName,
                customerEmail = s.customerEmail,
                customerPhone = s.customerPhone,
                status = s.status,
                assignedStaffId = s.assignedStaffId,
                assignedStaffName = s.assignedStaffName,
                lastMessage = s.lastMessage,
                unreadByStaff = s.unreadByStaff,
                updatedAt = s.updatedAt.ToString("HH:mm dd/MM"),
                updatedAtTime = s.updatedAt.ToString("HH:mm")
            }).ToList();

            var waitingCount = await _context.SupportChatSessions.CountAsync(s => s.Status != "Closed" && (s.Status == "Waiting" || s.AssignedStaffId == null));
            var myCount = await _context.SupportChatSessions.CountAsync(s => s.AssignedStaffId == currentUser.Id && s.Status != "Closed");

            return Ok(new
            {
                success = true,
                waitingCount,
                myCount,
                sessions
            });
        }

        [Authorize]
        [HttpPost("admin/accept-session")]
        public async Task<IActionResult> AcceptSession([FromBody] SessionActionRequest model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var session = await _context.SupportChatSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == model.SessionId || s.SessionCode == model.SessionCode);

            if (session == null) return NotFound(new { message = "Không tìm thấy phiên chat" });

            session.AssignedStaffId = currentUser.Id;
            session.AssignedStaffName = currentUser.FullName ?? currentUser.UserName;
            session.Status = "Active";
            session.UnreadByStaffCount = 0;
            session.UpdatedAt = DateTime.UtcNow;

            var sysMsg = new SupportChatMessage
            {
                SupportChatSessionId = session.Id,
                SenderName = "Hệ thống CSKH",
                SenderRole = "System",
                Message = $"Nhân viên {session.AssignedStaffName} đã tiếp nhận cuộc trò chuyện.",
                CreatedAt = DateTime.UtcNow
            };
            session.Messages.Add(sysMsg);

            await _context.SaveChangesAsync();

            var payload = new
            {
                sessionId = session.Id,
                sessionCode = session.SessionCode,
                status = session.Status,
                assignedStaffName = session.AssignedStaffName
            };

            await _hubContext.Clients.Group($"session:{session.SessionCode}").SendAsync("SessionStatusChanged", payload);
            await _hubContext.Clients.Group("role:cskh_staff").SendAsync("SessionStatusChanged", payload);

            return Ok(new { success = true, data = payload });
        }

        [Authorize]
        [HttpPost("admin/close-session")]
        public async Task<IActionResult> CloseSession([FromBody] SessionActionRequest model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var session = await _context.SupportChatSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == model.SessionId || s.SessionCode == model.SessionCode);

            if (session == null) return NotFound(new { message = "Không tìm thấy phiên chat" });

            session.Status = "Closed";
            session.UpdatedAt = DateTime.UtcNow;

            var sysMsg = new SupportChatMessage
            {
                SupportChatSessionId = session.Id,
                SenderName = "Hệ thống CSKH",
                SenderRole = "System",
                Message = "Cuộc trò chuyện hỗ trợ đã kết thúc. Cảm ơn bạn đã liên hệ!",
                CreatedAt = DateTime.UtcNow
            };
            session.Messages.Add(sysMsg);

            await _context.SaveChangesAsync();

            var payload = new
            {
                sessionId = session.Id,
                sessionCode = session.SessionCode,
                status = session.Status
            };

            await _hubContext.Clients.Group($"session:{session.SessionCode}").SendAsync("SessionStatusChanged", payload);
            await _hubContext.Clients.Group("role:cskh_staff").SendAsync("SessionStatusChanged", payload);

            return Ok(new { success = true });
        }

        [Authorize]
        [HttpPost("admin/send-message")]
        public async Task<IActionResult> StaffSendMessage([FromBody] SendMessageRequest model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(model.Message))
                return BadRequest(new { message = "Nội dung không hợp lệ" });

            var session = await _context.SupportChatSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == model.SessionId || s.SessionCode == model.SessionCode);

            if (session == null) return NotFound(new { message = "Không tìm thấy phiên chat" });

            var staffName = currentUser.FullName ?? currentUser.UserName;
            var chatMsg = new SupportChatMessage
            {
                SupportChatSessionId = session.Id,
                SenderId = currentUser.Id,
                SenderName = staffName,
                SenderRole = "Staff",
                Message = model.Message.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            session.LastMessage = model.Message.Trim();
            session.UpdatedAt = DateTime.UtcNow;
            session.AssignedStaffId = currentUser.Id;
            session.AssignedStaffName = staffName;
            session.Status = "Active";
            session.UnreadByCustomerCount += 1;
            session.UnreadByStaffCount = 0;

            _context.SupportChatMessages.Add(chatMsg);
            await _context.SaveChangesAsync();

            var msgPayload = new
            {
                id = chatMsg.Id,
                sessionId = session.Id,
                sessionCode = session.SessionCode,
                senderName = chatMsg.SenderName,
                senderRole = chatMsg.SenderRole,
                message = chatMsg.Message,
                createdAt = chatMsg.CreatedAt.ToString("HH:mm")
            };

            await _hubContext.Clients.Group($"session:{session.SessionCode}").SendAsync("ReceiveMessage", msgPayload);

            return Ok(new { success = true, data = msgPayload });
        }

        [Authorize]
        [HttpGet("admin/feedbacks")]
        public async Task<IActionResult> GetAdminFeedbacks([FromQuery] int? rating = null, [FromQuery] string status = null, [FromQuery] string search = null, [FromQuery] int? branchId = null)
        {
            var query = _context.CustomerFeedbacks.AsQueryable();

            if (rating.HasValue && rating.Value > 0)
            {
                query = query.Where(f => f.Rating == rating.Value);
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(f => f.Status == status);
            }
            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(f => f.BranchId == branchId.Value || f.BranchId == null);
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(f => (f.CustomerName != null && f.CustomerName.ToLower().Contains(s))
                                      || (f.CustomerPhone != null && f.CustomerPhone.ToLower().Contains(s))
                                      || (f.CustomerEmail != null && f.CustomerEmail.ToLower().Contains(s))
                                      || (f.Content != null && f.Content.ToLower().Contains(s))
                                      || (f.ProductName != null && f.ProductName.ToLower().Contains(s)));
            }

            var rawList = await query
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new
                {
                    id = f.Id,
                    customerId = f.CustomerId,
                    customerName = f.CustomerName,
                    customerEmail = f.CustomerEmail,
                    customerPhone = f.CustomerPhone,
                    rating = f.Rating,
                    category = f.Category,
                    productId = f.ProductId,
                    productName = f.ProductName,
                    content = f.Content,
                    status = f.Status,
                    branchId = f.BranchId,
                    adminResponse = f.AdminResponse,
                    respondedAt = f.RespondedAt,
                    respondedByName = f.RespondedByName,
                    createdAt = f.CreatedAt
                })
                .ToListAsync();

            var list = rawList.Select(f => new
            {
                id = f.id,
                customerId = f.customerId,
                customerName = f.customerName,
                customerEmail = f.customerEmail,
                customerPhone = f.customerPhone,
                phoneOrEmail = !string.IsNullOrWhiteSpace(f.customerPhone) ? f.customerPhone : (f.customerEmail ?? string.Empty),
                rating = f.rating,
                category = f.category,
                productId = f.productId,
                productName = f.productName,
                content = f.content,
                status = f.status,
                branchId = f.branchId,
                adminResponse = f.adminResponse,
                respondedAt = f.respondedAt.HasValue ? f.respondedAt.Value.ToString("HH:mm dd/MM/yyyy") : null,
                respondedByName = f.respondedByName,
                createdAt = f.createdAt.ToString("HH:mm dd/MM/yyyy")
            }).ToList();

            var totalCount = await _context.CustomerFeedbacks.CountAsync();
            var pendingCount = await _context.CustomerFeedbacks.CountAsync(f => f.Status == "Chờ xử lý");
            var processedCount = await _context.CustomerFeedbacks.CountAsync(f => f.Status == "Đã xử lý");
            var avgRating = totalCount > 0 ? await _context.CustomerFeedbacks.AverageAsync(f => (double)f.Rating) : 5.0;

            return Ok(new
            {
                success = true,
                totalCount,
                pendingCount,
                processedCount,
                avgRating = Math.Round(avgRating, 1),
                feedbacks = list
            });
        }

        [Authorize]
        [HttpPost("admin/reply-feedback")]
        public async Task<IActionResult> ReplyFeedback([FromBody] ReplyFeedbackRequest model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var feedback = await _context.CustomerFeedbacks.FindAsync(model.FeedbackId);
            if (feedback == null) return NotFound(new { message = "Không tìm thấy đánh giá" });

            feedback.AdminResponse = model.Response?.Trim();
            feedback.Status = "Đã xử lý";
            feedback.RespondedAt = DateTime.UtcNow;
            feedback.RespondedByName = currentUser.FullName ?? currentUser.UserName;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Đã lưu phản hồi đánh giá khách hàng thành công" });
        }

        [Authorize]
        [HttpPost("admin/mark-feedback-processed")]
        public async Task<IActionResult> MarkFeedbackProcessed([FromBody] ReplyFeedbackRequest model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var feedback = await _context.CustomerFeedbacks.FindAsync(model.FeedbackId);
            if (feedback == null) return NotFound(new { message = "Không tìm thấy đánh giá" });

            feedback.Status = "Đã xử lý";
            feedback.RespondedAt = DateTime.UtcNow;
            feedback.RespondedByName = currentUser.FullName ?? currentUser.UserName;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Đã đánh dấu xử lý đánh giá" });
        }

        [Authorize]
        [HttpPost("admin/delete-feedback")]
        public async Task<IActionResult> DeleteFeedback([FromBody] ReplyFeedbackRequest model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var feedback = await _context.CustomerFeedbacks.FindAsync(model.FeedbackId);
            if (feedback == null) return NotFound(new { message = "Không tìm thấy đánh giá" });

            _context.CustomerFeedbacks.Remove(feedback);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Đã xóa đánh giá thành công" });
        }
    }

    // DTO Models
    public class StartSessionRequest
    {
        public string SessionCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
    }

    public class SendMessageRequest
    {
        public int SessionId { get; set; }
        public string SessionCode { get; set; }
        public string Message { get; set; }
    }

    public class SessionActionRequest
    {
        public int SessionId { get; set; }
        public string SessionCode { get; set; }
    }

    public class SubmitFeedbackRequest
    {
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public int? BranchId { get; set; }
        public int Rating { get; set; } = 5;
        public string Category { get; set; }
        public int? ProductId { get; set; }
        public string ProductName { get; set; }
        public string Content { get; set; }
    }

    public class ReplyFeedbackRequest
    {
        public int FeedbackId { get; set; }
        public string Response { get; set; }
    }
}
