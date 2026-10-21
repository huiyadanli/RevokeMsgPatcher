param([string]$Configuration = 'Release')
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$main = Join-Path $repo "RevokeMsgPatcher\bin\$Configuration\RevokeMsgPatcher.exe"
$assistant = Join-Path $repo "RevokeMsgPatcher.Assistant\bin\$Configuration\RevokeMsgPatcher.Assistant.exe"
[void][Reflection.Assembly]::LoadFrom($main)
[void][Reflection.Assembly]::LoadFrom($assistant)
$json = (New-Object RevokeMsgPatcher.JsonData).BagJson()
$utf8 = New-Object Text.UTF8Encoding($false)
[IO.File]::WriteAllText((Join-Path $repo 'RevokeMsgPatcher.Assistant\Data\2.1\patch.json'), $json, $utf8)
$resxPath = Join-Path $repo 'RevokeMsgPatcher\Properties\Resources.resx'
$doc = New-Object Xml.XmlDocument
$doc.PreserveWhitespace = $true
$doc.Load($resxPath)
$doc.SelectSingleNode('/root/data[@name="PatchJson"]/value').InnerText = $json
$doc.Save($resxPath)
Write-Output 'Generated patch.json and embedded PatchJson from JsonData.cs.'
