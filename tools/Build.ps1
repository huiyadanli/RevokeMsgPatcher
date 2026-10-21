param(
    [string]$TargetFrameworkVersion = 'v4.8',
    [string]$QQPath,
    [string]$WeixinPath
)
$ErrorActionPreference = 'Stop'
if ([bool]$QQPath -ne [bool]$WeixinPath) { throw 'QQPath and WeixinPath must be supplied together.' }
$repo = Split-Path $PSScriptRoot -Parent
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (!(Test-Path -LiteralPath $vswhere)) { throw 'Install Visual Studio Build Tools with the .NET desktop build workload.' }
$msbuild = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (!$msbuild) { throw 'MSBuild was not found.' }
function Invoke-Build([string]$project, [switch]$Restore) {
    $buildArgs = @($project, '/nologo', '/v:minimal', '/p:Configuration=Release', "/p:TargetFrameworkVersion=$TargetFrameworkVersion")
    if ($Restore) { $buildArgs += '/restore', '/p:RestorePackagesConfig=true' }
    & $msbuild @buildArgs
    if ($LASTEXITCODE -ne 0) { throw "Build failed: $project" }
}
Push-Location $repo
try {
    Invoke-Build 'RevokeMsgPatcher.sln' -Restore
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'Update-PatchResources.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'Patch resource generation failed.' }
    # Rebuild the entire solution after regenerating embedded data.
    Invoke-Build 'RevokeMsgPatcher.sln'
    Invoke-Build 'RevokeMsgPatcher.Tests\RevokeMsgPatcher.Tests.csproj'
    $artifacts = Join-Path $repo 'artifacts'
    [void](New-Item -ItemType Directory -Path $artifacts -Force)
    $testExe = Join-Path $repo 'RevokeMsgPatcher.Tests\bin\Release\RevokeMsgPatcher.Tests.exe'
    $report = $null
    if ($QQPath) {
        $report = Join-Path $artifacts 'compatibility-report.json'
        & $testExe --local $QQPath $WeixinPath $report
    } else { & $testExe }
    if ($LASTEXITCODE -ne 0) { throw 'Verification failed; no release archive was produced.' }
    $package = Join-Path $artifacts ('LocalCompat-' + [Guid]::NewGuid().ToString('N').Substring(0, 8))
    [void](New-Item -ItemType Directory -Path $package)
    foreach ($pattern in @('RevokeMsgPatcher\bin\Release\*.exe', 'RevokeMsgPatcher\bin\Release\*.dll', 'RevokeMsgPatcher\bin\Release\*.config',
        'RevokeMsgPatcher.MultiInstance\bin\Release\*.exe', 'RevokeMsgPatcher.MultiInstance\bin\Release\*.config')) {
        Get-ChildItem -Path $pattern | Copy-Item -Destination $package
    }
    Copy-Item -LiteralPath README.md,LICENSE -Destination $package
    if ($report) { Copy-Item -LiteralPath $report -Destination $package }
    Get-ChildItem -LiteralPath $package -File | Get-FileHash -Algorithm SHA256 | ForEach-Object {
        $_.Hash.ToLowerInvariant() + '  ' + [IO.Path]::GetFileName($_.Path)
    } | Set-Content -LiteralPath (Join-Path $package 'SHA256SUMS.txt') -Encoding utf8
    $archive = Join-Path $artifacts ([IO.Path]::GetFileName($package) + '.zip')
    Compress-Archive -Path (Join-Path $package '*') -DestinationPath $archive
    Write-Output "Ready: $package"
    Write-Output "Archive: $archive"
} finally { Pop-Location }
