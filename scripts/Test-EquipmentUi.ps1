param([switch]$Screenshots,[switch]$Native,[switch]$Reduced,[string]$Scale='1',[string]$Size='1920x1080',[string]$OutputDirectory='artifacts/equipment-ui',[ValidateSet('none','drop-pause','drop-slot','late-pause')][string]$InputFault='none')
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'Environment.ps1')
$qa=Join-Path $projectRoot $OutputDirectory
New-Item -ItemType Directory -Force $qa | Out-Null
$name=($Size+'-'+$Scale+$(if($Native){'-native'}else{'-embedded'}))
if($Reduced){$name+='-reduced'}
$stdout=Join-Path $qa ($name+'.log'); $stderr=Join-Path $qa ($name+'.err.log')
$arguments=@('--path',('"'+$clientDir+'"'))
if(!$Screenshots){$arguments+='--headless'}
$arguments+=@('--','--mode=expedition','--equipment-check',('--ui-scale='+$Scale),'--time-scale=8','--duration=2000',('"--ui-evidence='+(Join-Path $qa ($name+'.json'))+'"'))
$arguments+=('--equipment-input-fault='+$InputFault)
if(!$Native){$arguments+=@('--overview',('--qa-size='+$Size))}
if($Reduced){$arguments+='--reduce-ui-motion'}
if($Screenshots){$arguments+=('"--ui-captures='+(Join-Path $qa $name)+'"')}
$process=Start-Process -FilePath $godotRuntimeExe -ArgumentList $arguments -WorkingDirectory $projectRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
$null=$process.Handle
try {
    if(!$process.WaitForExit(120000)){throw ($name+' excedeu 120s')}
    $process.WaitForExit();$process.Refresh()
    $text=[IO.File]::ReadAllText($stdout);$errors=[IO.File]::ReadAllText($stderr)
    if($process.ExitCode -ne 0 -or $text -notmatch 'EQUIPMENT UI PASS' -or $errors -match 'ERROR|Exception|SCRIPT ERROR'){throw ($name+' falhou: '+$stderr+': '+$errors)}
    Write-Host ('PASS '+$name)
} finally {if(!$process.HasExited){Stop-Process -Id $process.Id}}
