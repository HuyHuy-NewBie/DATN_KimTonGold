using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GoldManagementSystem.Data;
using GoldManagementSystem.Models;
using GoldManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GoldManagementSystem.Controllers;

/// <summary>
/// Receives only server-to-server payment-provider callbacks. A customer-facing
/// browser never has an endpoint that can confirm an online payment.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/payments")]
public sealed class PaymentWebhookController : ControllerBase
{
    private const int DefaultMaximumBodyBytes = 16 * 1024;
    private readonly ApplicationDbContext _context;
    private readonly OnlinePaymentWebhookOptions _options;
    private readonly ILogger<PaymentWebhookController> _logger;

    public PaymentWebhookController(
        ApplicationDbContext context,
        IOptions<OnlinePaymentWebhookOptions> options,
        ILogger<PaymentWebhookController> logger)
    {
        _context = context;
        _options = options.Value ?? new OnlinePaymentWebhookOptions();
        _logger = logger;
    }

    /// <remarks>
    /// Contract: the provider sends an UTF-8 JSON payload containing provider,
    /// eventId, orderId, transactionReference, amount and status. It signs the exact
    /// raw request bytes using HMAC-SHA256 and sends its lowercase hexadecimal digest
    /// in the configured signature header. Only status "Succeeded" changes an order.
    /// </remarks>
    [HttpPost("webhook")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> ReceiveAsync(CancellationToken cancellationToken)
    {
        if (!_options.IsConfigured)
        {
            _logger.LogCritical("Online payment webhook was called while its provider or signing secret is not configured.");
            return NotFound();
        }

        if (!Request.HasJsonContentType())
        {
            return StatusCode(StatusCodes.Status415UnsupportedMediaType, new { error = "JSON body is required." });
        }

        var maximumBodyBytes = Math.Clamp(_options.MaxRequestBodyBytes, 1, 1024 * 1024);
        if (Request.ContentLength is > 0 && Request.ContentLength > maximumBodyBytes)
        {
            return BadRequest(new { error = "Webhook body is too large." });
        }

        var body = await ReadBodyAsync(maximumBodyBytes, cancellationToken);
        if (body == null)
        {
            return BadRequest(new { error = "Webhook body is too large." });
        }

        if (!Request.Headers.TryGetValue(_options.SignatureHeaderName, out var providedSignature)
            || providedSignature.Count != 1
            || !HasValidSignature(body, providedSignature[0], _options.SigningSecret))
        {
            _logger.LogWarning("Rejected online payment webhook with an invalid signature.");
            return Unauthorized(new { error = "Invalid webhook signature." });
        }

        PaymentWebhookNotification notification;
        try
        {
            notification = JsonSerializer.Deserialize<PaymentWebhookNotification>(body, JsonOptions);
        }
        catch (JsonException)
        {
            return BadRequest(new { error = "Webhook JSON is invalid." });
        }

        if (!IsValidNotification(notification))
        {
            return BadRequest(new { error = "Webhook payload is incomplete or invalid." });
        }

        notification.Provider = notification.Provider.Trim();
        notification.EventId = notification.EventId.Trim();
        notification.TransactionReference = notification.TransactionReference.Trim();
        notification.Status = notification.Status.Trim();

        if (!string.Equals(notification.Provider, _options.Provider.Trim(), StringComparison.Ordinal))
        {
            _logger.LogWarning("Rejected signed webhook because provider {Provider} does not match the configured provider.", notification.Provider);
            return Unauthorized(new { error = "Provider is not accepted." });
        }

        // A signed non-success result is acknowledged but can never advance payment state.
        if (!string.Equals(notification.Status, "Succeeded", StringComparison.OrdinalIgnoreCase))
        {
            return Ok(new { received = true, processed = false });
        }

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);

            // TransactionReference is a database-enforced unique idempotency key. Serializable
            // isolation additionally prevents two distinct callbacks from confirming one order.
            var existingPayment = await _context.Payments
                .Include(item => item.Allocations)
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.TransactionReference == notification.TransactionReference, cancellationToken);
            if (existingPayment != null)
            {
                await transaction.CommitAsync(cancellationToken);
                var isExactDuplicate = IsSamePayment(existingPayment, notification);
                return isExactDuplicate
                    ? Ok(new { received = true, processed = false, duplicate = true })
                    : Conflict(new { error = "Transaction reference is already associated with another payment." });
            }

            var order = await _context.Orders.FirstOrDefaultAsync(item => item.Id == notification.OrderId, cancellationToken);
            if (order == null)
            {
                await transaction.CommitAsync(cancellationToken);
                return NotFound(new { error = "Order was not found." });
            }

            if (!IsOnlinePayment(order) || order.Status != Order.StatusAwaitingDepositPayment)
            {
                await transaction.CommitAsync(cancellationToken);
                return Ok(new { received = true, processed = false, ignored = true });
            }

            if (order.DepositDueAt.HasValue && order.DepositDueAt <= DateTime.UtcNow)
            {
                await transaction.CommitAsync(cancellationToken);
                _logger.LogWarning("Ignored signed payment callback for expired order {OrderId}.", order.Id);
                return Ok(new { received = true, processed = false, ignored = true });
            }

            if (notification.Amount != order.DepositAmount)
            {
                await transaction.CommitAsync(cancellationToken);
                _logger.LogWarning("Rejected signed payment callback with wrong amount for order {OrderId}. Expected {Expected}, received {Actual}.", order.Id, order.DepositAmount, notification.Amount);
                return Conflict(new { error = "Payment amount does not match the order." });
            }

            var now = DateTime.UtcNow;
            var payment = new Payment
            {
                PaymentNumber = $"PAY-{order.OrderNumber}-{now:yyyyMMddHHmmss}",
                BranchId = order.BranchId,
                Channel = PaymentChannelOptions.QR,
                Status = PaymentStatusOptions.Confirmed,
                Amount = order.DepositAmount,
                TransactionReference = notification.TransactionReference,
                Provider = notification.Provider,
                // This is the payer's account for the payment record; confirmation is evidenced by Provider + receipt, never by this user.
                CreatedByUserId = order.UserId,
                CreatedAt = now,
                ConfirmedAt = now
            };
            payment.Allocations.Add(new PaymentAllocation { OrderId = order.Id, Amount = order.DepositAmount, AllocatedAt = now });

            _context.Payments.Add(payment);

            if (!await _context.EInvoices.AnyAsync(item => item.OrderId == order.Id, cancellationToken))
            {
                _context.EInvoices.Add(new EInvoice
                {
                    OrderId = order.Id,
                    InvoiceNumber = $"INV-{order.OrderNumber}",
                    Status = EInvoiceStatusOptions.Pending,
                    CreatedByUserId = order.UserId
                });
            }

            order.Status = Order.StatusPendingConfirmation;
            order.DepositPaidAt = now;
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Confirmed online payment for order {OrderId} from provider {Provider}, transaction {TransactionReference}.", order.Id, notification.Provider, notification.TransactionReference);
            return Ok(new { received = true, processed = true });
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            _context.ChangeTracker.Clear();
            var existingPayment = await _context.Payments
                .Include(item => item.Allocations)
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.TransactionReference == notification.TransactionReference, cancellationToken);

            if (existingPayment != null && IsSamePayment(existingPayment, notification))
            {
                return Ok(new { received = true, processed = false, duplicate = true });
            }

            _logger.LogError(exception, "Online payment callback collided with an existing payment receipt.");
            return Conflict(new { error = "A different successful callback has already been recorded." });
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private static bool IsOnlinePayment(Order order) =>
        order.PaymentMethod == Order.PaymentMethodOnlineDeposit
        || order.PaymentMethod == Order.PaymentMethodOnlineFull;

    private static bool IsValidNotification(PaymentWebhookNotification notification) =>
        notification != null
        && !string.IsNullOrWhiteSpace(notification.Provider) && notification.Provider.Trim().Length <= 100
        && !string.IsNullOrWhiteSpace(notification.EventId) && notification.EventId.Trim().Length <= 100
        && notification.OrderId > 0
        && !string.IsNullOrWhiteSpace(notification.TransactionReference) && notification.TransactionReference.Trim().Length <= 100
        && notification.Amount > 0m
        && !string.IsNullOrWhiteSpace(notification.Status) && notification.Status.Trim().Length <= 30;

    private static bool IsSamePayment(Payment payment, PaymentWebhookNotification notification) =>
        payment.Provider == notification.Provider
        && payment.Amount == notification.Amount
        && payment.Allocations.Any(allocation => allocation.OrderId == notification.OrderId);

    private static bool HasValidSignature(byte[] body, string signatureHeader, string signingSecret)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader)) return false;

        var supplied = signatureHeader.Trim();
        if (supplied.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase)) supplied = supplied[7..];

        byte[] actualSignature;
        try
        {
            actualSignature = Convert.FromHexString(supplied);
        }
        catch (FormatException)
        {
            return false;
        }

        var expectedSignature = HMACSHA256.HashData(Encoding.UTF8.GetBytes(signingSecret), body);
        return CryptographicOperations.FixedTimeEquals(expectedSignature, actualSignature);
    }

    private async Task<byte[]> ReadBodyAsync(int maximumBodyBytes, CancellationToken cancellationToken)
    {
        await using var buffer = new MemoryStream(Math.Min(maximumBodyBytes, DefaultMaximumBodyBytes));
        var readBuffer = new byte[Math.Min(4096, maximumBodyBytes)];
        int bytesRead;
        while ((bytesRead = await Request.Body.ReadAsync(readBuffer.AsMemory(0, readBuffer.Length), cancellationToken)) > 0)
        {
            if (buffer.Length + bytesRead > maximumBodyBytes) return null;
            await buffer.WriteAsync(readBuffer.AsMemory(0, bytesRead), cancellationToken);
        }

        return buffer.ToArray();
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };
}
