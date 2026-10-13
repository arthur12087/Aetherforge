param([ValidateRange(1024,65535)][int]$MapPort=17779)
$ErrorActionPreference='Stop'
. "$PSScriptRoot\Environment.ps1"
$artifactDir=Join-Path $projectRoot 'artifacts'
New-Item -ItemType Directory -Force -Path $artifactDir | Out-Null
$children=@()
try {
    $health=$null
    try { $health=Invoke-RestMethod 'http://127.0.0.1:5080/health' -TimeoutSec 1 } catch { }
    if ($health -and $health.milestone -ne 'integration') { throw 'A porta 5080 pertence a outro servico. Encerre essa instancia antes de abrir o teste.' }
    if (-not $health) {
        $api=Start-Process $dotnetExe -ArgumentList @("`"$projectRoot\src\Aetherforge.Api\bin\Debug\net10.0\Aetherforge.Api.dll`"") -WorkingDirectory $projectRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput "$artifactDir\integration-api.log" -RedirectStandardError "$artifactDir\integration-api.err.log"
        $null=$api.Handle; $children+=$api
        for($attempt=0;$attempt-lt 30;$attempt++) {
            try { $health=Invoke-RestMethod 'http://127.0.0.1:5080/health'; break } catch { Start-Sleep -Milliseconds 200 }
        }
        if (-not $health) { throw 'API nao iniciou. Consulte artifacts/integration-api.err.log.' }
    }
    $server=Start-Process $godotRuntimeExe -ArgumentList @('--headless','--path',"`"$clientDir`"",'--','--mode=server',"--port=$MapPort") -WindowStyle Hidden -PassThru -RedirectStandardOutput "$artifactDir\integration-map-server.log" -RedirectStandardError "$artifactDir\integration-map-server.err.log"
    $null=$server.Handle; $children+=$server
    Start-Sleep -Milliseconds 1000
    if($server.HasExited) { throw 'Servidor de mapa nao iniciou. Verifique a porta e o log.' }
    $map=Start-Process $godotRuntimeExe -ArgumentList @('--path',"`"$clientDir`"",'--position','40,60','--','--mode=map',"--port=$MapPort") -WindowStyle Normal -PassThru
    $idle=Start-Process $godotRuntimeExe -ArgumentList @('--path',"`"$clientDir`"",'--','--mode=idle') -WindowStyle Normal -PassThru
    $null=$map.Handle; $null=$idle.Handle; $children+=@($map,$idle)
    Write-Host 'Aguarde a primeira vitoria no idle, envie o drop e equipe no MMO.'
    Write-Host 'Os itens ficam salvos em data/integration-state.json. Feche os dois clientes para encerrar.'
    $idle.WaitForExit(); $map.WaitForExit()
} finally { foreach($child in $children) { if(-not $child.HasExited) { Stop-Process -Id $child.Id } } }
