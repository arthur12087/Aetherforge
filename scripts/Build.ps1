param([switch]$BakeSprites)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\Environment.ps1"
Push-Location $projectRoot
try {
    foreach ($project in @('src\Aetherforge.Client\Aetherforge.Client.csproj','src\Aetherforge.Api\Aetherforge.Api.csproj','tests\Aetherforge.Checks\Aetherforge.Checks.csproj')) {
        & $dotnetExe build $project --nologo
        if ($LASTEXITCODE -ne 0) { throw "Falha ao compilar $project" }
    }
    & $godotExe --headless --path $clientDir --editor --import
    if ($LASTEXITCODE -ne 0) { throw 'Falha ao importar projeto Godot.' }
    if ($BakeSprites -or -not (Test-Path "$clientDir\Assets\Generated\Knight.png")) {
        & $godotExe --path $clientDir -- --mode=bake
        if ($LASTEXITCODE -ne 0) { throw 'Falha no render dos sprites.' }
        & $godotExe --headless --path $clientDir --editor --import
        if ($LASTEXITCODE -ne 0) { throw 'Falha ao importar sprites.' }
    }
} finally { Pop-Location }
