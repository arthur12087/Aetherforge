param([switch]$Screenshots,[switch]$Native,[string]$Size='1160x880',[string]$OutputDirectory='artifacts/number-layout')
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'Environment.ps1')
$qa=Join-Path $projectRoot $OutputDirectory
New-Item -ItemType Directory -Force $qa | Out-Null
$arguments=@('--path',('"'+$clientDir+'"'),'--log-file',('"'+(Join-Path $qa 'engine.log')+'"'))
if(!$Screenshots){$arguments+='--headless'}
$arguments+=@('--','--mode=expedition',('--qa-size='+$Size),'--time-scale=1','--number-layout-check',('"--ui-evidence='+(Join-Path $qa 'evidence.json')+'"'))
if(!$Native){$arguments+='--overview'}
if($Screenshots){$arguments+=('"--ui-captures='+(Join-Path $qa 'captures')+'"')}
$stdout=Join-Path $qa 'runtime.log'; $stderr=Join-Path $qa 'runtime.err.log'
$process=Start-Process -FilePath $godotRuntimeExe -ArgumentList $arguments -WorkingDirectory $projectRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
$null=$process.Handle
try {
    if(!$process.WaitForExit(55000)){throw 'Number layout check exceeded 55s'}
    $process.WaitForExit();$process.Refresh()
    $text=[IO.File]::ReadAllText($stdout);$errors=[IO.File]::ReadAllText($stderr)
    if($process.ExitCode -ne 0 -or $text -notmatch 'NUMBER LAYOUT CHECK PASS' -or $errors -match 'ERROR|Exception|SCRIPT ERROR'){throw ('Number layout check failed: '+$errors)}
    Write-Host ($text -split "`n" | Where-Object {$_ -match 'NUMBER LAYOUT CHECK PASS'})
} finally {if(!$process.HasExited){Stop-Process -Id $process.Id}}
