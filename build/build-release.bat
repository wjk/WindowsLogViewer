@echo off
cd %~dp0..
cd src\WindowsLogViewer
dotnet build -c Release -f net7.0-windows10.0.18362 /t:GenerateMsixBundle %*
