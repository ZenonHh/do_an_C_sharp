#!/usr/bin/env pwsh

# 🧹 Clean up & 🚀 Push to GitHub
# This script removes unnecessary docs and pushes to GitHub

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "🧹 Cleanup & 🚀 GitHub Push" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Change to repo directory
cd "C:\Users\LENOVO\source\repos\do_an_C_sharp"

# ===== STEP 1: CHECK GIT STATUS =====
Write-Host "Step 1: Checking Git status..." -ForegroundColor Yellow
git status
Write-Host ""

# ===== STEP 2: LIST FILES TO DELETE =====
Write-Host "Step 2: Finding unnecessary documentation files..." -ForegroundColor Yellow
Write-Host ""

$filesToDelete = @(
    # Device tracking docs
    "DoAnCSharp.AdminWeb/DEVICE_TRACKING_*.md",
    
    # POI not found docs
    "DoAnCSharp.AdminWeb/FIX_POI_NOT_FOUND_*.md",
    "DoAnCSharp.AdminWeb/POI_NOT_FOUND_INDEX.md",
    "DoAnCSharp.AdminWeb/POI_LOOKUP_FLOW_DIAGRAM.md",
    "DoAnCSharp.AdminWeb/QUICK_FIX_POI_NOT_FOUND.md",
    "DoAnCSharp.AdminWeb/DO_THIS_NOW_FIX.md",
    "DoAnCSharp.AdminWeb/START_FIX_NOW.md",
    "DoAnCSharp.AdminWeb/COMPLETE_FIX_POI_NOT_FOUND.md",
    "DoAnCSharp.AdminWeb/VISUAL_FIX_GUIDE.md",
    
    # QR code docs
    "DoAnCSharp.AdminWeb/QR_*.md",
    "DoAnCSharp.AdminWeb/QR_*.txt",
    
    # CSS docs
    "DoAnCSharp.AdminWeb/CSS_*.md",
    "DoAnCSharp.AdminWeb/FINAL_CSS_*.md",
    "DoAnCSharp.AdminWeb/START_CSS_*.md",
    
    # Fix/error docs
    "DoAnCSharp.AdminWeb/FIXES_*.md",
    "DoAnCSharp.AdminWeb/ERROR_*.md",
    "DoAnCSharp.AdminWeb/FIX_*.md",
    "DoAnCSharp.AdminWeb/FINAL_*.md",
    "DoAnCSharp.AdminWeb/START_*.md",
    "DoAnCSharp.AdminWeb/ALL_*.md",
    "DoAnCSharp.AdminWeb/READY_*.md",
    "DoAnCSharp.AdminWeb/WORK_*.md",
    "DoAnCSharp.AdminWeb/IMPLEMENTATION_*.md",
    "DoAnCSharp.AdminWeb/README_*.md",
    "DoAnCSharp.AdminWeb/DOCUMENTATION_*.md",
    "DoAnCSharp.AdminWeb/VERIFICATION_*.md",
    "DoAnCSharp.AdminWeb/QUICK_*.md",
    "DoAnCSharp.AdminWeb/HOW_TO_*.md",
    "DoAnCSharp.AdminWeb/TEST_*.md",
    "DoAnCSharp.AdminWeb/VISUAL_*.md",
    "DoAnCSharp.AdminWeb/CHANGES_*.md",
    "DoAnCSharp.AdminWeb/EXACT_*.md",
    "DoAnCSharp.AdminWeb/FORM_*.md",
    "DoAnCSharp.AdminWeb/GO_*.md",
    "DoAnCSharp.AdminWeb/USER_*.md",
    
    # Setup/test docs
    "DoAnCSharp.AdminWeb/*.ps1",
    "DoAnCSharp.AdminWeb/QUICK_REFERENCE.txt",
    "DoAnCSharp.AdminWeb/run-admin.ps1",
    "DoAnCSharp.AdminWeb/run-admin.bat",
    "DoAnCSharp.AdminWeb/quick-qr-fix.ps1",
    "DoAnCSharp.AdminWeb/test-qr-scanning.ps1",
    "DoAnCSharp.AdminWeb/test-all-features.ps1",
    "DoAnCSharp.AdminWeb/fix-css-qr.ps1",
    "DoAnCSharp.AdminWeb/cleanup-and-restart.ps1",
    "DoAnCSharp.AdminWeb/restart-server-with-fix.ps1",
    "DoAnCSharp.AdminWeb/restart-and-test-qr.ps1",
    "DoAnCSharp.AdminWeb/fix-and-restart.ps1",
    "DoAnCSharp.AdminWeb/generate-qr-codes.ps1",
    "DoAnCSharp.AdminWeb/fix-poi-not-found.ps1",
    "DoAnCSharp.AdminWeb/test-poi-seeding.ps1",
    
    # Other docs
    "DOAnCSharp.AdminWeb/restart-qr.sh",
    "start-admin.ps1",
    "HOW_TO_RUN_SIMPLE.md",
    "test-integration.ps1",
    "INTEGRATION_QUICK_START.md",
    
    # HTML test files
    "DoAnCSharp.AdminWeb/DoAnCSharp.AdminWeb/wwwroot/test-qr.html"
)

Write-Host "Files to delete:" -ForegroundColor Cyan
foreach ($pattern in $filesToDelete) {
    $files = Get-Item $pattern -ErrorAction SilentlyContinue
    if ($files) {
        $files | ForEach-Object { Write-Host "  🗑️  $_" }
    }
}

Write-Host ""
Write-Host "Total files found: $(($filesToDelete | Measure-Object).Count)" -ForegroundColor Yellow
Write-Host ""

# ===== STEP 3: CONFIRM DELETION =====
$response = Read-Host "Delete these files? (yes/no)"

if ($response -eq "yes") {
    Write-Host "Step 3: Deleting files..." -ForegroundColor Yellow
    
    foreach ($pattern in $filesToDelete) {
        Get-Item $pattern -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue
    }
    
    Write-Host "✅ Files deleted" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "Skipping deletion" -ForegroundColor Yellow
    Write-Host ""
}

# ===== STEP 4: CHECK WHAT'S STAGED =====
Write-Host "Step 4: Checking Git status..." -ForegroundColor Yellow
git status
Write-Host ""

# ===== STEP 5: ADD ALL CHANGES =====
Write-Host "Step 5: Staging changes..." -ForegroundColor Yellow
git add -A
Write-Host "✅ Changes staged" -ForegroundColor Green
Write-Host ""

# ===== STEP 6: COMMIT =====
Write-Host "Step 6: Creating commit..." -ForegroundColor Yellow
$commitMsg = "🧹 Cleanup: Remove unnecessary documentation files"
git commit -m $commitMsg
Write-Host "✅ Commit created" -ForegroundColor Green
Write-Host ""

# ===== STEP 7: PUSH TO GITHUB =====
Write-Host "Step 7: Pushing to GitHub..." -ForegroundColor Yellow
git push origin main

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Successfully pushed to GitHub!" -ForegroundColor Green
} else {
    Write-Host "❌ Push failed. Check your connection and try again." -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "✅ Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Repository: https://github.com/ZenonHh/test_C_sharp" -ForegroundColor Cyan
Write-Host "Branch: main" -ForegroundColor Cyan
