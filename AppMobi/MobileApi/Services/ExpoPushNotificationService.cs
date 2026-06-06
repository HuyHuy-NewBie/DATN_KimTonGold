using System.Net.Http.Json;
using System.Text.Json.Serialization;
using MobileApi.Models;

namespace MobileApi.Services;

public record ExpoPushMessage(
    [property: JsonPropertyName("to")] string To,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("body")] string Body,
    [property: JsonPropertyName("sound")] string Sound,
    [property: JsonPropertyName("data")] object Data);

public class ExpoPushNotificationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExpoPushNotificationService> _logger;

    public ExpoPushNotificationService(HttpClient httpClient, ILogger<ExpoPushNotificationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task SendPendingOrderAsync(Order order, IEnumerable<MobileDeviceToken> deviceTokens, CancellationToken cancellationToken)
    {
        var messages = deviceTokens
            .Where(token => token.IsActive && !string.IsNullOrWhiteSpace(token.ExpoPushToken))
            .Select(token => new ExpoPushMessage(
                token.ExpoPushToken,
                "Đơn hàng chờ phê duyệt",
                $"#{order.OrderNumber} - {(order.CustomerName ?? "Khách hàng")} - {order.TotalAmount:N0} đ",
                "default",
                new
                {
                    type = "pending-order",
                    orderId = order.Id,
                    orderNumber = order.OrderNumber,
                    status = order.Status
                }))
            .ToList();

        if (messages.Count == 0)
        {
            return;
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync("https://exp.host/--/api/v2/push/send", messages, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Expo push returned {StatusCode}: {Body}", response.StatusCode, body);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Unable to send Expo push notifications for order {OrderId}.", order.Id);
        }
    }
}
