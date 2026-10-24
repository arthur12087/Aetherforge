param([switch]$Screenshots, [string]$OutputDirectory = 'artifacts\ui-v15\qa')
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Environment.ps1')
# Shared input/layout suite also exercises the new Status member slips, with isolated V15 output.
$qa = if ([IO.Path]::IsPathRooted($OutputDirectory)) { $OutputDirectory } else { Join-Path $projectRoot $OutputDirectory }
& (Join-Path $PSScriptRoot 'Test-UiV14.ps1') -Screenshots:$Screenshots -OutputDirectory $qa
$cases = @(
    @{ Name='components-1'; Scale='1'; Graphics=$false; Reduced=$false },
    @{ Name='components-125'; Scale='1.25'; Graphics=$false; Reduced=$false },
    @{ Name='components-150'; Scale='1.5'; Graphics=$false; Reduced=$false },
    @{ Name='components-reduced'; Scale='1'; Graphics=$false; Reduced=$true }
)
if ($Screenshots) {
    $cases += @{ Name='components-native'; Scale='1'; Graphics=$true; Reduced=$false }
    $cases += @{ Name='components-native-150'; Scale='1.5'; Graphics=$true; Reduced=$false }
}
foreach ($case in $cases) {
    $stdout = Join-Path $qa ($case.Name+'.log'); $stderr = Join-Path $qa ($case.Name+'.err.log')
    $evidence = Join-Path $qa ($case.Name+'.json'); $captures = Join-Path $qa $case.Name
    $arguments = @('--path',('"'+$clientDir+'"'))
    if (!$case.Graphics) { $arguments += '--headless' }
    $arguments += @('--','--mode=expedition','--component-check',('--ui-scale='+$case.Scale),'--duration=35',('"--ui-evidence='+$evidence+'"'),('"--ui-captures='+$captures+'"'))
    if ($case.Reduced) { $arguments += '--reduce-ui-motion' }
    $process = Start-Process -FilePath $godotRuntimeExe -ArgumentList $arguments -WorkingDirectory $projectRoot -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
    $null = $process.Handle
    if (!$process.WaitForExit(45000)) { $process.Kill(); throw ($case.Name+' excedeu 45s') }
    $process.WaitForExit(); $process.Refresh()
    $text = [IO.File]::ReadAllText($stdout); $errors = [IO.File]::ReadAllText($stderr)
    if ($process.ExitCode -ne 0 -or $text -notmatch 'UI V15 COMPONENT PASS' -or $errors -match 'ERROR|Exception|SCRIPT ERROR') { throw ($case.Name+' falhou: '+$stdout) }
    Write-Host ('PASS '+$case.Name)
}
Write-Host "UI V15 PASS - input real automatizado + fixtures de componentes; evidence: $qa"
