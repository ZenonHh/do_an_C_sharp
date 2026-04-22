using DoAnCSharp.AdminWeb.Services;
using DoAnCSharp.AdminWeb.Models;
using Microsoft.AspNetCore.Http.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddScoped<DatabaseService>();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ✅ FIX: Bind to all network interfaces (not just localhost)
app.Urls.Clear();
app.Urls.Add("http://0.0.0.0:5000");      // Listen on all IPv4 interfaces
app.Urls.Add("http://[::]:5000");         // Listen on all IPv6 interfaces

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
    await dbService.InitAsync();
    // Seed sample data (POIs, users, etc.) for development
    await dbService.SeedSampleDataAsync();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

// Serve static files (HTML, CSS, JS)
app.UseStaticFiles();

// ===== TEST ENDPOINT (KHÔNG CẦN DATABASE) =====
app.MapGet("/test-qr", (HttpContext context) =>
{
    return Results.Redirect("/poi-public.html?poiId=1&test=true");
});

// Map API controllers (includes QRScansController with /qr/{code} endpoint)
app.MapControllers();

// ===== QR SCAN LANDING ENDPOINT =====
// Entry point when user scans QR code
// Tracks device, enforces 5 scans/day limit, redirects to restaurant info
app.MapGet("/qr-scan", async (
    HttpContext context,
    string? code,
    DatabaseService db,
    ILogger<Program> logger) =>
{
    if (string.IsNullOrEmpty(code))
    {
        return Results.Redirect("/poi-public.html?error=invalid_qr");
    }

    try
    {
        // Get device info
        var deviceId = GetOrCreateDeviceId(context);
        var deviceInfo = GetDeviceInfo(context);
        var ipAddress = GetClientIP(context);
        var userAgent = context.Request.Headers["User-Agent"].ToString();

        logger.LogInformation($"QR Scan: Code={code}, Device={deviceId}, IP={ipAddress}");

        // Look up POI by QR code
        var poi = await db.GetPOIByQRCodeAsync(code);
        if (poi == null)
        {
            logger.LogWarning($"Invalid QR code scanned: {code}");
            return Results.Redirect("/poi-public.html?error=not_found");
        }

        // Check device scan limit (5 scans/day for free users)
        var deviceLimit = await db.GetDeviceScanLimitAsync(deviceId);
        bool canScan = true;
        string limitMessage = "";

        if (deviceLimit == null)
        {
            // First time device - create limit record
            deviceLimit = new DeviceScanLimit
            {
                DeviceId = deviceId,
                ScanCount = 0,
                MaxScans = 5, // Free limit
                LastResetDate = DateTime.UtcNow.Date,
                CreatedAt = DateTime.UtcNow
            };
            await db.SaveDeviceScanLimitAsync(deviceLimit);
        }
        else
        {
            // Reset count if new day
            if (deviceLimit.LastResetDate < DateTime.UtcNow.Date)
            {
                deviceLimit.ScanCount = 0;
                deviceLimit.LastResetDate = DateTime.UtcNow.Date;
                await db.SaveDeviceScanLimitAsync(deviceLimit);
            }

            // Check if limit reached
            if (deviceLimit.ScanCount >= deviceLimit.MaxScans)
            {
                canScan = false;
                limitMessage = $"Bạn đã hết lượt quét miễn phí hôm nay ({deviceLimit.MaxScans} lần/ngày). Tải app để không giới hạn!";
                logger.LogWarning($"Device {deviceId} reached scan limit: {deviceLimit.ScanCount}/{deviceLimit.MaxScans}");
            }
        }

        if (canScan)
        {
            // Increment scan count
            deviceLimit.ScanCount++;
            await db.SaveDeviceScanLimitAsync(deviceLimit);

            // Record scan request for analytics
            var scanRequest = new QRScanRequest
            {
                UserId = 0, // Anonymous scan
                RestaurantId = poi.Id,
                RestaurantName = poi.Name,
                ScanTime = DateTime.UtcNow,
                DeviceInfo = deviceInfo,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Status = "success",
                Message = $"Scan successful. Remaining today: {deviceLimit.MaxScans - deviceLimit.ScanCount}"
            };
            await db.InsertQRScanRequestAsync(scanRequest);

            logger.LogInformation($"Scan successful: POI={poi.Name}, Device={deviceId}, Count={deviceLimit.ScanCount}/{deviceLimit.MaxScans}");

            // Check if mobile app is installed (deep link support)
            var deepLink = $"vinhkhanhtour://poi/{poi.Id}";
            var webLink = $"/poi-public.html?qr={code}&poi={poi.Id}&scans={deviceLimit.ScanCount}&max={deviceLimit.MaxScans}";

            // Return HTML that tries deep link first, fallback to web
            return Results.Content($@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Đang tải...</title>
    <style>
        body {{
            margin: 0;
            padding: 20px;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
        }}
        .loader {{
            text-align: center;
            background: white;
            padding: 40px;
            border-radius: 12px;
            box-shadow: 0 8px 32px rgba(0,0,0,0.2);
        }}
        .spinner {{
            width: 50px;
            height: 50px;
            border: 4px solid #e0e0e0;
            border-top: 4px solid #667eea;
            border-radius: 50%;
            animation: spin 1s linear infinite;
            margin: 0 auto 20px;
        }}
        @keyframes spin {{
            to {{ transform: rotate(360deg); }}
        }}
        h2 {{ color: #2c3e50; margin-bottom: 10px; }}
        p {{ color: #666; font-size: 14px; }}
    </style>
    <script>
        // Try to open app via deep link
        window.location.href = '{deepLink}';

        // Fallback to web page after 2 seconds if app not installed
        setTimeout(function() {{
            window.location.href = '{webLink}';
        }}, 2000);
    </script>
</head>
<body>
    <div class='loader'>
        <div class='spinner'></div>
        <h2>🍴 Vĩnh Khánh Tour</h2>
        <p>Đang mở thông tin quán <strong>{poi.Name}</strong>...</p>
        <p style='font-size: 12px; margin-top: 15px;'>Lượt quét hôm nay: {deviceLimit.ScanCount}/{deviceLimit.MaxScans}</p>
    </div>
</body>
</html>", "text/html");
        }
        else
        {
            // Limit reached - show upgrade message
            return Results.Redirect($"/poi-public.html?qr={code}&poi={poi.Id}&limit_reached=true&message={Uri.EscapeDataString(limitMessage)}");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, $"Error processing QR scan: {code}");
        return Results.Redirect("/poi-public.html?error=server_error");
    }
});

// Helper functions
string GetOrCreateDeviceId(HttpContext context)
{
    const string cookieName = "vkt_device_id";

    if (context.Request.Cookies.TryGetValue(cookieName, out var deviceId) && !string.IsNullOrEmpty(deviceId))
    {
        return deviceId;
    }

    // Generate new device ID
    deviceId = Guid.NewGuid().ToString("N");

    context.Response.Cookies.Append(cookieName, deviceId, new CookieOptions
    {
        Expires = DateTimeOffset.UtcNow.AddYears(10),
        HttpOnly = true,
        Secure = context.Request.IsHttps,
        SameSite = SameSiteMode.Lax
    });

    return deviceId;
}

string GetDeviceInfo(HttpContext context)
{
    var userAgent = context.Request.Headers["User-Agent"].ToString();

    // Simple device detection
    if (userAgent.Contains("iPhone") || userAgent.Contains("iPad"))
        return "iOS Device";
    if (userAgent.Contains("Android"))
        return "Android Device";
    if (userAgent.Contains("Windows"))
        return "Windows PC";
    if (userAgent.Contains("Mac"))
        return "Mac";

    return "Unknown Device";
}

string GetClientIP(HttpContext context)
{
    // Check for forwarded IP first (if behind proxy)
    var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
    if (!string.IsNullOrEmpty(forwardedFor))
    {
        return forwardedFor.Split(',')[0].Trim();
    }

    return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
}

// Fallback to index.html for SPA (MUST be last)
app.MapFallbackToFile("index.html");

app.Run();
