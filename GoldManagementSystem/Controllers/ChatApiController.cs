using GoldManagementSystem.Data;
using GoldManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GoldManagementSystem.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public ChatApiController(
            ApplicationDbContext context,
            IConfiguration config,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _config = config;
            _httpClientFactory = httpClientFactory;
        }  
        //1//


        public class ChatRequest
        {
            public List<ChatMessage> History { get; set; } = new();
            public string UserMessage { get; set; } = string.Empty;
        }

        public class ChatMessage
        {
            public string Role { get; set; } = "user";
            public string Text { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.UserMessage))
                return BadRequest(new { error = "Tin nhắn không được để trống." });

            var settings = await _context.ChatSettings.FirstOrDefaultAsync()
                           ?? new ChatSettings();

            string reply;
            var apiKey = _config["GeminiAI:ApiKey"] ?? string.Empty;

            // Thử gọi AI nếu có key hợp lệ, ngược lại dùng rule-based
            bool useAI = !string.IsNullOrWhiteSpace(apiKey)
                         && apiKey != "YOUR_GEMINI_API_KEY_HERE";

            if (useAI)
            {
                try
                {
                    var systemPrompt = BuildSystemPrompt(settings);
                    if (apiKey.StartsWith("sk-or-", StringComparison.OrdinalIgnoreCase))
                        reply = await CallOpenRouterAsync(apiKey, systemPrompt, request.History, request.UserMessage);
                    else
                        reply = await CallGeminiAsync(apiKey, systemPrompt, request.History, request.UserMessage);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[ChatApi Error] Gemini/OpenRouter API call failed: {ex.Message}");
                    // Fallback sang rule-based nếu AI lỗi
                    reply = RuleBasedReply(request.UserMessage, settings);
                }
            }
            else
            {
                reply = RuleBasedReply(request.UserMessage, settings);
            }

            return Ok(new { reply });
        }

        // ── Rule-based reply (không cần AI, hoạt động offline) ─────────────
        private static string RuleBasedReply(string msg, ChatSettings s)
        {
            var m = msg.ToLower().Trim();
            var hotline = s.Hotline ?? "1800 9999";

            // Chào hỏi - Fix lỗi trùng 'hi' trong 'chi nhánh' / 'địa chỉ' bằng cách so khớp chính xác từ 'hi'
            if (ContainsAny(m, "xin chào", "chào", "hello", "alo") || m == "hi" || m.Split(' ').Contains("hi"))
                return $"Xin chào anh/chị! Em là trợ lý tư vấn của {s.ShopName}. " +
                       "Anh/chị cần tư vấn về giá sản phẩm, chọn size, bảo hành, thu đổi hay đặt hàng ạ? 😊";

            // Giá sản phẩm
            if (ContainsAny(m, "giá", "tiền công", "bao nhiêu", "chi phí", "giá vàng", "giá sản phẩm", "mua"))
            {
                if (!string.IsNullOrWhiteSpace(s.ProductPriceInfo))
                    return $"Về giá sản phẩm tại {s.ShopName}:\n\n{s.ProductPriceInfo}\n\n" +
                           $"Anh/chị muốn biết thêm chi tiết, liên hệ Hotline {hotline} để được báo giá chính xác nhé ạ! 💛";
                return $"Giá sản phẩm tại {s.ShopName} được tính theo công thức:\n" +
                       "Giá = Giá vàng hôm nay × Trọng lượng + Tiền công chế tác\n\n" +
                       $"Anh/chị vui lòng liên hệ Hotline {hotline} để được báo giá chính xác theo mẫu cụ thể ạ!";
            }

            // Chọn size
            if (ContainsAny(m, "size", "cỡ", "đo tay", "đo ngón", "số đeo", "số nhẫn", "chọn size"))
            {
                if (!string.IsNullOrWhiteSpace(s.SizeGuideInfo))
                    return $"Hướng dẫn chọn size tại {s.ShopName}:\n\n{s.SizeGuideInfo}";
                return "Để chọn size nhẫn chuẩn, anh/chị có thể:\n" +
                       "• Đo chu vi ngón tay bằng chỉ/giấy\n" +
                       "• Size VN: 5=49mm, 6=51.5mm, 7=54mm, 8=57mm, 9=59mm, 10=62mm\n\n" +
                       $"Hoặc ghé cửa hàng để được đo trực tiếp! Hotline: {hotline}";
            }

            // Bảo hành
            if (ContainsAny(m, "bảo hành", "sửa chữa", "đánh bóng", "bảo dưỡng", "hỏng", "lỗi"))
            {
                if (!string.IsNullOrWhiteSpace(s.WarrantyInfo))
                    return $"Chính sách bảo hành tại {s.ShopName}:\n\n{s.WarrantyInfo}";
                return $"Tại {s.ShopName}, sản phẩm được bảo hành chính hãng. " +
                       $"Anh/chị vui lòng liên hệ Hotline {hotline} để biết chi tiết chính sách bảo hành ạ!";
            }

            // Thu đổi
            if (ContainsAny(m, "đổi", "thu đổi", "bán lại", "đổi vàng", "thu mua", "trả lại"))
            {
                if (!string.IsNullOrWhiteSpace(s.ExchangePolicy))
                    return $"Chính sách thu đổi tại {s.ShopName}:\n\n{s.ExchangePolicy}";
                return $"Tại {s.ShopName} có chính sách thu đổi vàng trang sức. " +
                       $"Anh/chị liên hệ Hotline {hotline} để được tư vấn chi tiết về tỷ lệ thu đổi ạ!";
            }

            // Đặt hàng
            if (ContainsAny(m, "đặt hàng", "đặt cọc", "mua online", "thanh toán", "giao hàng", "ship", "đặt"))
            {
                if (!string.IsNullOrWhiteSpace(s.OrderProcess))
                    return $"Quy trình đặt hàng tại {s.ShopName}:\n\n{s.OrderProcess}";
                return $"Để đặt hàng tại {s.ShopName}:\n" +
                       "1. Chọn sản phẩm → Thêm vào giỏ hàng\n" +
                       "2. Điền thông tin → Xác nhận đơn\n" +
                       "3. Đặt cọc 30-50% → Chờ xác nhận\n" +
                       "4. Nhận hàng & thanh toán phần còn lại\n\n" +
                       $"Hotline hỗ trợ: {hotline} 📞";
            }

            // Địa chỉ / cửa hàng
            if (ContainsAny(m, "địa chỉ", "ở đâu", "cửa hàng", "chi nhánh", "tìm cửa hàng"))
            {
                var addr = !string.IsNullOrWhiteSpace(s.ShopAddress) ? s.ShopAddress : "vui lòng liên hệ hotline để biết địa chỉ gần nhất";
                return $"Địa chỉ {s.ShopName}: {addr}\n\nHotline: {hotline} 📞";
            }

            // Hotline / liên hệ
            if (ContainsAny(m, "hotline", "liên hệ", "số điện thoại", "gọi", "zalo"))
                return $"Thông tin liên hệ {s.ShopName}:\n📞 Hotline: {hotline}\n\nEm sẵn sàng hỗ trợ anh/chị! 😊";

            // Cảm ơn
            if (ContainsAny(m, "cảm ơn", "thank", "ok", "được rồi", "hiểu rồi"))
                return $"Dạ, cảm ơn anh/chị đã tin tưởng {s.ShopName}! " +
                       "Nếu cần tư vấn thêm, anh/chị cứ nhắn tin cho em nhé. " +
                       "Chúc anh/chị một ngày vui vẻ! 💛";

            // Mặc định
            return $"Dạ, em có thể tư vấn cho anh/chị về:\n" +
                   "• 💰 Giá sản phẩm & tiền công\n" +
                   "• 📏 Hướng dẫn chọn size\n" +
                   "• 🔧 Chính sách bảo hành\n" +
                   "• 🔄 Chính sách thu đổi\n" +
                   "• 🛍️ Quy trình đặt hàng\n\n" +
                   $"Hoặc liên hệ trực tiếp Hotline {hotline} để được hỗ trợ nhanh nhất ạ!";
        }

        private static bool ContainsAny(string text, params string[] keywords)
            => keywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));

        // ── Build System Prompt ─────────────────────────────────────────────
        private string BuildSystemPrompt(ChatSettings s)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Bạn là trợ lý tư vấn của tiệm vàng {s.ShopName}. Hotline: {s.Hotline}");
            if (!string.IsNullOrWhiteSpace(s.ShopAddress)) sb.AppendLine($"Địa chỉ: {s.ShopAddress}");
            sb.AppendLine("Phong cách: lịch sự, thân thiện, ngắn gọn, xưng em. Trả lời tiếng Việt. KHÔNG dùng markdown.");
            sb.AppendLine();
            AppendSection(sb, "GIÁ SẢN PHẨM", s.ProductPriceInfo, s.Hotline);
            AppendSection(sb, "CHỌN SIZE", s.SizeGuideInfo, s.Hotline);
            AppendSection(sb, "BẢO HÀNH", s.WarrantyInfo, s.Hotline);
            AppendSection(sb, "THU ĐỔI", s.ExchangePolicy, s.Hotline);
            AppendSection(sb, "ĐẶT HÀNG", s.OrderProcess, s.Hotline);
            return sb.ToString();
        }

        private static void AppendSection(StringBuilder sb, string title, string content, string hotline)
        {
            sb.AppendLine($"=== {title} ===");
            sb.AppendLine(string.IsNullOrWhiteSpace(content) ? $"[Chưa có - hướng dẫn gọi {hotline}]" : content.Trim());
            sb.AppendLine();
        }

        // ── Gọi Google Gemini (?key= param) ────────────────────────────────
        private async Task<string> CallGeminiAsync(string apiKey, string systemPrompt, List<ChatMessage> history, string userMessage)
        {
            var model       = _config["GeminiAI:Model"] ?? "gemini-2.0-flash";
            var maxTokens   = int.TryParse(_config["GeminiAI:MaxOutputTokens"], out var mt) ? mt : 1024;
            var temperature = double.TryParse(_config["GeminiAI:Temperature"], NumberStyles.Float, CultureInfo.InvariantCulture, out var t) ? t : 0.4;
            var url         = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            var contents = new List<object>();
            foreach (var msg in history)
                contents.Add(new { role = msg.Role, parts = new[] { new { text = msg.Text } } });
            contents.Add(new { role = "user", parts = new[] { new { text = userMessage } } });

            var payload  = new { system_instruction = new { parts = new[] { new { text = systemPrompt } } }, contents, generationConfig = new { maxOutputTokens = maxTokens, temperature } };
            var httpClient   = _httpClientFactory.CreateClient();
            var response     = await httpClient.PostAsync(url, new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new Exception($"Gemini {response.StatusCode}: {responseBody}");
            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString()?.Trim() ?? string.Empty;
        }

        // ── Gọi OpenRouter ──────────────────────────────────────────────────
        private async Task<string> CallOpenRouterAsync(string apiKey, string systemPrompt, List<ChatMessage> history, string userMessage)
        {
            var maxTokens   = int.TryParse(_config["GeminiAI:MaxOutputTokens"], out var mt) ? mt : 1024;
            var temperature = double.TryParse(_config["GeminiAI:Temperature"], NumberStyles.Float, CultureInfo.InvariantCulture, out var t) ? t : 0.4;
            var model       = "qwen/qwen3-8b:free";  // free model hiện có

            var messages = new List<object> { new { role = "system", content = systemPrompt } };
            foreach (var msg in history)
                messages.Add(new { role = msg.Role == "model" ? "assistant" : "user", content = msg.Text });
            messages.Add(new { role = "user", content = userMessage });

            var payload  = new { model, messages, max_tokens = maxTokens, temperature };
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://kimton.vn");
            httpClient.DefaultRequestHeaders.Add("X-Title", "KimTon Chatbot");

            var response     = await httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new Exception($"OpenRouter {response.StatusCode}: {responseBody}");
            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()?.Trim() ?? string.Empty;
        }
    }
}
