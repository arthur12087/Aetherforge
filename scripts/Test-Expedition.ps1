param([switch]$Screenshots)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\Environment.ps1"
$evidence = Join-Path $projectRoot 'artifacts\studio-godot\qa'
New-Item -ItemType Directory -Force $evidence | Out-Null
function Invoke-ExpeditionProof([string]$Name, [string[]]$Arguments, [int]$Timeout = 50000) {
    $process = Start-Process $godotRuntimeExe -ArgumentList $Arguments -WorkingDirectory $projectRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput "$evidence\$Name.log" -RedirectStandardError "$evidence\$Name.err.log"
    $null = $process.Handle
    try {
        if (-not $process.WaitForExit($Timeout)) { throw "Timeout no teste de expedição: $Name" }
        $process.Refresh()
        if ($process.ExitCode -ne 0) { throw "Expedicao falhou ($Name, exit=$($process.ExitCode)). Consulte $evidence" }
        $errors = Get-Content "$evidence\$Name.err.log" -Raw -Encoding UTF8
        if ($errors -match 'ERROR|Exception|SCRIPT ERROR') { throw "Erro de runtime ($Name): $errors" }
    } finally { if (-not $process.HasExited) { Stop-Process -Id $process.Id } }
}
Push-Location $projectRoot
try {
    & $dotnetExe run --no-build --project tests/Aetherforge.Checks/Aetherforge.Checks.csproj
    if ($LASTEXITCODE -ne 0) { throw 'Checks de regras/persistência falharam.' }
    Invoke-ExpeditionProof 'route' @('--headless','--path',"`"$clientDir`"",'--','--mode=expedition','--overview','--expedition-smoke','--time-scale=12','--duration=150',"`"--evidence=$evidence\route.json`"")
    $log = Get-Content "$evidence\route.log" -Raw -Encoding UTF8
    if ($log -notmatch 'EXPEDITION PASS') { throw 'Sem prova de conclusão da rota.' }
    Invoke-ExpeditionProof 'interactions' @('--headless','--path',"`"$clientDir`"",'--','--mode=expedition','--overview','--interaction-check','--time-scale=6','--duration=90')
    if ((Get-Content "$evidence\interactions.log" -Raw -Encoding UTF8) -notmatch 'EXPEDITION INTERACTION PASS') { throw 'Sem prova dos callbacks de interface.' }
    if ($Screenshots) {
        Invoke-ExpeditionProof 'native-interactions' @('--path',"`"$clientDir`"",'--','--mode=expedition','--interaction-check','--time-scale=6','--duration=90')
        if ((Get-Content "$evidence\native-interactions.log" -Raw -Encoding UTF8) -notmatch 'EXPEDITION NATIVE WINDOWS PASS') { throw 'Sem prova de tres janelas nativas.' }
        Invoke-ExpeditionProof 'compact' @('--path',"`"$clientDir`"",'--','--mode=expedition','--layout-check','--duration=3',"`"--screenshot=$evidence\compact.png`"")
        foreach ($case in @(@('march','3','1'),@('combat','11','2'),@('pressure','63','6'),@('boss','116','8'),@('complete','149','10'))) {
            $name = $case[0]
            Invoke-ExpeditionProof $name @('--path',"`"$clientDir`"",'--','--mode=expedition','--overview','--layout-check',"--duration=$($case[1])","--time-scale=$($case[2])","`"--screenshot=$evidence\$name.png`"","`"--evidence=$evidence\$name.json`"")
            if (-not (Test-Path "$evidence\$name.png")) { throw "Captura ausente: $name" }
        }
    }
    Write-Host 'PASS: expedicao 3D, rota de cinco encontros/seis tipos, caminhada e paradas, cura, dano e acoes reais da UI.'
    Write-Host "Evidencias: $evidence"
} finally { Pop-Location }
