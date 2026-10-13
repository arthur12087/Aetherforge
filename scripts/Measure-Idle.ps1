param([ValidateRange(10,3600)][int]$DurationSeconds=600)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\Environment.ps1"
$artifactDir = Join-Path $projectRoot 'artifacts'
New-Item -ItemType Directory -Force -Path $artifactDir | Out-Null
$duration = $DurationSeconds + 15
$process = Start-Process -FilePath $godotRuntimeExe -ArgumentList @('--path',"`"$clientDir`"",'--','--mode=idle',"--duration=$duration") -WindowStyle Hidden -PassThru -RedirectStandardOutput "$artifactDir\idle-performance.log" -RedirectStandardError "$artifactDir\idle-performance.err.log"
$null = $process.Handle
try {
    Start-Sleep -Seconds 5
    $process.Refresh()
    $initialCpu = $process.TotalProcessorTime.TotalSeconds
    $startTime = [DateTime]::UtcNow
    $peakWorkingSet = $process.WorkingSet64
    $samples = 0
    while (([DateTime]::UtcNow - $startTime).TotalSeconds -lt $DurationSeconds) {
        Start-Sleep -Seconds 1
        $process.Refresh()
        if ($process.HasExited) { throw 'Idle encerrou antes da medicao.' }
        $peakWorkingSet = [Math]::Max($peakWorkingSet,$process.WorkingSet64)
        $samples++
        if ($samples % 30 -eq 0) { Write-Host "Medicao idle: $samples s" }
    }
    $elapsed = ([DateTime]::UtcNow - $startTime).TotalSeconds
    $cpuPercent = 100 * ($process.TotalProcessorTime.TotalSeconds - $initialCpu) / $elapsed / [Environment]::ProcessorCount
    $report = [ordered]@{
        seconds = [Math]::Round($elapsed,2)
        averageCpuPercentOfMachine = [Math]::Round($cpuPercent,3)
        peakWorkingSetMB = [Math]::Round($peakWorkingSet / 1MB,2)
        logicalProcessors = [Environment]::ProcessorCount
        windows = (Get-CimInstance Win32_OperatingSystem).Caption
        osBuild = [Environment]::OSVersion.Version.ToString()
        gpu = @((Get-CimInstance Win32_VideoController).Name)
        fpsLimit = 30
        build = 'Godot .NET debug, editor executable; not exported release'
        fullTenMinuteRun = ($DurationSeconds -ge 600)
    }
    $report | ConvertTo-Json -Depth 3 | Set-Content "$artifactDir\idle-performance.json" -Encoding UTF8
    $report | ConvertTo-Json -Depth 3
} finally { if (-not $process.HasExited) { Stop-Process -Id $process.Id } }
