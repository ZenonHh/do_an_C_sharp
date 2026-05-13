$ErrorActionPreference = "Stop"

Write-Host "[*] Dang tim file APK moi nhat trong du an..." -ForegroundColor Cyan

# Tim file APK moi nhat trong cac thu muc build
$apkPath = Get-ChildItem -Path "bin" -Filter "*.apk" -Recurse | Where-Object { $_.Name -notmatch "-x86" -and $_.Name -notmatch "-arm" } | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if ($null -eq $apkPath) {
    Write-Host "[x] Khong tim thay file APK nao! Vui long mo Visual Studio va chay App (F5) it nhat 1 lan de tao file." -ForegroundColor Red
    exit
}

Write-Host "[v] Da tim thay App: $($apkPath.FullName)" -ForegroundColor Green

$targetDir = "DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb\wwwroot\apk"
if (-not (Test-Path $targetDir)) {
    New-Item -ItemType Directory -Path $targetDir | Out-Null
}

$targetFile = Join-Path $targetDir "VinhKhanhTour.apk"
Write-Host "[-] Dang tu dong copy file APK vao Server Admin..." -ForegroundColor Cyan
Copy-Item -Path $apkPath.FullName -Destination $targetFile -Force

Write-Host "[v] HOAN TAT! File APK da duoc dat dung cho. Ban co the quet QR de tai App ngay bay gio!" -ForegroundColor Green