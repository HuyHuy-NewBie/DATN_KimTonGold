using GoldManagementSystem.Properties.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;


namespace GoldManagementSystem.Controllers
{
    [Route("chat")] 
    public class ChatController : Controller
    {
        private readonly ChatService _chatService;

        public class ChatRequest
        {
            public string Message { get; set; }
        }

        public class ChatResponse
        {
            public string Reply { get; set; }
        }
//1//
        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return Json(new ChatResponse { Reply = "Xin vui lòng nhập câu hỏi để tôi có thể hỗ trợ bạn." });
            }

            var message = request.Message.Trim();
            var reply = await _chatService.GenerateReplyAsync(message);
            return Json(new ChatResponse { Reply = reply });
        }

    }
}
