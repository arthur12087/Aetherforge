$projectRoot = Split-Path $PSScriptRoot -Parent
$env:DOTNET_ROOT = Join-Path $projectRoot '.tools\dotnet'
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'
$dotnetExe = Join-Path $env:DOTNET_ROOT 'dotnet.exe'
$godotExe = (Get-ChildItem (Join-Path $projectRoot '.tools\godot') -Recurse -Filter '*_console.exe' | Select-Object -First 1).FullName
$godotRuntimeExe = (Get-ChildItem (Join-Path $projectRoot '.tools\godot') -Recurse -Filter '*_win64.exe' | Select-Object -First 1).FullName
$clientDir = Join-Path $projectRoot 'src\Aetherforge.Client'
if (-not (Test-Path -LiteralPath $dotnetExe) -or -not $godotExe) { throw 'Execute scripts/Setup.ps1 primeiro.' }
