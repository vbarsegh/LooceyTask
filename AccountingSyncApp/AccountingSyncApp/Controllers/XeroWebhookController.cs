using Application_Layer.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

[ApiController]
[Route("api/webhooks/xero")]
public class XeroWebhookController : ControllerBase
{
    private readonly AccountingSyncManager _syncManager;
    private readonly ILogger<XeroWebhookController> _logger;
    private readonly IConfiguration _config;

    public XeroWebhookController(AccountingSyncManager syncManager, ILogger<XeroWebhookController> logger, IConfiguration config)
    {
        _syncManager = syncManager;
        _logger = logger;
        _config = config;
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveWebhook()
    {
        // 1️⃣ Read the raw body from Xero
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();

        _logger.LogInformation("Received Xero webhook payload: {payload}", payload);

        // 2️⃣ Verify webhook signature (important for security)
        var webhookKey = _config["XeroSettings:WebhookKey"];
        var xeroSignature = Request.Headers["x-xero-signature"].ToString();
        var computedSignature = ComputeHmacSha256(payload, webhookKey);

        if (computedSignature != xeroSignature)
        {
            _logger.LogWarning("Invalid Xero webhook signature — ignoring request.");
            return Unauthorized();
        }

        // 3️⃣ This request might be the “intent to receive” test OR a real webhook
        if (string.IsNullOrWhiteSpace(payload))
        {
            _logger.LogInformation("Received 'Intent to receive' handshake from Xero ✅");
            return Ok();
        }

        // 4️⃣ For real webhook events → sync data
        await _syncManager.SyncFromXeroAsync();
        _logger.LogInformation("SyncFromXeroAsync executed successfully.");

        return Ok();
    }
    private string ComputeHmacSha256(string message, string secret)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return Convert.ToBase64String(hash);
    }
}
