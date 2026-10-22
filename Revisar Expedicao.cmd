@echo off
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\scripts\Run.ps1" -Mode Expedition -Overview
if errorlevel 1 pause
