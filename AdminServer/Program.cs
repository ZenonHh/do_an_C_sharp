using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();
app.UseCors();
app.UseStaticFiles();

// ── Database ────────────────────────────────────────────────────────────────
var DbPath = Environment.GetEnvironmentVariable("DB_PATH") ?? "admin.db";

void InitDb()
{
    using var conn = new SqliteConnection($"Data Source={DbPath}");
    conn.Open();
    var cmd = conn.CreateCommand();
    cmd.CommandText = """
        CREATE TABLE IF NOT EXISTS PlayHistory (
            Id       INTEGER PRIMARY KEY AUTOINCREMENT,
            UserId   TEXT NOT NULL DEFAULT 'guest',
            PoiName  TEXT NOT NULL,
            PlayedAt TEXT NOT NULL,
            DeviceId TEXT
        );
        CREATE TABLE IF NOT EXISTS UserSession (
            DeviceId TEXT PRIMARY KEY,
            UserId   TEXT NOT NULL DEFAULT 'guest',
            LastSeen TEXT NOT NULL
        );
        """;
    cmd.ExecuteNonQuery();
}

InitDb();

SqliteConnection OpenDb()
{
    var conn = new SqliteConnection($"Data Source={DbPath}");
    conn.Open();
    return conn;
}

// ── Sync endpoints (MAUI app gọi) ──────────────────────────────────────────
app.MapPost("/api/sync/history", (SyncHistoryRequest req) =>
{
    using var conn = OpenDb();
    var cmd = conn.CreateCommand();
    cmd.CommandText = """
        INSERT INTO PlayHistory (UserId, PoiName, PlayedAt, DeviceId)
        VALUES (@u, @p, @t, @d)
        """;
    cmd.Parameters.AddWithValue("@u", req.UserId ?? "guest");
    cmd.Parameters.AddWithValue("@p", req.PoiName);
    cmd.Parameters.AddWithValue("@t", req.PlayedAt);
    cmd.Parameters.AddWithValue("@d", (object?)req.DeviceId ?? DBNull.Value);
    cmd.ExecuteNonQuery();
    return Results.Ok(new { success = true });
});

app.MapPost("/api/sync/heartbeat", (HeartbeatRequest req) =>
{
    using var conn = OpenDb();
    var cmd = conn.CreateCommand();
    cmd.CommandText = """
        INSERT INTO UserSession (DeviceId, UserId, LastSeen)
        VALUES (@d, @u, @t)
        ON CONFLICT(DeviceId) DO UPDATE SET UserId = @u, LastSeen = @t
        """;
    cmd.Parameters.AddWithValue("@d", req.DeviceId);
    cmd.Parameters.AddWithValue("@u", req.UserId ?? "guest");
    cmd.Parameters.AddWithValue("@t", DateTime.UtcNow.ToString("o"));
    cmd.ExecuteNonQuery();
    return Results.Ok(new { success = true });
});

// ── Stats endpoints (dashboard gọi) ────────────────────────────────────────
app.MapGet("/api/stats/online", () =>
{
    using var conn = OpenDb();
    var threshold = DateTime.UtcNow.AddSeconds(-90).ToString("o");

    var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT COUNT(*) FROM UserSession WHERE LastSeen > @t";
    cmd.Parameters.AddWithValue("@t", threshold);
    var online = (long)(cmd.ExecuteScalar() ?? 0L);

    cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT COUNT(*) FROM UserSession";
    var total = (long)(cmd.ExecuteScalar() ?? 0L);

    return Results.Ok(new { online, offline = total - online, total });
});

app.MapGet("/api/stats/summary", () =>
{
    using var conn = OpenDb();
    var today = DateTime.Now.ToString("yyyy-MM-dd");
    var weekStart = DateTime.Now.AddDays(-6).Date;

    // Fetch all histories and process in C# to handle timezone-aware ISO strings correctly
    var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT PlayedAt, UserId FROM PlayHistory";
    using var reader = cmd.ExecuteReader();

    long todayCount = 0, weekCount = 0, totalCount = 0;
    var userSet = new HashSet<string>();

    while (reader.Read())
    {
        var raw = reader.GetString(0);
        var uid = reader.GetString(1);
        totalCount++;
        userSet.Add(uid);
        if (DateTime.TryParse(raw, out var dt))
        {
            if (dt.Date == DateTime.Now.Date) todayCount++;
            if (dt.Date >= weekStart) weekCount++;
        }
    }

    return Results.Ok(new { todayCount, weekCount, totalCount, userCount = userSet.Count });
});

app.MapGet("/api/stats/peak-hours", () =>
{
    using var conn = OpenDb();
    var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT PlayedAt FROM PlayHistory";
    using var reader = cmd.ExecuteReader();

    var hours = new int[24];
    while (reader.Read())
    {
        if (DateTime.TryParse(reader.GetString(0), out var dt))
            hours[dt.Hour]++;
    }
    return Results.Ok(hours);
});

app.MapGet("/api/stats/weekly", () =>
{
    using var conn = OpenDb();
    var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT PlayedAt FROM PlayHistory";
    using var reader = cmd.ExecuteReader();

    var counts = new Dictionary<string, int>();
    while (reader.Read())
    {
        if (DateTime.TryParse(reader.GetString(0), out var dt))
        {
            var key = dt.ToString("yyyy-MM-dd");
            counts[key] = counts.TryGetValue(key, out var c) ? c + 1 : 1;
        }
    }

    var result = Enumerable.Range(0, 7)
        .Select(i => DateTime.Now.AddDays(i - 6).ToString("yyyy-MM-dd"))
        .Select(d => new { date = d, count = counts.TryGetValue(d, out var c) ? c : 0 })
        .ToList();
    return Results.Ok(result);
});

app.MapGet("/api/stats/top-users", () =>
{
    using var conn = OpenDb();
    var cmd = conn.CreateCommand();
    cmd.CommandText = """
        SELECT UserId, COUNT(*) as Count
        FROM PlayHistory
        GROUP BY UserId
        ORDER BY Count DESC
        LIMIT 10
        """;
    using var reader = cmd.ExecuteReader();
    var result = new List<object>();
    while (reader.Read())
        result.Add(new { userId = reader.GetString(0), count = reader.GetInt32(1) });
    return Results.Ok(result);
});

app.MapGet("/api/stats/top-pois", () =>
{
    using var conn = OpenDb();
    var cmd = conn.CreateCommand();
    cmd.CommandText = """
        SELECT PoiName, COUNT(*) as Count
        FROM PlayHistory
        GROUP BY PoiName
        ORDER BY Count DESC
        LIMIT 10
        """;
    using var reader = cmd.ExecuteReader();
    var result = new List<object>();
    while (reader.Read())
        result.Add(new { poiName = reader.GetString(0), count = reader.GetInt32(1) });
    return Results.Ok(result);
});

// ── QR Landing page ─────────────────────────────────────────────────────────
app.MapGet("/poi/{poiName}", (string poiName) =>
{
    var decoded = Uri.UnescapeDataString(poiName);
    var appLink = $"vinhkhanhtour://play_audio?poi_name={Uri.EscapeDataString(decoded)}";
    var html = $$$"""
        <!DOCTYPE html>
        <html lang="vi">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>{{{decoded}}} — Vinh Khánh Tour</title>
            <style>
                *{margin:0;padding:0;box-sizing:border-box}
                body{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;
                     background:linear-gradient(135deg,#ff6b35,#f7931e);
                     min-height:100vh;display:flex;align-items:center;
                     justify-content:center;padding:20px}
                .card{background:#fff;border-radius:20px;padding:40px 28px;
                      max-width:400px;width:100%;text-align:center;
                      box-shadow:0 20px 60px rgba(0,0,0,.2)}
                .logo{font-size:56px;margin-bottom:8px}
                h1{color:#ff6b35;font-size:22px;margin-bottom:4px}
                .sub{color:#999;font-size:13px;margin-bottom:28px}
                .poi-box{background:#fff8f0;border:2px solid #ff6b35;
                         border-radius:12px;padding:16px;margin-bottom:28px}
                .poi-box h2{color:#333;font-size:18px}
                .poi-box p{color:#ff6b35;font-size:12px;margin-top:4px}
                #status{color:#666;font-size:15px;margin-bottom:22px;min-height:22px}
                .spin{display:inline-block;width:18px;height:18px;
                      border:3px solid #ddd;border-top-color:#ff6b35;
                      border-radius:50%;animation:sp .8s linear infinite;
                      vertical-align:middle;margin-right:6px}
                @keyframes sp{to{transform:rotate(360deg)}}
                .btn{display:block;width:100%;padding:14px;border-radius:12px;
                     font-size:15px;font-weight:600;text-decoration:none;
                     border:none;cursor:pointer;margin-bottom:10px;
                     transition:opacity .15s}
                .btn:active{opacity:.85}
                .btn-orange{background:#ff6b35;color:#fff}
                .btn-dark{background:#111;color:#fff}
                .btn-light{background:#f0f0f0;color:#333}
                .or{color:#ccc;font-size:12px;margin:8px 0}
                #fallback{display:none}
            </style>
        </head>
        <body>
          <div class="card">
            <div class="logo">🦪</div>
            <h1>Vinh Khánh Tour</h1>
            <p class="sub">Khám phá ẩm thực đường Vĩnh Khánh</p>
            <div class="poi-box">
              <h2>{{{decoded}}}</h2>
              <p>🎧 Nhấn để nghe thuyết minh âm thanh</p>
            </div>
            <p id="status"><span class="spin"></span>Đang mở ứng dụng...</p>
            <div id="fallback">
              <button class="btn btn-orange" onclick="tryOpen()">Mở ứng dụng ngay</button>
              <p class="or">— Chưa có ứng dụng? —</p>
              <a href="https://play.google.com/store/apps/details?id=com.vinhkhanh.tour"
                 class="btn btn-dark" target="_blank">⬇ Tải trên Google Play</a>
              <a href="https://apps.apple.com/app/id000000000"
                 class="btn btn-light" target="_blank">⬇ Tải trên App Store</a>
            </div>
          </div>
          <script>
            var appLink = "{{{appLink}}}";
            function tryOpen(){ window.location.href = appLink; }
            window.onload = function(){
              tryOpen();
              setTimeout(function(){
                document.getElementById('status').textContent = 'Không tìm thấy ứng dụng trên thiết bị này.';
                document.getElementById('fallback').style.display = 'block';
              }, 2500);
            };
          </script>
        </body>
        </html>
        """;
    return Results.Content(html, "text/html; charset=utf-8");
});

app.MapGet("/", () => Results.Redirect("/dashboard.html"));

// Railway inject PORT qua biến môi trường, local dùng 5000
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");

// ── Request models (phải đặt sau tất cả top-level statements) ───────────────
record SyncHistoryRequest(string UserId, string PoiName, string PlayedAt, string? DeviceId);
record HeartbeatRequest(string DeviceId, string UserId);
