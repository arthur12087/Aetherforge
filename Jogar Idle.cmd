@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\Run.ps1" -Mode Idle
if errorlevel 1 pause
