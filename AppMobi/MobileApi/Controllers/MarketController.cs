using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MobileApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarketController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly string _webBaseUrl;

    public MarketController(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _webBaseUrl = configuration["ExternalApis:WebProjectBaseUrl"] ?? "http://localhost:5240";
    }

    [HttpGet("prices")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPrices()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_webBaseUrl}/Home/MarketPrices");
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Không thể lấy dữ liệu từ hệ thống Web.");
            }

            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
        }
    }
}
