@echo off
REM QR Queue System - Quick Test (Windows Batch)
echo ========================================
echo   QR Queue System - Quick Test
echo ========================================
echo.

echo [1/4] Checking if queue service is running...
curl -s http://localhost:5000/api/qrqueue/stats > nul 2>&1
if %errorlevel% neq 0 (
    echo   X Queue service is NOT running!
    echo   Please start the server first: dotnet run
    exit /b 1
)
echo   √ Queue service is running!
echo.

echo [2/4] Testing single request...
curl -s -X POST "http://localhost:5000/api/qrqueue/test/enqueue?isPaid=false"
echo.
echo.

echo [3/4] Adding 15 requests to test queue...
for /L %%i in (1,1,15) do (
    start /B curl -s -X POST "http://localhost:5000/api/qrqueue/test/enqueue?isPaid=false" > nul 2>&1
)
timeout /t 3 /nobreak > nul
echo   √ 15 requests added!
echo.

echo [4/4] Checking final stats...
curl -s http://localhost:5000/api/qrqueue/stats
echo.
echo.

echo ========================================
echo   Test Completed!
echo ========================================
echo.
echo Next Steps:
echo   1. Open: http://localhost:5000/qr-queue-monitor.html
echo   2. Click "Add 10 Requests" button
echo   3. Watch the queue in action!
echo.
pause
