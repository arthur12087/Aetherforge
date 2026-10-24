# Deterministic reference crops, not production art or source-file edits.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..'))
$dest = Join-Path $root 'assets\concepts\brass-v16-ficha\derived'
[IO.Directory]::CreateDirectory($dest) | Out-Null
function Save-Crops([string]$source, [hashtable]$crops) {
    $image = [Drawing.Bitmap]::FromFile((Join-Path $root $source))
    try {
        foreach ($entry in $crops.GetEnumerator()) {
            $v = $entry.Value
            $bounds = [Drawing.Rectangle]::new([int]$v[0], [int]$v[1], [int]$v[2], [int]$v[3])
            $copy = $image.Clone($bounds, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
            try {
                foreach ($fx in @(0.0, 0.25, 0.5, 0.75, 1.0)) {
                    foreach ($fy in @(0.0, 0.25, 0.5, 0.75, 1.0)) {
                        $cx = [int][Math]::Floor(($bounds.Width - 1) * $fx)
                        $cy = [int][Math]::Floor(($bounds.Height - 1) * $fy)
                        if ($copy.GetPixel($cx, $cy).ToArgb() -ne $image.GetPixel($bounds.X + $cx, $bounds.Y + $cy).ToArgb()) {
                            throw "Reference crop pixel mismatch: $($entry.Key)"
                        }
                    }
                }
                $copy.Save((Join-Path $dest ($entry.Key + '.png')), [Drawing.Imaging.ImageFormat]::Png)
            } finally { $copy.Dispose() }
        }
    } finally { $image.Dispose() }
}
Save-Crops 'assets\concepts\brass-v12-gpt\generated\01-equipe-r02.png' @{
    'guard' = @(555, 0, 565, 941)
    'guard-upper' = @(555, 0, 565, 750)
    'guard-face' = @(650, 0, 410, 455)
    'gunner-face' = @(130, 0, 350, 455)
    'healer-face' = @(1195, 0, 350, 455)
}
Save-Crops 'assets\concepts\brass-v13-godot\compact.png' @{
    'stage' = @(0, 62, 1000, 210)
}
Write-Output 'V16: six native-pixel reference crops / 150 pixel samples equal / originals unchanged.'
