param([ValidateSet('Idle','Server','Map','Api','Demo')][string]$Mode='Idle')
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\Environment.ps1"
Push-Location $projectRoot
try {
    if ($Mode -eq 'Api') {
        & $dotnetExe run --no-build --project 'src\Aetherforge.Api\Aetherforge.Api.csproj'
    } elseif ($Mode -eq 'Demo') {
        $artifactDir = Join-Path $projectRoot 'artifacts'
        New-Item -ItemType Directory -Force -Path $artifactDir | Out-Null
        # A server helper is hidden. The two interactive client windows are intentionally visible.
        $server = Start-Process -FilePath $godotRuntimeExe -ArgumentList @('--headless','--path',"`"$clientDir`"",'--','--mode=server') -WindowStyle Hidden -PassThru -RedirectStandardOutput "$artifactDir\demo-server.log" -RedirectStandardError "$artifactDir\demo-server.err.log"
        try {
            Start-Sleep -Milliseconds 1500
            $one = Start-Process -FilePath $godotRuntimeExe -ArgumentList @('--path',"`"$clientDir`"",'--position','40,60','--','--mode=map') -PassThru
            $two = Start-Process -FilePath $godotRuntimeExe -ArgumentList @('--path',"`"$clientDir`"",'--position','620,160','--','--mode=map') -PassThru
            $one.WaitForExit(); $two.WaitForExit()
        } finally { if (-not $server.HasExited) { Stop-Process -Id $server.Id } }
    } else {
        $arguments = @('--path',$clientDir)
        if ($Mode -eq 'Server') { $arguments = @('--headless') + $arguments }
        & $godotExe @arguments -- "--mode=$($Mode.ToLowerInvariant())"
    }
} finally { Pop-Location }
