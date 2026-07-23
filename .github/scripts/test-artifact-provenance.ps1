[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$ExpectedCommit,
    [Parameter(Mandatory = $true)][string]$ExpectedTree,
    [Parameter(Mandatory = $true)][string]$ZipPath,
    [string]$LegacyZipPath
)

$ErrorActionPreference = 'Stop'
$verifier = Join-Path $PSScriptRoot 'verify-artifact-provenance.ps1'

& $verifier -ExpectedCommit $ExpectedCommit -ExpectedTree $ExpectedTree -ZipPath $ZipPath

$wrongCommit = '0000000000000000000000000000000000000000'
if ($wrongCommit -eq $ExpectedCommit.ToLowerInvariant()) {
    $wrongCommit = 'ffffffffffffffffffffffffffffffffffffffff'
}
$wrongCommitRejected = $false
try {
    & $verifier -ExpectedCommit $wrongCommit -ExpectedTree $ExpectedTree -ZipPath $ZipPath
}
catch {
    $wrongCommitRejected = $true
    Write-Host "Expected negative result for wrong commit: $($_.Exception.Message)"
}
if (-not $wrongCommitRejected) {
    throw 'Provenance verifier accepted an intentionally wrong expected commit'
}

$layoutTestRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('mahou-provenance-layout-' + [Guid]::NewGuid().ToString('N'))
try {
    $nestedPackage = Join-Path $layoutTestRoot 'package'
    New-Item -ItemType Directory -Path $nestedPackage -Force | Out-Null
    Expand-Archive -LiteralPath $ZipPath -DestinationPath $nestedPackage -Force
    [System.IO.File]::WriteAllText((Join-Path $layoutTestRoot 'untracked-sibling.txt'), 'must be rejected')
    $malformedZip = Join-Path ([System.IO.Path]::GetTempPath()) ('mahou-provenance-malformed-' + [Guid]::NewGuid().ToString('N') + '.zip')
    Compress-Archive -Path (Join-Path $layoutTestRoot '*') -DestinationPath $malformedZip -CompressionLevel Optimal
    $malformedRejected = $false
    try {
        & $verifier -ExpectedCommit $ExpectedCommit -ExpectedTree $ExpectedTree -ZipPath $malformedZip
    }
    catch {
        $malformedRejected = $true
        Write-Host "Expected negative result for nested manifest with sibling content: $($_.Exception.Message)"
    }
    if (-not $malformedRejected) {
        throw 'Provenance verifier accepted nested manifest or sibling archive content'
    }
}
finally {
    if ($malformedZip -and (Test-Path -LiteralPath $malformedZip)) {
        Remove-Item -LiteralPath $malformedZip -Force
    }
    if (Test-Path -LiteralPath $layoutTestRoot) {
        Remove-Item -LiteralPath $layoutTestRoot -Recurse -Force
    }
}

if (-not [string]::IsNullOrWhiteSpace($LegacyZipPath)) {
    $legacyRejected = $false
    try {
        & $verifier -ExpectedCommit $ExpectedCommit -ExpectedTree $ExpectedTree -ZipPath $LegacyZipPath
    }
    catch {
        $legacyRejected = $true
        Write-Host "Expected negative result for legacy artifact: $($_.Exception.Message)"
    }
    if (-not $legacyRejected) {
        throw 'Provenance verifier accepted the legacy artifact for the current expected source'
    }
}

Write-Host 'Artifact provenance positive and negative regression tests passed.'
