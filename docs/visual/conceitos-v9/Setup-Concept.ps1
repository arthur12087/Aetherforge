param()
$ErrorActionPreference = 'Stop'
$Root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$Target = Join-Path $Root '.tools/concept-v9/three'
New-Item -ItemType Directory -Force $Target | Out-Null
$Version = '0.180.0'
foreach ($File in @('three.module.min.js', 'three.core.min.js')) {
    Invoke-WebRequest -UseBasicParsing -Uri "https://unpkg.com/three@$Version/build/$File" -OutFile (Join-Path $Target $File)
}
Invoke-WebRequest -UseBasicParsing -Uri "https://unpkg.com/three@$Version/LICENSE" -OutFile (Join-Path $Target 'LICENSE.txt')
Write-Output "Three.js $Version (MIT) preparado em .tools/concept-v9/three. Apenas geradores de conceito; runtime do jogo inalterado."
