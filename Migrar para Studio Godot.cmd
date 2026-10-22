@echo off
cd /d "%~dp0"
echo Feche o OhMyGame antes de continuar. O cadastro atual sera preservado em backup.
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\scripts\Studio-Godot.ps1" -Apply -OpenEditor
pause
