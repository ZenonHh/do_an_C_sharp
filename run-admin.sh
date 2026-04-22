#!/bin/bash
# Script chạy Web Admin nhanh (Linux/Mac)

PORT=${1:-5000}

echo ""
echo "========================================"
echo "  Vĩnh Khánh Tour - Admin Web"
echo "========================================"
echo ""
echo "Starting on http://localhost:$PORT"
echo "Press Ctrl+C to stop"
echo ""

cd DoAnCSharp.AdminWeb/DoAnCSharp.AdminWeb
dotnet run --urls "http://localhost:$PORT"
