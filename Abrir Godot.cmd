@echo off
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\scripts\Studio-Godot.ps1" -OpenEditor
if errorlevel 1 pause
