[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$ExpectedCommit,
    [Parameter(Mandatory = $true)][string]$ExpectedTree,
    [Parameter(Mandatory = $true)][ValidateSet('x86', 'x64')][string]$ExpectedPlatform,
    [Parameter(Mandatory = $true)][string]$ExpectedRepository,
    [Parameter(Mandatory = $true)][string]$ExpectedRuntimeVersion,
    [Parameter(Mandatory = $true)][string]$ZipPath
)

$ErrorActionPreference = 'Stop'

function Assert-FullSha {
    param([Parameter(Mandatory = $true)][string]$Name,
          [Parameter(Mandatory = $true)][string]$Value)
    if ($Value -notmatch '^[0-9a-fA-F]{40}$') {
        throw "$Name must be a full 40-character SHA: $Value"
    }
}

function Get-RelativeArtifactPath {
    param([Parameter(Mandatory = $true)][string]$Root,
          [Parameter(Mandatory = $true)][string]$Path)
    return $Path.Substring($Root.Length).TrimStart('\', '/').Replace('\', '/')
}

Assert-FullSha -Name 'ExpectedCommit' -Value $ExpectedCommit
Assert-FullSha -Name 'ExpectedTree' -Value $ExpectedTree
if ([string]::IsNullOrWhiteSpace($ExpectedRepository)) {
    throw 'ExpectedRepository must not be empty'
}
if ([string]::IsNullOrWhiteSpace($ExpectedRuntimeVersion)) {
    throw 'ExpectedRuntimeVersion must not be empty'
}
$expectedCommit = $ExpectedCommit.ToLowerInvariant()
$expectedTree = $ExpectedTree.ToLowerInvariant()
$resolvedZip = (Resolve-Path -LiteralPath $ZipPath).Path
if (-not (Test-Path -LiteralPath $resolvedZip -PathType Leaf)) {
    throw "ZIP not found: $ZipPath"
}

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('mahou-provenance-' + [Guid]::NewGuid().ToString('N'))
try {
    New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null
    Expand-Archive -LiteralPath $resolvedZip -DestinationPath $tempRoot -Force

    $manifests = @(Get-ChildItem -LiteralPath $tempRoot -Filter 'build-manifest.json' -File -Recurse)
    if ($manifests.Count -ne 1) {
        throw "Expected exactly one build-manifest.json, found $($manifests.Count)"
    }
    $artifactRoot = $manifests[0].Directory.FullName
    if (-not [System.IO.Path]::GetFullPath($artifactRoot).Equals(
            [System.IO.Path]::GetFullPath($tempRoot),
            [System.StringComparison]::OrdinalIgnoreCase)) {
        throw 'build-manifest.json must be at the archive root; nested or sibling content is forbidden'
    }
    $manifest = Get-Content -LiteralPath $manifests[0].FullName -Raw -Encoding UTF8 | ConvertFrom-Json

    $requiredFields = @(
        'product', 'runtime_version', 'source_repository', 'source_commit', 'source_tree',
        'source_ref', 'workflow_name', 'workflow_run_id', 'workflow_run_attempt', 'platform',
        'configuration', 'build_timestamp_utc', 'deterministic_build', 'security_regression_passed'
    )
    foreach ($field in $requiredFields) {
        if ($null -eq $manifest.$field -or [string]::IsNullOrWhiteSpace([string]$manifest.$field)) {
            throw "Manifest field is missing or empty: $field"
        }
    }

    if ([string]$manifest.source_commit -ne $expectedCommit) {
        throw "Source commit mismatch: expected $expectedCommit, manifest has $($manifest.source_commit)"
    }
    if ([string]$manifest.source_tree -ne $expectedTree) {
        throw "Source tree mismatch: expected $expectedTree, manifest has $($manifest.source_tree)"
    }
    if (-not [string]::Equals(
            [string]$manifest.platform,
            $ExpectedPlatform,
            [System.StringComparison]::Ordinal)) {
        throw "Platform mismatch: expected $ExpectedPlatform, manifest has $($manifest.platform)"
    }
    if (-not [string]::Equals(
            [string]$manifest.source_repository,
            $ExpectedRepository,
            [System.StringComparison]::Ordinal)) {
        throw "Source repository mismatch: expected $ExpectedRepository, manifest has $($manifest.source_repository)"
    }
    if (-not [string]::Equals(
            [string]$manifest.runtime_version,
            $ExpectedRuntimeVersion,
            [System.StringComparison]::Ordinal)) {
        throw "Runtime version mismatch: expected $ExpectedRuntimeVersion, manifest has $($manifest.runtime_version)"
    }
    if ($manifest.product -ne 'Mahou') { throw "Unexpected product: $($manifest.product)" }
    if ($manifest.configuration -ne 'Release') { throw "Unexpected configuration: $($manifest.configuration)" }
    if ($manifest.deterministic_build -ne $true) { throw 'Manifest does not prove a deterministic build' }
    if ($manifest.security_regression_passed -ne $true) { throw 'Manifest does not prove the security regression passed' }

    $sumsPath = Join-Path $artifactRoot 'SHA256SUMS.txt'
    if (-not (Test-Path -LiteralPath $sumsPath -PathType Leaf)) {
        throw 'SHA256SUMS.txt is missing'
    }

    $listed = @{}
    foreach ($line in Get-Content -LiteralPath $sumsPath -Encoding UTF8) {
        if ([string]::IsNullOrWhiteSpace($line)) { continue }
        if ($line -notmatch '^([0-9a-fA-F]{64}) \*(.+)$') {
            throw "Invalid SHA256SUMS.txt line: $line"
        }
        $expectedHash = $Matches[1].ToLowerInvariant()
        $relative = $Matches[2].Replace('\', '/')
        if ($relative.StartsWith('/') -or $relative.Contains('../') -or $relative.Contains('/..')) {
            throw "Unsafe path in SHA256SUMS.txt: $relative"
        }
        if ($listed.ContainsKey($relative)) { throw "Duplicate SHA entry: $relative" }
        $filePath = [System.IO.Path]::GetFullPath((Join-Path $artifactRoot $relative))
        $rootPrefix = $artifactRoot.TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar
        if (-not $filePath.StartsWith($rootPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "SHA entry escapes artifact root: $relative"
        }
        if (-not (Test-Path -LiteralPath $filePath -PathType Leaf)) { throw "Hashed file is missing: $relative" }
        $actualHash = (Get-FileHash -LiteralPath $filePath -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($actualHash -ne $expectedHash) {
            throw "SHA-256 mismatch for $relative`: expected $expectedHash, found $actualHash"
        }
        $listed[$relative] = $true
    }

    $issuedFiles = @(Get-ChildItem -LiteralPath $artifactRoot -File -Recurse |
        Where-Object { $_.FullName -ne $sumsPath })
    foreach ($file in $issuedFiles) {
        $relative = Get-RelativeArtifactPath -Root $artifactRoot -Path $file.FullName
        if (-not $listed.ContainsKey($relative)) { throw "Issued file is not covered by SHA256SUMS.txt: $relative" }
    }
    if ($listed.Count -ne $issuedFiles.Count) {
        throw "SHA256SUMS coverage mismatch: $($listed.Count) entries for $($issuedFiles.Count) issued files"
    }

    $exePath = Join-Path $artifactRoot 'Mahou.exe'
    if (-not (Test-Path -LiteralPath $exePath -PathType Leaf)) { throw 'Mahou.exe is missing' }
    $versionInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($exePath)
    $runtimeNumeric = ([string]$manifest.runtime_version -split '-', 2)[0]
    if ([string]$versionInfo.FileVersion -ne $runtimeNumeric) {
        throw "Executable version mismatch: manifest runtime $($manifest.runtime_version), FileVersion $($versionInfo.FileVersion)"
    }

    $bytes = [System.IO.File]::ReadAllBytes($exePath)
    $ascii = [System.Text.Encoding]::ASCII.GetString($bytes)
    $unicode = [System.Text.Encoding]::Unicode.GetString($bytes)
    if (-not $ascii.Contains($expectedCommit) -and -not $unicode.Contains($expectedCommit)) {
        throw "Expected commit is not embedded in Mahou.exe: $expectedCommit"
    }

    $zipHash = (Get-FileHash -LiteralPath $resolvedZip -Algorithm SHA256).Hash.ToLowerInvariant()
    Write-Host 'Artifact provenance verification passed.'
    Write-Host "zip_sha256=$zipHash"
    Write-Host "mahou_exe_sha256=$((Get-FileHash -LiteralPath $exePath -Algorithm SHA256).Hash.ToLowerInvariant())"
    Write-Host "source_commit=$expectedCommit"
    Write-Host "source_tree=$expectedTree"
    Write-Host "source_repository=$ExpectedRepository"
    Write-Host "runtime_version=$($manifest.runtime_version)"
    Write-Host "platform=$($manifest.platform)"
}
finally {
    if (Test-Path -LiteralPath $tempRoot) {
        Remove-Item -LiteralPath $tempRoot -Recurse -Force
    }
}
