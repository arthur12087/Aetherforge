param([switch]$Screenshots, [string]$OutputDirectory = 'artifacts\ui-v14\qa')
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Environment.ps1')
$qa = if ([IO.Path]::IsPathRooted($OutputDirectory)) { $OutputDirectory } else { Join-Path $projectRoot $OutputDirectory }
New-Item -ItemType Directory -Force $qa | Out-Null

function Invoke-UiCase {
    param([string]$Name, [string[]]$Options, [switch]$Graphics, [string]$Expected='UI V14 INPUT PASS', [int]$Timeout=65)
    $stdout = Join-Path $qa ($Name+'.log'); $stderr = Join-Path $qa ($Name+'.err.log')
    $arguments = @('--path',('"'+$clientDir+'"'))
    if (!$Graphics) { $arguments += '--headless' }
    $arguments += '--'; $arguments += $Options
    $process = Start-Process -FilePath $godotRuntimeExe -ArgumentList $arguments -WorkingDirectory $projectRoot -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
    $null = $process.Handle
    if (!$process.WaitForExit($Timeout*1000)) { $process.Kill(); throw "$Name excedeu ${Timeout}s" }
    $process.WaitForExit(); $process.Refresh()
    $text = [IO.File]::ReadAllText($stdout)
    if ($process.ExitCode -ne 0 -or ($Expected -and $text -notmatch [regex]::Escape($Expected))) { throw "$Name falhou: veja $stdout e $stderr" }
    $errors = [IO.File]::ReadAllText($stderr)
    if ($errors -match 'ERROR|Exception|SCRIPT ERROR') { throw "$Name emitiu erro: $stderr" }
    Write-Host "PASS $Name"
}

foreach ($scale in @('1','1.25','1.5')) {
    $name = 'input-'+$scale.Replace('.','-')
    $evidence = Join-Path $qa ($name+'.json')
    # A large virtual viewport isolates typography/scroll checks from physical desktop DPI.
    Invoke-UiCase -Name $name -Options @('--mode=expedition','--overview','--qa-size=1920x1080','--ui-check',"--ui-scale=$scale",'--time-scale=2','--duration=55',('"--ui-evidence='+$evidence+'"'))
}

if ($Screenshots) {
    foreach ($scale in @('1','1.25','1.5')) {
        $suffix = if ($scale -eq '1') { '' } else { '-'+$scale.Replace('.','-') }
        $captures = Join-Path $qa ('native-menus'+$suffix)
        $nativeEvidence = Join-Path $qa ('native-input'+$suffix+'.json')
        Invoke-UiCase -Name ('native-input'+$suffix) -Graphics -Options @('--mode=expedition','--ui-check',"--ui-scale=$scale",'--time-scale=2','--duration=55',('"--ui-evidence='+$nativeEvidence+'"'),('"--ui-captures='+$captures+'"'))
    }

    $cases = @(
        @{ Name='typography'; Options=@('--mode=ui-proof'); Scale='1'; Speed='1'; Duration='1.2' },
        @{ Name='compact'; Options=@('--mode=expedition'); Scale='1'; Speed='1'; Duration='2' },
        @{ Name='compact-125'; Options=@('--mode=expedition'); Scale='1.25'; Speed='1'; Duration='2' },
        @{ Name='compact-150'; Options=@('--mode=expedition'); Scale='1.5'; Speed='1'; Duration='2' },
        @{ Name='overview-150'; Options=@('--mode=expedition','--overview'); Scale='1.5'; Speed='2'; Duration='11' },
        @{ Name='combat'; Options=@('--mode=expedition','--overview'); Scale='1'; Speed='1'; Duration='10' },
        @{ Name='boss'; Options=@('--mode=expedition','--overview'); Scale='1'; Speed='8'; Duration='116' },
        @{ Name='detailed'; Options=@('--mode=expedition','--overview','--hud-details'); Scale='1'; Speed='1'; Duration='10' },
        @{ Name='complete'; Options=@('--mode=expedition','--overview'); Scale='1'; Speed='10'; Duration='149' }
    )
    foreach ($case in $cases) {
        $image = Join-Path $qa ($case.Name+'.png'); $evidence = Join-Path $qa ($case.Name+'.json')
        $options = $case.Options + @('--ui-scale='+$case.Scale,'--time-scale='+$case.Speed,'--duration='+$case.Duration,'--layout-check',('"--screenshot='+$image+'"'),('"--evidence='+$evidence+'"'))
        Invoke-UiCase -Name $case.Name -Graphics -Options $options -Expected 'SCREENSHOT Ok'
    }
}
Write-Host "UI V14 PASS - evidence: $qa"
