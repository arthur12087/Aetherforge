param([switch]$Screenshots,[switch]$ProgressionOnly)
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'Environment.ps1')
$qa=Join-Path $projectRoot 'artifacts\ui-v19\qa'
if(!$ProgressionOnly){ & (Join-Path $PSScriptRoot 'Test-UiV15.ps1') -OutputDirectory $qa }
$cases=@(
    @{Name='progression-1';Scale='1';Graphics=$false;Size='1920x1080'},
    @{Name='progression-150';Scale='1.5';Graphics=$false;Size='1366x900'}
)
if($Screenshots){$cases+=@(
    @{Name='review-1';Scale='1';Graphics=$true;Size='1920x1080'},
    @{Name='review-125';Scale='1.25';Graphics=$true;Size='1920x1080'},
    @{Name='review-150';Scale='1.5';Graphics=$true;Size='1920x1080'},
    @{Name='narrow-150';Scale='1.5';Graphics=$true;Size='1366x900'}
)}
foreach($case in $cases){
    $name=$case.Name; $stdout=Join-Path $qa ($name+'.log');$stderr=Join-Path $qa ($name+'.err.log')
    $evidence=Join-Path $qa ($name+'.json');$captures=Join-Path $qa $name
    $arguments=@('--path',('"'+$clientDir+'"'))
    if(!$case.Graphics){$arguments+='--headless'}
    $arguments+=@('--','--mode=expedition','--overview','--progression-check',('--qa-size='+$case.Size),('--ui-scale='+$case.Scale),'--time-scale=8','--duration=400',('"--ui-evidence='+$evidence+'"'),('"--ui-captures='+$captures+'"'))
    $process=Start-Process -FilePath $godotRuntimeExe -ArgumentList $arguments -WorkingDirectory $projectRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
    $null=$process.Handle
    if(!$process.WaitForExit(60000)){$process.Kill();throw ($name+' excedeu 60s')}
    $process.WaitForExit();$process.Refresh()
    $text=[IO.File]::ReadAllText($stdout);$errors=[IO.File]::ReadAllText($stderr)
    if($process.ExitCode -ne 0 -or $text -notmatch 'UI V19 PROGRESSION PASS' -or $errors -match 'ERROR|Exception|SCRIPT ERROR'){throw ($name+' falhou: '+$stdout)}
    Write-Host ('PASS '+$name)
}
Write-Host "UI V19 PASS - level, XP, rewards, boss loot, inspection, retained retry; evidence: $qa"
