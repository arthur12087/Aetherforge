param([ValidateSet('tema','hud','menus')][string[]]$Workers = @('tema','hud','menus'))
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$command = Get-Command codex.exe -ErrorAction SilentlyContinue
$codex = if ($command) { $command.Source } else { Join-Path $env:LOCALAPPDATA 'Programs\OpenAI\Codex\bin\codex.exe' }
if (-not (Test-Path -LiteralPath $codex)) { throw 'Codex CLI nao encontrado. Nenhum worker iniciado.' }
$artifacts = Join-Path $root 'artifacts\ui-v14\agents'
New-Item -ItemType Directory -Force $artifacts | Out-Null
$started = @()
foreach ($worker in $Workers) {
    $prompt = Join-Path $root "docs\studio-godot\tasks\ui-v14-$worker.txt"
    $arguments = @('exec','--sandbox','workspace-write','--ephemeral','-C',"`"$root`"",'--json','--color','never','-o',"`"$artifacts\$worker-last.md`"")
    foreach ($image in @('assets/concepts/brass-v13-godot/boss.png','assets/concepts/ui-v14-planejamento/referencia-hud-usuario.png')) {
        $arguments += @('-i',"`"$root/$image`"")
    }
    $arguments += '-'
    $child = Start-Process -FilePath $codex -ArgumentList $arguments -WorkingDirectory $root -RedirectStandardInput $prompt -RedirectStandardOutput "$artifacts\$worker.jsonl" -RedirectStandardError "$artifacts\$worker.stderr.log" -PassThru -WindowStyle Hidden
    $started += [pscustomobject]@{worker=$worker;pid=$child.Id;startedUtc=[DateTime]::UtcNow.ToString('o');sandbox='workspace-write';prompt="docs/studio-godot/tasks/ui-v14-$worker.txt"}
}
$started | ConvertTo-Json -Depth 4 | Set-Content -Encoding UTF8 "$artifacts\processes.json"
$started | Format-Table -AutoSize
