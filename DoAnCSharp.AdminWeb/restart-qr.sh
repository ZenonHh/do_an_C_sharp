#!/bin/bash
# Simple restart script for Mac/Linux users

cd ~/source/repos/do_an_C_sharp/DoAnCSharp.AdminWeb/DoAnCSharp.AdminWeb

# Kill old process
pkill -f "dotnet run" 2>/dev/null

# Wait
sleep 2

# Delete old database
rm -f ~/AppData/Local/VinhKhanhTour/VinhKhanhTour_Full.db3 2>/dev/null

# Clean build
dotnet clean &amp;&amp; dotnet build

# Start
echo "Starting server..."
dotnet run

