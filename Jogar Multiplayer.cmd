@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\Run.ps1" -Mode Demo
if errorlevel 1 pause
