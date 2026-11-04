param([switch]$South,[switch]$Compact,[switch]$Natural,[switch]$Before,[switch]$Wide,[switch]$SkipBuild,[switch]$Video,[string]$OutputDirectory='artifacts/camera/head')
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'Environment.ps1')
$qa=Join-Path $projectRoot $OutputDirectory
New-Item -ItemType Directory -Force $qa | Out-Null
$baseSha='52267cb833b135778b6b6b7a639eab4c2f0d06ca'
$clientSources=@('src/Aetherforge.Client/NavigationTrial.cs','src/Aetherforge.Client/TrialGroupCamera.cs')
$backups=@{}
$child=$null
try{
    if($Before){
        # Exact baseline files, no reset/stash/branch rewrite. Restore working bytes even on failure.
        foreach($file in $clientSources){$backups[$file]=[IO.File]::ReadAllBytes((Join-Path $projectRoot $file))}
        & git -C $projectRoot archive -o (Join-Path $qa 'baseline-client.zip') $baseSha -- $clientSources
        if($LASTEXITCODE-ne 0){throw 'Could not extract exact baseline'}
        Expand-Archive -LiteralPath (Join-Path $qa 'baseline-client.zip') -DestinationPath (Join-Path $qa 'baseline-source') -Force
        foreach($file in $clientSources){Copy-Item -LiteralPath (Join-Path (Join-Path $qa 'baseline-source') $file) -Destination (Join-Path $projectRoot $file)}
    }
    $source=[ordered]@{base=$baseSha;before=[bool]$Before;files=@()}
    foreach($file in $clientSources){$source.files+=[ordered]@{file=$file;sha256=(Get-FileHash (Join-Path $projectRoot $file) -Algorithm SHA256).Hash.ToLowerInvariant()}}
    [IO.File]::WriteAllText((Join-Path $qa 'source.json'),($source|ConvertTo-Json -Depth 5),[Text.UTF8Encoding]::new($false))
    if(!$SkipBuild -or $Before){
        # Archive timestamps may be older than the existing assembly. A/B evidence
        # must rebuild rather than reuse the incrementally cached visual binary.
        & $dotnetExe build (Join-Path $clientDir 'Aetherforge.Client.csproj') --no-incremental --nologo -p:NuGetAudit=false | Tee-Object -FilePath (Join-Path $qa 'build.log')
        if($LASTEXITCODE-ne 0){throw 'Camera build failed'}
    }
    $source.binaries=@()
    foreach($name in @('Aetherforge.Client.dll','Aetherforge.Domain.dll')){$source.binaries+=[ordered]@{file=$name;sha256=(Get-FileHash (Join-Path $clientDir ('.godot/mono/temp/bin/Debug/'+$name)) -Algorithm SHA256).Hash.ToLowerInvariant()}}
    $source.driverSha256=(Get-FileHash (Join-Path $clientDir 'TrialCameraCheck.cs') -Algorithm SHA256).Hash.ToLowerInvariant()
    [IO.File]::WriteAllText((Join-Path $qa 'source.json'),($source|ConvertTo-Json -Depth 5),[Text.UTF8Encoding]::new($false))
    & $godotExe --headless --path $clientDir --editor --import --log-file (Join-Path $qa 'import-engine.log') | Out-File (Join-Path $qa 'import.log')
    if($LASTEXITCODE-ne 0){throw 'Camera scene import failed'}
    $arguments=@('--path',('"'+$clientDir+'"'),'--log-file',('"'+(Join-Path $qa 'engine.log')+'"'),'res://TrialCameraCheck.tscn','--',('"--evidence='+$qa+'"'),('--tag='+$(if($Before){$baseSha}else{'visual-increment'})))
    if($South){$arguments+='--south'}
    if($Compact){$arguments+='--compact'}
    if($Natural){$arguments+='--camera-natural'}
    if($Before){$arguments+='--camera-before'}
    if($Wide){$arguments+='--camera-wide'}
    if($Video){if(!$Natural){throw 'Video evidence requires natural wall clock'};$arguments+=('"--video='+(Join-Path $qa 'natural-1x.mp4')+'"')}
    $child=Start-Process $godotRuntimeExe -ArgumentList $arguments -WorkingDirectory $projectRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $qa 'runtime.log') -RedirectStandardError (Join-Path $qa 'runtime.err.log')
    $null=$child.Handle
    if(!$child.WaitForExit(95000)){throw 'Camera proof exceeded 95s'}
    $child.WaitForExit();$child.Refresh()
    $log=[IO.File]::ReadAllText((Join-Path $qa 'runtime.log'));$errors=[IO.File]::ReadAllText((Join-Path $qa 'runtime.err.log'))
    if($child.ExitCode-ne 0 -or $log-notmatch 'TRIAL CAMERA CHECK PASS' -or $errors-match 'ERROR|Exception'){throw ('Camera proof failed: '+$errors+' '+$log)}
    $record=Get-Content (Join-Path $qa 'camera.json') -Raw | ConvertFrom-Json
    if(!$record.passed){throw 'Camera JSON reported failure'}
    Write-Host ('TRIAL CAMERA CHECK PASS '+$OutputDirectory)
}finally{
    if($child -and !$child.HasExited){Stop-Process -Id $child.Id}
    foreach($file in $backups.Keys){[IO.File]::WriteAllBytes((Join-Path $projectRoot $file),$backups[$file])}
}
