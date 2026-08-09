using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GoldManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace GoldManagementSystem.Properties.Services
{
    public class ChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatService> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly string _apiKey;
        private readonly string _model;

        public ChatService(HttpClient httpClient, IConfiguration configuration, ILogger<ChatService> logger, ApplicationDbContext dbContext)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

            _apiKey = _configuration["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            _model = _configuration["OpenAI:Model"] ?? "gpt-3.5-turbo";

            var baseUrl = _configuration["OpenAI:ApiBaseUrl"]?.TrimEnd('/') ?? "https://api.openai.com/v1";
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var apiUri))
            {
                throw new InvalidOperationException("OpenAI base URL is not a valid absolute URI.");
            }

            _httpClient.BaseAddress = apiUri;
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        //2//

        public async Task<string> GenerateReplyAsync(string userMessage, IEnumerable<ChatHistoryEntry> conversationHistory = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return "Xin vui lòng nhập câu hỏi để tôi có thể hỗ trợ bạn.";
            }

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                _logger.LogWarning("OpenAI API key is not configured. Falling back to rule-based support replies.");
                return GenerateFallbackReply(userMessage);
            }

            var systemPrompt = await BuildSystemPromptAsync(cancellationToken);
            var messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    Role = "system",
                    Content = systemPrompt
                }
            };

            if (conversationHistory != null)
            {
                foreach (var entry in conversationHistory.TakeLast(12))
                {
                    if (string.IsNullOrWhiteSpace(entry.Role) || string.IsNullOrWhiteSpace(entry.Content))
                    {
                        continue;
                    }

                    var role = entry.Role.Equals("user", StringComparison.OrdinalIgnoreCase)
                        ? "user"
                        : "assistant";

                    messages.Add(new ChatMessage
                    {
                        Role = role,
                        Content = entry.Content.Trim()
                    });
                }
            }

            messages.Add(new ChatMessage
            {
                Role = "user",
                Content = userMessage.Trim()
            });

            var requestBody = new
            {
                model = _model,
                messages,
                temperature = 0.3,
                max_tokens = 600,
                top_p = 1.0,
                frequency_penalty = 0.0,
                presence_penalty = 0.0
            };

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
                {
                    Content = JsonContent.Create(requestBody)
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("OpenAI request failed with status {StatusCode}: {ResponseText}", response.StatusCode, responseText);
                    return GenerateFallbackReply(userMessage);
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var openAiResponse = JsonSerializer.Deserialize<OpenAiChatResponse>(responseText, options);

                var content = openAiResponse?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();
                if (!string.IsNullOrWhiteSpace(content))
                {
                    return content;
                }

                _logger.LogWarning("OpenAI response did not contain a valid completion. Response body: {ResponseText}", responseText);
                return GenerateFallbackReply(userMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while calling OpenAI chat completion.");
                return GenerateFallbackReply(userMessage);
            }
        }

        private async Task<string> BuildSystemPromptAsync(CancellationToken cancellationToken)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Bạn là trợ lý khách hàng của KimTon Jewelry, trả lời bằng tiếng Việt một cách lịch sự, rõ ràng và chuyên nghiệp.");
            builder.AppendLine("Bạn biết các thông tin sau về cửa hàng và sản phẩm:");
            builder.AppendLine("- KimTon Jewelry cung cấp vàng 24K/9999, vàng 18K, vàng trắng, bạc, kim cương và moissanite.");
            builder.AppendLine("- Khách hàng có thể liên hệ qua Messenger, Zalo hoặc Hotline 0961137407.");
            builder.AppendLine("- Khi trả lời, nếu cần giá chính xác, hãy nhắc khách hàng cung cấp loại sản phẩm, mã sản phẩm, hoặc chi nhánh.");
            builder.AppendLine("- Nếu khách hỏi về đổi trả hoặc bảo hành, hãy ưu tiên hướng dẫn khách liên hệ trực tiếp nhân viên để kiểm tra chính sách cụ thể.");
            builder.AppendLine("- Nếu khách hỏi về hình thức thanh toán, hãy đề cập đến chuyển khoản ngân hàng, ví điện tử hoặc thanh toán trực tiếp tại cửa hàng.");
            builder.AppendLine("- Nếu khách hỏi về giao hàng, hãy trả lời thời gian 1-3 ngày trong nội thành và 3-5 ngày khi giao xa, kèm thông tin sẽ gửi mã vận đơn khi gửi hàng.");
            builder.AppendLine("- Nếu khách hỏi về vàng 24K và 9999, giải thích rằng 24K/9999 là vàng gần như nguyên chất, 18K phù hợp trang sức đeo hàng ngày, vàng trắng phù hợp phong cách hiện đại.");

            var topBranches = await _dbContext.Branches
                .Where(branch => branch.IsActive)
                .OrderBy(branch => branch.BranchName)
                .Take(3)
                .Select(branch => new
                {
                    branch.BranchName,
                    branch.Address,
                    branch.PhoneNumber
                })
                .ToListAsync(cancellationToken);

            if (topBranches.Any())
            {
                builder.AppendLine("- Các chi nhánh KimTon đang hoạt động:");
                foreach (var branch in topBranches)
                {
                    builder.AppendLine($"  + {branch.BranchName}: {branch.Address}. Hotline: {branch.PhoneNumber}.");
                }
            }

            var branchAdminData = await _dbContext.Branches
                .Where(branch => branch.IsActive)
                .OrderBy(branch => branch.BranchName)
                .Select(branch => new
                {
                    branch.BranchName,
                    branch.Address,
                    branch.PhoneNumber,
                    branch.ProductPriceInfo,
                    branch.SizeSelectionInfo,
                    branch.WarrantyInfo,
                    branch.TradeInPolicyInfo,
                    branch.OrderProcessInfo
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (branchAdminData != null)
            {
                builder.AppendLine("- Dữ liệu Chat AI của chi nhánh tham chiếu:");
                builder.AppendLine($"  + Tên chi nhánh: {branchAdminData.BranchName}.");
                builder.AppendLine($"  + Địa chỉ: {branchAdminData.Address}.");
                builder.AppendLine($"  + Hotline/Zalo: {branchAdminData.PhoneNumber}.");
                if (!string.IsNullOrWhiteSpace(branchAdminData.ProductPriceInfo))
                {
                    builder.AppendLine($"  + Giá sản phẩm: {branchAdminData.ProductPriceInfo}");
                }
                if (!string.IsNullOrWhiteSpace(branchAdminData.SizeSelectionInfo))
                {
                    builder.AppendLine($"  + Chọn size: {branchAdminData.SizeSelectionInfo}");
                }
                if (!string.IsNullOrWhiteSpace(branchAdminData.WarrantyInfo))
                {
                    builder.AppendLine($"  + Bảo hành: {branchAdminData.WarrantyInfo}");
                }
                if (!string.IsNullOrWhiteSpace(branchAdminData.TradeInPolicyInfo))
                {
                    builder.AppendLine($"  + Chính sách thu đổi: {branchAdminData.TradeInPolicyInfo}");
                }
                if (!string.IsNullOrWhiteSpace(branchAdminData.OrderProcessInfo))
                {
                    builder.AppendLine($"  + Đặt hàng: {branchAdminData.OrderProcessInfo}");
                }
            }

            builder.AppendLine("- Các nút gợi ý nhanh trên giao diện chat tương ứng với: Giá sản phẩm, Chọn size, Bảo hành, Đặt hàng, Chính sách thu đổi.");
            builder.AppendLine("  Nếu khách hỏi về một trong những mục này, ưu tiên trả lời bằng nội dung admin tương ứng nếu đã được cập nhật.");
            builder.AppendLine("  Nếu nội dung tương ứng chưa có, hãy trả lời một cách lịch sự, chuyên nghiệp với thông tin chung phù hợp và đề nghị khách cung cấp thêm chi tiết hoặc liên hệ Hotline/Zalo để được tư vấn cụ thể.");

            var sampleProducts = await _dbContext.Products
                .Include(product => product.Branch)
                .Where(product => product.Branch != null)
                .OrderByDescending(product => product.CreatedAt)
                .Take(5)
                .Select(product => new
                {
                    product.Name,
                    product.Category,
                    product.GoldType,
                    product.SellPrice,
                    BranchName = product.Branch.BranchName
                })
                .ToListAsync(cancellationToken);

            if (sampleProducts.Any())
            {
                builder.AppendLine("- Một số sản phẩm tiêu biểu đang có trên hệ thống:");
                foreach (var product in sampleProducts)
                {
                    builder.AppendLine($"  + {product.Name} ({product.Category}, {product.GoldType}) tại chi nhánh {product.BranchName} với giá tham khảo {product.SellPrice:N0} ₫.");
                }
            }

            builder.AppendLine("Luôn giữ câu trả lời ngắn gọn, tránh suy đoán nếu không có đủ thông tin, và khuyến khích khách hàng cung cấp thêm thông tin cụ thể khi cần.");

            return builder.ToString();
        }

        private string GenerateFallbackReply(string message)
        {
            var lower = message.ToLowerInvariant();

            if (lower.Contains("giá vàng") || lower.Contains("giá sản phẩm") || lower.Contains("bao nhiêu"))
            {
                return "Giá vàng hôm nay thay đổi theo loại và thương hiệu. Vui lòng cung cấp thêm thông tin sản phẩm để tôi kiểm tra giá chính xác, hoặc bạn có thể chọn loại vàng cụ thể như 24K, 9999, 18K. Nếu bạn muốn, tôi có thể hướng dẫn bạn xem giá vàng mới nhất ngay bây giờ.";
            }

            if ((lower.Contains("24k") && lower.Contains("9999")) || lower.Contains("24k hay vàng 9999") || lower.Contains("9999 hay 24k"))
            {
                return "Vàng 24K là vàng nguyên chất 99,9%, còn vàng 9999 là tên gọi khác của vàng 24K với độ tinh khiết 99,99%. Nếu bạn cần tích trữ giá trị dài hạn, vàng 24K/9999 là lựa chọn tốt hơn. Nếu mua trang sức, bạn có thể cân nhắc vàng 18K để bền đẹp hơn.";
            }

            if ((lower.Contains("18k") && lower.Contains("24k")) || lower.Contains("18k và vàng 24k") || lower.Contains("khác nhau"))
            {
                return "Vàng 18K có hàm lượng vàng là khoảng 75% và thường nhẹ nhàng, phù hợp làm trang sức bền đẹp. Vàng 24K (hoặc 9999) gần như nguyên chất, giá trị cao hơn nhưng mềm hơn và thường dùng để đầu tư hoặc làm sản phẩm giữ giá trị. Chọn 18K nếu bạn muốn trang sức dùng hàng ngày, chọn 24K nếu bạn muốn tích trữ hoặc đầu tư.";
            }

            if (lower.Contains("đầu tư") || lower.Contains("tích trữ") || lower.Contains("mua vàng để đầu tư") || lower.Contains("nên chọn loại nào"))
            {
                return "Đầu tư vàng nên ưu tiên loại vàng có tính thanh khoản và giá trị ổn định như vàng 24K/9999. Nếu bạn muốn vừa đầu tư vừa giữ làm quà tặng, có thể cân nhắc những sản phẩm trang sức vàng 24K hoặc 9999 có thiết kế đơn giản. Hãy chọn sản phẩm có giấy kiểm định rõ ràng và nguồn gốc uy tín.";
            }

            if (lower.Contains("vàng trắng") || lower.Contains("vang trang"))
            {
                return "Vàng trắng thường là hợp kim vàng pha với kim loại trắng như palladium hoặc bạc để có màu sáng, dùng phổ biến trong trang sức. Nó có vẻ đẹp hiện đại và dễ phối trang phục. Nếu bạn cần giữ giá trị, vàng trắng có thể không cao bằng vàng 24K nhưng vẫn là lựa chọn trang sức sang trọng.";
            }

            if (lower.Contains("khuyến mãi") || lower.Contains("ưu đãi") || lower.Contains("khuyen mai"))
            {
                return "Cửa hàng thường có các chương trình khuyến mãi theo thời điểm như giảm giá sản phẩm, tặng quà, hoặc hỗ trợ trả góp. Bạn có thể liên hệ nhân viên để biết chương trình khuyến mãi hiện tại và nhận ưu đãi tốt nhất.";
            }

            if (lower.Contains("đổi trả") || lower.Contains("doi tra") || lower.Contains("chính sách đổi trả"))
            {
                return "Chính sách đổi trả của chúng tôi thường cho phép đổi trả trong vòng 7-14 ngày nếu sản phẩm còn nguyên vẹn, kèm hóa đơn và giấy tờ kiểm định. Vui lòng liên hệ trực tiếp nhân viên để xác nhận điều kiện cụ thể cho sản phẩm bạn đã mua.";
            }

            if (lower.Contains("thanh toán") || lower.Contains("phương thức") || lower.Contains("thanhtoan"))
            {
                return "Bạn có thể thanh toán bằng chuyển khoản ngân hàng, ví điện tử hoặc thanh toán trực tiếp tại cửa hàng. Nếu mua online, chúng tôi sẽ gửi hướng dẫn thanh toán chi tiết sau khi xác nhận đơn hàng.";
            }

            if (lower.Contains("giao hàng") || lower.Contains("giao hang") || lower.Contains("thời gian giao"))
            {
                return "Thời gian giao hàng thông thường là 1-3 ngày làm việc trong khu vực nội thành. Với đơn hàng xa, thời gian có thể kéo dài 3-5 ngày. Chúng tôi sẽ cung cấp mã vận đơn khi đơn hàng được gửi.";
            }

            if (lower.Contains("kiểm tra") && lower.Contains("chính hãng") || lower.Contains("xác thực vàng") || lower.Contains("vàng chính hãng"))
            {
                return "Để kiểm tra vàng chính hãng, bạn nên yêu cầu giấy tờ kiểm định, tem nhãn thương hiệu và kiểm tra dấu bảo hành. Ngoài ra, bạn có thể đến cửa hàng để nhân viên sử dụng thiết bị đo chuyên dụng xác nhận độ tinh khiết. Mua từ thương hiệu uy tín giúp bạn yên tâm hơn.";
            }

            if (lower.Contains("24k") || lower.Contains("9999") || lower.Contains("18k") || lower.Contains("vàng") || lower.Contains("vàng trắng"))
            {
                return "Vàng có nhiều loại khác nhau để phục vụ nhu cầu khác nhau: 24K/9999 cho đầu tư và tích trữ, 18K cho trang sức đeo hàng ngày, vàng trắng cho phong cách hiện đại. Nếu bạn cần tư vấn chi tiết hơn, vui lòng cho biết mục đích mua vàng của bạn.";
            }

            return "Xin lỗi, tôi chưa có câu trả lời chính xác cho câu hỏi này. Bạn có thể liên hệ nhân viên tư vấn để được hỗ trợ nhanh chóng và chính xác hơn.";
        }

        public class ChatHistoryEntry
        {
            public string Role { get; set; }
            public string Content { get; set; }
        }

        private class ChatMessage
        {
            public string Role { get; set; }
            public string Content { get; set; }
        }

        private class OpenAiChatResponse
        {
            public List<OpenAiChoice> Choices { get; set; }
        }

        private class OpenAiChoice
        {
            public ChatMessage Message { get; set; }
        }
    }
}
