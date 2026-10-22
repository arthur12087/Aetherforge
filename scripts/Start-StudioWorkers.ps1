param([string[]]$Workers = @('regras','cena','interface'))
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$codex = (Get-Command codex.exe -ErrorAction Stop).Source
$artifacts = Join-Path $root 'artifacts\studio-godot\agents'
New-Item -ItemType Directory -Force $artifacts | Out-Null
$started = @()
foreach ($worker in $Workers) {
    if ($worker -notin @('regras','cena','interface')) { throw "Worker desconhecido: $worker" }
    $prompt = Join-Path $root "docs\studio-godot\tasks\$worker.txt"
    $arguments = @('exec','--sandbox','workspace-write','--ephemeral','-C',"`"$root`"",'--json','--color','never','-o',"`"$artifacts\$worker-last.md`"")
    if ($worker -ne 'regras') {
        foreach ($image in @('01-equipe-r02.png','02-bestiario-1.png','03-bestiario-2.png')) {
            $arguments += @('-i',"`"$root\assets\concepts\brass-v12-gpt\generated\$image`"")
        }
        if ($worker -eq 'interface') { $arguments += @('-i',"`"$root\assets\concepts\brass-v10\01-menus-cor.png`"") }
    }
    $arguments += '-'
    $child = Start-Process -FilePath $codex -ArgumentList $arguments -WorkingDirectory $root -RedirectStandardInput $prompt -RedirectStandardOutput "$artifacts\$worker.jsonl" -RedirectStandardError "$artifacts\$worker.stderr.log" -PassThru -WindowStyle Hidden
    $started += [pscustomobject]@{ worker=$worker; pid=$child.Id; startedUtc=[DateTime]::UtcNow.ToString('o'); sandbox='workspace-write'; prompt="docs/studio-godot/tasks/$worker.txt" }
}
$started | ConvertTo-Json -Depth 4 | Set-Content -Encoding UTF8 "$artifacts\processes.json"
$started | Format-Table -AutoSize
