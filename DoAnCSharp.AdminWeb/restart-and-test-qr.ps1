# ===== RESTART SERVER WITH QR CODE FIX =====
Write-Host "🔄 Đang restart server với sửa QR code..." -ForegroundColor Cyan

# 1. Kill existing dotnet processes
Write-Host "1️⃣ Dừng các process dotnet cũ..."
Stop-Process -Name dotnet -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2
Write-Host "   ✅ Đã dừng" -ForegroundColor Green

# 2. Delete old database to reseed with full URLs
Write-Host "2️⃣ Xóa database cũ để tạo lại với full URLs..."
$dbPath = "$env:APPDATA\VinhKhanhTour\VinhKhanhTour_Full.db3"
if (Test-Path $dbPath) {
    Remove-Item $dbPath -Force
    Write-Host "   ✅ Database đã xóa: $dbPath" -ForegroundColor Green
} else {
    Write-Host "   ℹ️ Database chưa tồn tại" -ForegroundColor Yellow
}

# 3. Clean build
Write-Host "3️⃣ Clean build project..."
cd "C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb"
dotnet clean | Out-Null
dotnet build | Out-Null
Write-Host "   ✅ Build thành công" -ForegroundColor Green

# 4. Start server
Write-Host "4️⃣ Khởi động server..."
Write-Host "   Chờ 3 giây để server khởi động..." -ForegroundColor Yellow
Start-Process -FilePath dotnet -ArgumentList "run" -NoNewWindow
Start-Sleep -Seconds 3

# 5. Test database seeding
Write-Host "5️⃣ Kiểm tra database đã được seed..." -ForegroundColor Cyan
$maxRetries = 5
$retries = 0
$success = $false

while ($retries -lt $maxRetries) {
    try {
        $response = Invoke-WebRequest -Uri "http://172.20.10.2:5000/api/pois/debug/all" -ErrorAction Stop
        $data = $response.Content | ConvertFrom-Json
        
        Write-Host "   ✅ Server đang chạy, POIs được seed:" -ForegroundColor Green
        Write-Host "      Tổng số quán ăn: $($data.totalCount)" -ForegroundColor Green
        
        $data.pois | ForEach-Object {
            Write-Host "      📍 $($_.name)" -ForegroundColor Cyan
            Write-Host "         QR Code: $($_.qrCode)" -ForegroundColor Yellow
        }
        
        $success = $true
        break
    }
    catch {
        $retries++
        if ($retries -lt $maxRetries) {
            Write-Host "   ⏳ Chờ server khởi động ($retries/$maxRetries)..." -ForegroundColor Yellow
            Start-Sleep -Seconds 2
        }
    }
}

if (-not $success) {
    Write-Host "   ❌ Server không phản hồi sau $maxRetries lần thử" -ForegroundColor Red
    Write-Host "   Hãy kiểm tra server logs hoặc chạy 'dotnet run' thủ công" -ForegroundColor Yellow
}
else {
    Write-Host ""
    Write-Host "════════════════════════════════════════════════" -ForegroundColor Green
    Write-Host "✅ SERVER READY FOR QR CODE TESTING!" -ForegroundColor Green
    Write-Host "════════════════════════════════════════════════" -ForegroundColor Green
    Write-Host ""
    Write-Host "📱 Cách test QR Code scan:" -ForegroundColor Cyan
    Write-Host "1. Mở trình duyệt trên máy tính hoặc điện thoại" -ForegroundColor White
    Write-Host "2. Truy cập: http://172.20.10.2:5000" -ForegroundColor Yellow
    Write-Host "3. Vào tab 'POIs' để xem mã QR cho mỗi quán" -ForegroundColor White
    Write-Host "4. Quét mã QR bằng camera điện thoại" -ForegroundColor White
    Write-Host "5. Nó sẽ mở trang thông tin quán ăn" -ForegroundColor White
    Write-Host ""
    Write-Host "⚠️ Lưu ý về QR Code:" -ForegroundColor Yellow
    Write-Host "   • QR Code bây giờ chứa full URL: http://172.20.10.2:5000/qr/POI_XXXXX" -ForegroundColor White
    Write-Host "   • Điện thoại quét được URL đầy đủ → có thể mở ngay" -ForegroundColor White
    Write-Host "   • Không cần copy/paste nữa!" -ForegroundColor White
    Write-Host ""
}
