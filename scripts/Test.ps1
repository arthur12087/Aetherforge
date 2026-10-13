param([switch]$Screenshots)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\Environment.ps1"
$artifactDir = Join-Path $projectRoot 'artifacts'
New-Item -ItemType Directory -Force -Path $artifactDir | Out-Null
$children = @()
function Start-TestProcess([string]$Exe, [string[]]$Arguments, [string]$Name) {
    $process = Start-Process -FilePath $Exe -ArgumentList $Arguments -WorkingDirectory $projectRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput "$artifactDir\$Name.log" -RedirectStandardError "$artifactDir\$Name.err.log"
    $null = $process.Handle
    return $process
}
try {
    & $dotnetExe run --no-build --project (Join-Path $projectRoot 'tests\Aetherforge.Checks\Aetherforge.Checks.csproj')
    if ($LASTEXITCODE -ne 0) { throw 'Falha nos testes de dominio.' }
    $testState=Join-Path $artifactDir ('m0-test-'+[Guid]::NewGuid().ToString('N')+'.json')
    $api = Start-TestProcess $dotnetExe @("`"$projectRoot\src\Aetherforge.Api\bin\Debug\net10.0\Aetherforge.Api.dll`"",'--listen=http://127.0.0.1:5084',"`"--state-file=$testState`"") 'api'
    $children += $api
    $health = $null
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        try { $health = Invoke-RestMethod 'http://127.0.0.1:5084/health'; break } catch { Start-Sleep -Milliseconds 200 }
    }
    if ($health.status -ne 'ok') { throw 'API sem resposta.' }
    $party = Invoke-RestMethod 'http://127.0.0.1:5084/prototype/team'
    if ($party.Count -ne 3 -or @($party.role | Sort-Object -Unique).Count -ne 3) { throw 'API nao devolveu tres funcoes.' }
    Write-Host 'PASS: API .NET 10 e biblioteca de dominio compartilhada.'

    $server = Start-TestProcess $godotRuntimeExe @('--headless','--path',"`"$clientDir`"",'--','--mode=server','--port=17777','--duration=25') 'network-server'
    $children += $server
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        if ((Test-Path "$artifactDir\network-server.log") -and (Get-Content "$artifactDir\network-server.log" -Raw) -match 'SERVER READY') { break }
        Start-Sleep -Milliseconds 200
    }
    $arguments = @('--headless','--path',"`"$clientDir`"",'--','--mode=map','--port=17777','--smoke','--expect-wall','--duration=8')
    if ($Screenshots) { $arguments = @('--path',"`"$clientDir`"",'--','--mode=map','--port=17777','--smoke','--expect-wall','--duration=8','--rotate',"`"--screenshot=$artifactDir\map-m0.png`"") }
    $one = Start-TestProcess $godotRuntimeExe ($arguments + @('--bot=0','--api=http://127.0.0.1:5084')) 'network-one'
    $twoDuration = if ($Screenshots) { '--duration=16' } else { '--duration=9' }
    $two = Start-TestProcess $godotRuntimeExe @('--headless','--path',"`"$clientDir`"",'--','--mode=map','--port=17777','--api=http://127.0.0.1:5084','--smoke','--expect-wall',$twoDuration,'--bot=wall') 'network-two'
    $children += @($one,$two)
    foreach ($client in @($one,$two)) {
        if (-not $client.WaitForExit(18000)) { throw 'Timeout no cliente de rede.' }
        $client.Refresh()
        if ($client.ExitCode -ne 0) { throw 'Cliente retornou erro. Consulte artifacts/network-*.log.' }
    }
    foreach ($name in @('network-one','network-two')) {
        $log = Get-Content "$artifactDir\$name.log" -Raw
        if ($log -notmatch 'NETWORK PASS') { throw "Sem evidencia de sucesso: $name" }
        Write-Host ($log -split "`n" | Where-Object { $_ -match 'NETWORK PASS' })
    }
    foreach ($file in @('network-server','network-one','network-two','api')) {
        if ((Get-Content "$artifactDir\$file.err.log" -Raw) -match 'ERROR|Exception') { throw "Erro de runtime: $file" }
    }
    if ($Screenshots) {
        & $godotExe --path $clientDir -- --mode=idle --api=http://127.0.0.1:5084 --layout-check --duration=8.5 "--screenshot=$artifactDir\idle-m0.png"
        if ($LASTEXITCODE -ne 0) { throw 'Erro ao capturar idle.' }
    }
    Write-Host 'PASS: verificacao M0. Logs em artifacts.'
} finally {
    foreach ($child in $children) { if (-not $child.HasExited) { Stop-Process -Id $child.Id } }
}
