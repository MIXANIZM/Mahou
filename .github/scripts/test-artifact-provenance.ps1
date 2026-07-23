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
