param([switch]$Screenshots,[switch]$South,[switch]$Compact,[switch]$SkipChecks,[switch]$SkipBuild,[switch]$Interpolated,[string]$OutputDirectory='artifacts/navigation/render')
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'Environment.ps1')
$qa=Join-Path $projectRoot $OutputDirectory
New-Item -ItemType Directory -Force $qa | Out-Null
if(!$SkipBuild){
    & $dotnetExe build (Join-Path $clientDir 'Aetherforge.Client.csproj') --nologo -p:NuGetAudit=false | Tee-Object -FilePath (Join-Path $qa 'build.log')
    if($LASTEXITCODE -ne 0){throw 'Navigation client build failed'}
}
if(!$SkipChecks){
    & $dotnetExe run --project (Join-Path $projectRoot 'tests/Aetherforge.NavigationChecks') -p:NuGetAudit=false -- (Join-Path $qa 'checks.json') | Tee-Object -FilePath (Join-Path $qa 'checks.log')
    if($LASTEXITCODE -ne 0){throw 'Navigation invariants failed; checks.json/log retained'}
    $checkLog=Get-Content (Join-Path $qa 'checks.log') -Raw
    $checkJson=Get-Content (Join-Path $qa 'checks.json') -Raw | ConvertFrom-Json
    if($checkLog -notmatch 'NAVIGATION TRIAL CHECK PASS' -or !$checkJson.passed){throw 'Missing navigation check evidence'}
    $findings=@($checkJson.corrective | ForEach-Object { $_.finding })
    foreach($required in @('F1','F2','F3-wait','F3-lane','F3-explicit-return','F4-stale','F4-restore')){
        if($findings -notcontains $required){throw ('Missing corrective coverage: '+$required)}
    }
    if(@($checkJson.continuous).Count -ne 2 -or @($checkJson.continuous | Where-Object { !$_.Complete }).Count -ne 0){throw 'Missing continuous N/S evidence'}
}
$env:DOTNET_ROOT=Join-Path $projectRoot '.tools/dotnet'
& $godotExe --headless --path $clientDir --editor --import --log-file (Join-Path $qa 'import-engine.log') | Tee-Object -FilePath (Join-Path $qa 'import.log')
if($LASTEXITCODE -ne 0){throw 'Godot import failed'}
if((Get-Content (Join-Path $qa 'import-engine.log') -Raw) -match 'ERROR:|Exception'){throw 'Godot import reported errors; logs retained'}
$arguments=@('--path',('"'+$clientDir+'"'),'--log-file',('"'+(Join-Path $qa 'engine.log')+'"'),'res://NavigationTrial.tscn','--','--trial-check',('"--evidence='+$qa+'"'))
if(!$Screenshots){$arguments=@('--headless')+$arguments}
if($South){$arguments+='--south'}
if($Compact){$arguments+='--compact'}
if($Interpolated){$arguments+='--interpolated'}
$child=Start-Process $godotRuntimeExe -ArgumentList $arguments -WorkingDirectory $projectRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $qa 'runtime.log') -RedirectStandardError (Join-Path $qa 'runtime.err.log')
$null=$child.Handle
try{
    if(!$child.WaitForExit(55000)){throw 'Navigation render exceeded 55s'}
    $child.WaitForExit();$child.Refresh()
    $log=[IO.File]::ReadAllText((Join-Path $qa 'runtime.log'))
    $errors=[IO.File]::ReadAllText((Join-Path $qa 'runtime.err.log'))
    if($child.ExitCode -ne 0 -or $log -notmatch 'NAVIGATION TRIAL RENDER PASS' -or $errors -match 'ERROR|Exception'){
        throw ('Navigation render failed: '+$errors+' '+$log)
    }
    $render=Get-Content (Join-Path $qa 'render.json') -Raw | ConvertFrom-Json
    if(!$render.passed -or !$render.checkpointVerified -or !$render.transitionsVerified){throw 'Missing navigation smoke JSON evidence'}
    Write-Host 'NAVIGATION TRIAL RENDER PASS'
}finally{if(!$child.HasExited){Stop-Process -Id $child.Id}}
