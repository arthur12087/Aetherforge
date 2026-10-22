param([switch]$Apply, [switch]$RestoreCanvas, [switch]$OpenEditor)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\Environment.ps1"
$projects = Join-Path $env:APPDATA 'ohmygame\data\projects'
$matches = @()
foreach ($directory in Get-ChildItem $projects -Directory -ErrorAction SilentlyContinue) {
    $file = Join-Path $directory.FullName 'project.json'
    if (-not (Test-Path -LiteralPath $file)) { continue }
    $text = [IO.File]::ReadAllText($file)
    try { $metadata = $text | ConvertFrom-Json } catch { continue }
    if ($metadata.workspacePath -and [IO.Path]::GetFullPath($metadata.workspacePath).TrimEnd('\') -eq [IO.Path]::GetFullPath($projectRoot).TrimEnd('\')) {
        $matches += [pscustomobject]@{ file=$file; text=$text; metadata=$metadata }
    }
}
if ($matches.Count -ne 1) { throw 'Nao foi encontrado exatamente um projeto OhMyGame para esta pasta. Nenhum cadastro foi alterado.' }
$entry = $matches[0]
$target = if ($RestoreCanvas) { 'asset-canvas' } else { 'godot-game' }
Write-Host "Projeto: $($entry.metadata.name)"
Write-Host "Tipo atual: $($entry.metadata.type) -> $target"
Write-Host "Cadastro: $($entry.file)"
Write-Host "Projeto Godot .NET: $clientDir\project.godot"
if ($Apply -and $entry.metadata.type -ne $target) {
    if (Get-Process -Name OhMyGame -ErrorAction SilentlyContinue) {
        throw 'Feche o OhMyGame antes de aplicar a migracao. Nao sera encerrado nenhum processo automaticamente.'
    }
    if ($entry.metadata.type -notin @('asset-canvas','godot-game') -or $entry.metadata.version -ne 1) {
        throw 'Cadastro com contrato inesperado; migracao recusada.'
    }
    $backupDirectory = Join-Path $projectRoot 'artifacts\studio-godot\migration'
    New-Item -ItemType Directory -Force $backupDirectory | Out-Null
    $backup = Join-Path $backupDirectory ('project-before-'+[DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss')+'-'+[Guid]::NewGuid().ToString('N')+'.json')
    [IO.File]::WriteAllText($backup,$entry.text,[Text.UTF8Encoding]::new($false))
    $entry.metadata.type = $target
    $entry.metadata.updatedAt = [DateTime]::UtcNow.ToString('o')
    $next = $entry.metadata | ConvertTo-Json -Depth 100
    if ([IO.File]::ReadAllText($entry.file) -ne $entry.text) { throw 'Cadastro mudou durante a operacao. Nenhuma alteracao aplicada.' }
    $temporary = $entry.file+'.studio.tmp'
    [IO.File]::WriteAllText($temporary,$next,[Text.UTF8Encoding]::new($false))
    Move-Item -LiteralPath $temporary -Destination $entry.file -Force
    Write-Host "APLICADO. Backup: $backup"
    Write-Host 'Reabra o OhMyGame. Canvas, historico da conversa e workspace foram preservados.'
} elseif (-not $Apply) {
    Write-Host 'DRY RUN: nenhum cadastro alterado. Feche o OhMyGame e use -Apply para aplicar.'
}
if ($OpenEditor) {
    Start-Process -FilePath $godotRuntimeExe -ArgumentList @('--editor','--path',"`"$clientDir`"") -WorkingDirectory $projectRoot | Out-Null
    Write-Host 'Godot .NET portatil aberto. Use F6/F5 para executar Main.tscn.'
}
