param([switch]$SkipAssets)
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$projectRoot = Split-Path $PSScriptRoot -Parent
$toolsDir = Join-Path $projectRoot '.tools'
New-Item -ItemType Directory -Force -Path $toolsDir | Out-Null
function Get-PortableArchive([string]$Url, [string]$File, [string]$Destination, [string]$Sha512 = '') {
    if (-not (Test-Path -LiteralPath $Destination)) {
        $archive = Join-Path $toolsDir $File
        if (-not (Test-Path -LiteralPath $archive)) { Invoke-WebRequest -Uri $Url -OutFile $archive -UseBasicParsing }
        if ($Sha512 -and (Get-FileHash -LiteralPath $archive -Algorithm SHA512).Hash.ToLowerInvariant() -ne $Sha512) {
            throw "Checksum invalido: $File. Remova apenas esse arquivo e tente novamente."
        }
        Expand-Archive -LiteralPath $archive -DestinationPath $Destination
    }
}
Get-PortableArchive 'https://builds.dotnet.microsoft.com/dotnet/Sdk/10.0.401/dotnet-sdk-10.0.401-win-x64.zip' 'dotnet-10.0.401.zip' (Join-Path $toolsDir 'dotnet') '24b670ad3d923bfcf47df6c3b034152398b42f6dbc388e10d783aee1cfb5e5817d399fc0ae2a12cfa822a55e61d34830ccb15c50ef6efee437ab874bb7c79430'
Get-PortableArchive 'https://github.com/godotengine/godot/releases/download/4.7.2-stable/Godot_v4.7.2-stable_mono_win64.zip' 'godot-4.7.2.zip' (Join-Path $toolsDir 'godot')
if (-not $SkipAssets) {
    Get-PortableArchive 'https://github.com/KayKit-Game-Assets/KayKit-Character-Pack-Adventures-1.0/archive/672074b73ba276876a19e8816ecdc5241817ab47.zip' 'kaykit-adventurers.zip' (Join-Path $toolsDir 'kaykit')
    $source = Get-ChildItem (Join-Path $toolsDir 'kaykit') -Recurse -File -Filter '*.glb'
    $assetsDir = Join-Path $projectRoot 'src\Aetherforge.Client\Assets\Originals\KayKit'
    New-Item -ItemType Directory -Force -Path $assetsDir | Out-Null
    foreach ($name in @('Knight.glb','Rogue.glb','Mage.glb')) {
        $asset = $source | Where-Object Name -eq $name | Select-Object -First 1
        if (-not $asset) { throw "Asset nao encontrado no pacote: $name" }
        Copy-Item -LiteralPath $asset.FullName -Destination (Join-Path $assetsDir $name)
        Get-ChildItem $asset.DirectoryName -File -Filter '*_texture.png' | ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $assetsDir $_.Name) }
    }
    Get-ChildItem (Join-Path $toolsDir 'kaykit') -Recurse -File -Filter 'LICENSE.txt' | Select-Object -First 1 | ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $assetsDir 'LICENSE.txt') }
}
Write-Host 'Ferramentas portateis prontas em .tools; nenhuma instalacao global foi alterada.'
