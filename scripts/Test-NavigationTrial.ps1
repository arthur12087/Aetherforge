param([switch]$Screenshots,[switch]$South,[switch]$Compact,[switch]$SkipChecks,[string]$OutputDirectory='artifacts/navigation/render')
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'Environment.ps1')
$qa=Join-Path $projectRoot $OutputDirectory
New-Item -ItemType Directory -Force $qa | Out-Null
& $dotnetExe build (Join-Path $clientDir 'Aetherforge.Client.csproj') --nologo -p:NuGetAudit=false
if($LASTEXITCODE-ne 0){throw 'Navigation client build failed'}
if(!$SkipChecks){
    & $dotnetExe run --project (Join-Path $projectRoot 'tests/Aetherforge.NavigationChecks') -p:NuGetAudit=false -- (Join-Path $qa 'checks.json')
    if($LASTEXITCODE-ne 0){throw 'Navigation invariants failed'}
}
$env:DOTNET_ROOT=Join-Path $projectRoot '.tools/dotnet'
& $godotExe --headless --path $clientDir --editor --import
if($LASTEXITCODE-ne 0){throw 'Godot import failed'}
$arguments=@('--path',('"'+$clientDir+'"'),'res://NavigationTrial.tscn','--','--trial-check',('"--evidence='+$qa+'"'))
if(!$Screenshots){$arguments=@('--headless')+$arguments}
if($South){$arguments+='--south'}
if($Compact){$arguments+='--compact'}
$child=Start-Process $godotRuntimeExe -ArgumentList $arguments -WorkingDirectory $projectRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $qa 'runtime.log') -RedirectStandardError (Join-Path $qa 'runtime.err.log')
$null=$child.Handle
try{
    if(!$child.WaitForExit(55000)){throw 'Navigation render exceeded 55s'}
    $child.WaitForExit();$child.Refresh()
    $log=[IO.File]::ReadAllText((Join-Path $qa 'runtime.log'))
    $errors=[IO.File]::ReadAllText((Join-Path $qa 'runtime.err.log'))
    if($child.ExitCode-ne 0 -or $log-notmatch 'NAVIGATION TRIAL RENDER PASS' -or $errors-match 'ERROR|Exception'){
        throw ('Navigation render failed: '+$errors+' '+$log)
    }
    Write-Host 'NAVIGATION TRIAL RENDER PASS'
}finally{if(!$child.HasExited){Stop-Process -Id $child.Id}}
