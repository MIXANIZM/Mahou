[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$ExpectedCommit,
    [Parameter(Mandatory = $true)][string]$ExpectedTree,
    [Parameter(Mandatory = $true)][ValidateSet('x86', 'x64')][string]$ExpectedPlatform,
    [Parameter(Mandatory = $true)][string]$ExpectedRepository,
    [Parameter(Mandatory = $true)][string]$ExpectedRuntimeVersion,
    [Parameter(Mandatory = $true)][string]$ZipPath,
    [string]$LegacyZipPath
)

$ErrorActionPreference = 'Stop'
$verifier = Join-Path $PSScriptRoot 'verify-artifact-provenance.ps1'

function Assert-ProvenanceRejected {
    param(
        [Parameter(Mandatory = $true)][string]$Case,
        [Parameter(Mandatory = $true)][string]$ExpectedMessage,
        [Parameter(Mandatory = $true)][scriptblock]$Verification
    )

    try {
        & $Verification
    }
    catch {
        if (-not $_.Exception.Message.Contains($ExpectedMessage)) {
            throw "$Case was rejected for an unexpected reason: $($_.Exception.Message)"
        }
        Write-Host "Expected negative result for $Case`: $($_.Exception.Message)"
        return
    }
    throw "Provenance verifier accepted $Case"
}

function Invoke-ExpectedVerification {
    param(
        [Parameter(Mandatory = $true)][string]$Commit,
        [Parameter(Mandatory = $true)][string]$Tree,
        [Parameter(Mandatory = $true)][string]$Platform,
        [Parameter(Mandatory = $true)][string]$Archive
    )
    & $verifier `
        -ExpectedCommit $Commit `
        -ExpectedTree $Tree `
        -ExpectedPlatform $Platform `
        -ExpectedRepository $ExpectedRepository `
        -ExpectedRuntimeVersion $ExpectedRuntimeVersion `
        -ZipPath $Archive
}

Invoke-ExpectedVerification `
    -Commit $ExpectedCommit `
    -Tree $ExpectedTree `
    -Platform $ExpectedPlatform `
    -Archive $ZipPath

$wrongCommit = '0000000000000000000000000000000000000000'
if ($wrongCommit -eq $ExpectedCommit.ToLowerInvariant()) {
    $wrongCommit = 'ffffffffffffffffffffffffffffffffffffffff'
}
$wrongTree = '0000000000000000000000000000000000000000'
if ($wrongTree -eq $ExpectedTree.ToLowerInvariant()) {
    $wrongTree = 'ffffffffffffffffffffffffffffffffffffffff'
}
$wrongPlatform = if ($ExpectedPlatform -eq 'x86') { 'x64' } else { 'x86' }

Assert-ProvenanceRejected `
    -Case 'an intentionally wrong expected commit' `
    -ExpectedMessage 'Source commit mismatch' `
    -Verification {
        Invoke-ExpectedVerification -Commit $wrongCommit -Tree $ExpectedTree -Platform $ExpectedPlatform -Archive $ZipPath
    }
Assert-ProvenanceRejected `
    -Case 'an intentionally wrong expected tree' `
    -ExpectedMessage 'Source tree mismatch' `
    -Verification {
        Invoke-ExpectedVerification -Commit $ExpectedCommit -Tree $wrongTree -Platform $ExpectedPlatform -Archive $ZipPath
    }
Assert-ProvenanceRejected `
    -Case 'an intentionally wrong expected platform' `
    -ExpectedMessage 'Platform mismatch' `
    -Verification {
        Invoke-ExpectedVerification -Commit $ExpectedCommit -Tree $ExpectedTree -Platform $wrongPlatform -Archive $ZipPath
    }

$layoutTestRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('mahou-provenance-layout-' + [Guid]::NewGuid().ToString('N'))
$malformedZip = $null
try {
    $nestedPackage = Join-Path $layoutTestRoot 'package'
    New-Item -ItemType Directory -Path $nestedPackage -Force | Out-Null
    Expand-Archive -LiteralPath $ZipPath -DestinationPath $nestedPackage -Force
    [System.IO.File]::WriteAllText((Join-Path $layoutTestRoot 'untracked-sibling.txt'), 'must be rejected')
    $malformedZip = Join-Path ([System.IO.Path]::GetTempPath()) ('mahou-provenance-malformed-' + [Guid]::NewGuid().ToString('N') + '.zip')
    Compress-Archive -Path (Join-Path $layoutTestRoot '*') -DestinationPath $malformedZip -CompressionLevel Optimal
    Assert-ProvenanceRejected `
        -Case 'nested manifest or sibling archive content' `
        -ExpectedMessage 'build-manifest.json must be at the archive root' `
        -Verification {
            Invoke-ExpectedVerification -Commit $ExpectedCommit -Tree $ExpectedTree -Platform $ExpectedPlatform -Archive $malformedZip
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

$tamperTestRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('mahou-provenance-tamper-' + [Guid]::NewGuid().ToString('N'))
$tamperedZip = $null
try {
    New-Item -ItemType Directory -Path $tamperTestRoot -Force | Out-Null
    Expand-Archive -LiteralPath $ZipPath -DestinationPath $tamperTestRoot -Force
    $tamperTarget = Join-Path $tamperTestRoot 'Mahou.exe.config'
    $tamperedBytes = [System.IO.File]::ReadAllBytes($tamperTarget)
    if ($tamperedBytes.Length -eq 0) {
        throw 'Cannot run byte-tamper regression against an empty Mahou.exe.config'
    }
    $tamperIndex = [Math]::Floor($tamperedBytes.Length / 2)
    $tamperedBytes[$tamperIndex] = [byte]($tamperedBytes[$tamperIndex] -bxor 1)
    [System.IO.File]::WriteAllBytes($tamperTarget, $tamperedBytes)
    $tamperedZip = Join-Path ([System.IO.Path]::GetTempPath()) ('mahou-provenance-tampered-' + [Guid]::NewGuid().ToString('N') + '.zip')
    Compress-Archive -Path (Join-Path $tamperTestRoot '*') -DestinationPath $tamperedZip -CompressionLevel Optimal
    Assert-ProvenanceRejected `
        -Case 'a one-byte Mahou.exe.config modification' `
        -ExpectedMessage 'SHA-256 mismatch for Mahou.exe.config' `
        -Verification {
            Invoke-ExpectedVerification -Commit $ExpectedCommit -Tree $ExpectedTree -Platform $ExpectedPlatform -Archive $tamperedZip
        }
}
finally {
    if ($tamperedZip -and (Test-Path -LiteralPath $tamperedZip)) {
        Remove-Item -LiteralPath $tamperedZip -Force
    }
    if (Test-Path -LiteralPath $tamperTestRoot) {
        Remove-Item -LiteralPath $tamperTestRoot -Recurse -Force
    }
}

if (-not [string]::IsNullOrWhiteSpace($LegacyZipPath)) {
    Assert-ProvenanceRejected `
        -Case 'the legacy artifact for the current expected source' `
        -ExpectedMessage 'Manifest field is missing or empty: runtime_version' `
        -Verification {
            Invoke-ExpectedVerification -Commit $ExpectedCommit -Tree $ExpectedTree -Platform $ExpectedPlatform -Archive $LegacyZipPath
        }
}

Write-Host 'Artifact provenance positive and negative regression tests passed.'
