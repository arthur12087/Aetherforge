param([switch]$Screenshots)
$ErrorActionPreference='Stop'
. "$PSScriptRoot\Environment.ps1"
$artifactDir=Join-Path $projectRoot 'artifacts'
New-Item -ItemType Directory -Force -Path $artifactDir | Out-Null
$stateFile=Join-Path $artifactDir ("integration-test-"+[Guid]::NewGuid().ToString('N')+'.json')
$children=@()
function Start-Proof([string]$Exe,[string[]]$Arguments,[string]$Name) {
    $child=Start-Process $Exe -ArgumentList $Arguments -WorkingDirectory $projectRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput "$artifactDir\$Name.log" -RedirectStandardError "$artifactDir\$Name.err.log"
    $null=$child.Handle; return $child
}
function Start-ProofApi {
    return Start-Proof $dotnetExe @("`"$projectRoot\src\Aetherforge.Api\bin\Debug\net10.0\Aetherforge.Api.dll`"",'--listen=http://127.0.0.1:5085',"`"--state-file=$stateFile`"") 'transfer-api'
}
function Wait-ProofApi {
    for($attempt=0;$attempt-lt 40;$attempt++) {
        try { $health=Invoke-RestMethod 'http://127.0.0.1:5085/health'; if($health.milestone-eq 'integration') { return } } catch { }
        Start-Sleep -Milliseconds 200
    }
    throw 'API de teste nao iniciou.'
}
try {
    & $dotnetExe run --no-build --project (Join-Path $projectRoot 'tests\Aetherforge.Checks\Aetherforge.Checks.csproj')
    if($LASTEXITCODE-ne 0) { throw 'Falha de dominio/persistencia.' }
    $api=Start-ProofApi; $children+=$api; Wait-ProofApi
    $server=Start-Proof $godotRuntimeExe @('--headless','--path',"`"$clientDir`"",'--','--mode=server','--port=17780','--duration=35') 'transfer-server'; $children+=$server
    Start-Sleep -Milliseconds 700
    $visualArgs=@()
    if(-not $Screenshots) { $visualArgs=@('--headless') }
    $idleArgs=$visualArgs+@('--path',"`"$clientDir`"",'--','--mode=idle','--api=http://127.0.0.1:5085','--integration-smoke','--duration=20')
    $mapArgs=$visualArgs+@('--path',"`"$clientDir`"",'--','--mode=map','--port=17780','--api=http://127.0.0.1:5085','--integration-smoke','--duration=21')
    if($Screenshots) { $idleArgs+= "`"--screenshot=$artifactDir\integration-idle.png`""; $mapArgs+="`"--screenshot=$artifactDir\integration-mmo.png`"" }
    $idle=Start-Proof $godotRuntimeExe $idleArgs 'transfer-idle'; $map=Start-Proof $godotRuntimeExe $mapArgs 'transfer-mmo'; $children+=@($idle,$map)
    foreach($client in @($idle,$map)) { if(-not $client.WaitForExit(30000)) { throw 'Timeout do fluxo integrado.' }; if($client.ExitCode-ne 0) { throw 'Cliente falhou. Consulte artifacts/transfer-*.log.' } }
    $idleLog=Get-Content "$artifactDir\transfer-idle.log" -Raw -Encoding UTF8
    $mapLog=Get-Content "$artifactDir\transfer-mmo.log" -Raw -Encoding UTF8
    if($idleLog-notmatch 'INTEGRATION idle PASS item=([a-f0-9]+)') { throw 'Idle nao confirmou transferencia.' }; $itemId=$Matches[1]
    if($mapLog-notmatch 'INTEGRATION map PASS item=([a-f0-9]+)') { throw 'MMO nao confirmou equipamento.' }
    if($Matches[1]-ne $itemId) { throw 'Os clientes usaram itens diferentes.' }
    $state=Invoke-RestMethod 'http://127.0.0.1:5085/integration/state'
    if($state.items.Count-ne 1 -or $state.character.defense-ne 13 -or $state.character.lastDamage-ne 17) { throw 'Resultado integrado incorreto.' }
    Stop-Process -Id $api.Id; $api.WaitForExit()
    $restart=Start-ProofApi; $children+=$restart; Wait-ProofApi
    $restored=Invoke-RestMethod 'http://127.0.0.1:5085/integration/state'
    if($restored.character.equippedItemId-ne $itemId -or $restored.items.Count-ne 1 -or $restored.character.defense-ne 13) { throw 'Reinicio perdeu ou duplicou equipamento.' }
    foreach($name in @('transfer-api','transfer-server','transfer-idle','transfer-mmo')) {
        if((Get-Content "$artifactDir\$name.err.log" -Raw)-match 'ERROR|Exception') { throw "Erro de runtime: $name" }
    }
    Write-Host "PASS: batalha real no servico, drop $itemId, transferencia entre clientes, armadura equipada, dano 17 e persistencia apos reinicio."
} finally { foreach($child in $children) { if(-not $child.HasExited) { Stop-Process -Id $child.Id } } }
