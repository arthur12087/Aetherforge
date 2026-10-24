param([switch]$Screenshots,[switch]$Fast,[string]$OutputDirectory='artifacts/presentation')
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'Environment.ps1')
$qa=Join-Path $projectRoot $OutputDirectory
New-Item -ItemType Directory -Force $qa | Out-Null
$arguments=@('--path',('"'+$clientDir+'"'),'--log-file',('"'+(Join-Path $qa 'engine.log')+'"'))
if(!$Screenshots){$arguments+='--headless'}
$arguments+=@('--','--mode=expedition','--overview','--qa-size=1000x720','--time-scale=1','--presentation-check',('"--ui-evidence='+(Join-Path $qa 'evidence.json')+'"'))
if($Fast){$arguments+='--presentation-fast'}
if($Screenshots){$arguments+=('"--ui-captures='+(Join-Path $qa 'captures')+'"')}
$stdout=Join-Path $qa 'runtime.log'; $stderr=Join-Path $qa 'runtime.err.log'
$process=Start-Process -FilePath $godotRuntimeExe -ArgumentList $arguments -WorkingDirectory $projectRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
$null=$process.Handle
try {
    if(!$process.WaitForExit(55000)){throw 'Presentation check exceeded 55s'}
    $process.WaitForExit();$process.Refresh()
    $text=[IO.File]::ReadAllText($stdout);$errors=[IO.File]::ReadAllText($stderr)
    if($process.ExitCode -ne 0 -or $text -notmatch 'PRESENTATION CHECK PASS' -or $errors -match 'ERROR|Exception|SCRIPT ERROR'){throw ('Presentation check failed: '+$errors)}
    Write-Host ($text -split "`n" | Where-Object {$_ -match 'PRESENTATION CHECK PASS'})
} finally {if(!$process.HasExited){Stop-Process -Id $process.Id}}
