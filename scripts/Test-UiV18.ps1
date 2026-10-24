param([switch]$Screenshots)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Environment.ps1')
$qa = Join-Path $projectRoot 'artifacts\ui-v18\qa'
# Shared suite exercises real Viewport input, the Status state stack, layout and motion.
& (Join-Path $PSScriptRoot 'Test-UiV15.ps1') -OutputDirectory $qa
if ($Screenshots) {
    foreach ($scale in @('1','1.25','1.5')) {
        $name = 'status-review-'+$scale.Replace('.','-')
        $stdout = Join-Path $qa ($name+'.log'); $stderr = Join-Path $qa ($name+'.err.log')
        $evidence = Join-Path $qa ($name+'.json'); $captures = Join-Path $qa $name
        $arguments = @('--path',('"'+$clientDir+'"'),'--','--mode=expedition','--overview','--qa-size=1920x1080','--ui-check',('--ui-scale='+$scale),'--time-scale=2','--duration=55',('"--ui-evidence='+$evidence+'"'),('"--ui-captures='+$captures+'"'))
        $process = Start-Process -FilePath $godotRuntimeExe -ArgumentList $arguments -WindowStyle Hidden -WorkingDirectory $projectRoot -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
        $null = $process.Handle
        if (!$process.WaitForExit(65000)) { $process.Kill(); throw ($name+' excedeu 65s') }
        $process.WaitForExit(); $process.Refresh()
        $text = [IO.File]::ReadAllText($stdout); $errors = [IO.File]::ReadAllText($stderr)
        if ($process.ExitCode -ne 0 -or $text -notmatch 'UI V14 INPUT PASS' -or $errors -match 'ERROR|Exception|SCRIPT ERROR') { throw ($name+' falhou: '+$stdout) }
        Write-Host ('PASS '+$name)
    }
    foreach ($scale in @('1','1.5')) {
        $name = 'status-narrow-'+$scale.Replace('.','-')
        $stdout = Join-Path $qa ($name+'.log'); $stderr = Join-Path $qa ($name+'.err.log')
        $evidence = Join-Path $qa ($name+'.json'); $captures = Join-Path $qa $name
        $arguments = @('--path',('"'+$clientDir+'"'),'--','--mode=expedition','--overview','--qa-size=1366x900','--ui-check',('--ui-scale='+$scale),'--time-scale=2','--duration=55',('"--ui-evidence='+$evidence+'"'),('"--ui-captures='+$captures+'"'))
        $process = Start-Process -FilePath $godotRuntimeExe -ArgumentList $arguments -WindowStyle Hidden -WorkingDirectory $projectRoot -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
        $null = $process.Handle
        if (!$process.WaitForExit(65000)) { $process.Kill(); throw ($name+' excedeu 65s') }
        $process.WaitForExit(); $process.Refresh()
        $text = [IO.File]::ReadAllText($stdout); $errors = [IO.File]::ReadAllText($stderr)
        if ($process.ExitCode -ne 0 -or $text -notmatch 'UI V14 INPUT PASS' -or $errors -match 'ERROR|Exception|SCRIPT ERROR') { throw ($name+' falhou: '+$stdout) }
        Write-Host ('PASS '+$name)
    }
}
Write-Host "UI V18 STATUS PASS - summary, details, attribute explanation, shared selection and keyboard; evidence: $qa"
