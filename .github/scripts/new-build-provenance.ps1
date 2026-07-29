[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$ArtifactDirectory,
    [Parameter(Mandatory = $true)][ValidateSet('x86', 'x64')][string]$Platform,
    [Parameter(Mandatory = $true)][string]$SourceRepository,
    [Parameter(Mandatory = $true)][string]$SourceCommit,
    [Parameter(Mandatory = $true)][string]$SourceTree,
    [Parameter(Mandatory = $true)][string]$SourceRef,
    [Parameter(Mandatory = $true)][string]$WorkflowName,
    [Parameter(Mandatory = $true)][string]$WorkflowRunId,
    [Parameter(Mandatory = $true)][string]$WorkflowRunAttempt,
    [Parameter(Mandatory = $true)][string]$BuildTimestampUtc,
    [string]$RuntimeVersion = '2.9.0.1-dev',
    [string]$Configuration = 'Release',
    [switch]$DeterministicBuild,
    [switch]$SecurityRegressionPassed
)

$ErrorActionPreference = 'Stop'
$Utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)

function Write-Utf8File {
    param([Parameter(Mandatory = $true)][string]$Path,
          [Parameter(Mandatory = $true)][string]$Content)
    [System.IO.File]::WriteAllText($Path, $Content, $Utf8WithoutBom)
}

function Assert-Sha256 {
    param([Parameter(Mandatory = $true)][string]$Name,
          [Parameter(Mandatory = $true)][string]$Value)
    if ($Value -notmatch '^[0-9a-fA-F]{40}$') {
        throw "$Name must be a full 40-character Git commit SHA: $Value"
    }
}

$artifactRoot = [System.IO.Path]::GetFullPath($ArtifactDirectory)
if (-not (Test-Path -LiteralPath $artifactRoot -PathType Container)) {
    throw "Artifact directory not found: $artifactRoot"
}

Assert-Sha256 -Name 'SourceCommit' -Value $SourceCommit
Assert-Sha256 -Name 'SourceTree' -Value $SourceTree

foreach ($requiredFile in @('Mahou.exe', 'Mahou.exe.config')) {
    if (-not (Test-Path -LiteralPath (Join-Path $artifactRoot $requiredFile) -PathType Leaf)) {
        throw "Required artifact file is missing: $requiredFile"
    }
}

$sourceCommit = $SourceCommit.ToLowerInvariant()
$sourceTree = $SourceTree.ToLowerInvariant()
$manifestPath = Join-Path $artifactRoot 'build-manifest.json'
$sbomPath = Join-Path $artifactRoot 'sbom.cdx.json'
$sumsPath = Join-Path $artifactRoot 'SHA256SUMS.txt'

Remove-Item -LiteralPath $manifestPath,$sbomPath,$sumsPath -Force -ErrorAction SilentlyContinue

$payloadFiles = Get-ChildItem -LiteralPath $artifactRoot -File -Recurse |
    Sort-Object { $_.FullName.Substring($artifactRoot.Length).TrimStart('\', '/').Replace('\', '/') }
$manifestFiles = @()
foreach ($file in $payloadFiles) {
    $relative = $file.FullName.Substring($artifactRoot.Length).TrimStart('\', '/').Replace('\', '/')
    $hash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    $manifestFiles += [ordered]@{ path = $relative; sha256 = $hash }
}

$manifest = [ordered]@{
    schema_version = 2
    product = 'Mahou'
    runtime_version = $RuntimeVersion
    source_repository = $SourceRepository
    source_commit = $sourceCommit
    source_tree = $sourceTree
    source_ref = $SourceRef
    workflow_name = $WorkflowName
    workflow_run_id = [string]$WorkflowRunId
    workflow_run_attempt = [string]$WorkflowRunAttempt
    platform = $Platform
    configuration = $Configuration
    build_timestamp_utc = $BuildTimestampUtc
    deterministic_build = [bool]$DeterministicBuild
    security_regression_passed = [bool]$SecurityRegressionPassed
    target_framework = '.NET Framework 4.8'
    runtime_test_status = 'required before release'
    files = $manifestFiles
}
Write-Utf8File -Path $manifestPath -Content (($manifest | ConvertTo-Json -Depth 10) + "`n")

$sbom = [ordered]@{
    bomFormat = 'CycloneDX'
    specVersion = '1.5'
    version = 1
    metadata = [ordered]@{
        component = [ordered]@{
            type = 'application'
            name = 'Mahou'
            version = $RuntimeVersion
            properties = @(
                [ordered]@{ name = 'mixanizm:source-commit'; value = $sourceCommit },
                [ordered]@{ name = 'mixanizm:source-tree'; value = $sourceTree },
                [ordered]@{ name = 'mixanizm:platform'; value = $Platform },
                [ordered]@{ name = 'mixanizm:deterministic-rebuild'; value = ([bool]$DeterministicBuild).ToString().ToLowerInvariant() }
            )
        }
    }
    components = @(
        [ordered]@{ type = 'data'; name = 'Mahou AutoSwitch dictionary'; version = 'bundled'; scope = 'required' }
    )
}
Write-Utf8File -Path $sbomPath -Content (($sbom | ConvertTo-Json -Depth 10) + "`n")

$hashLines = @()
$issuedFiles = Get-ChildItem -LiteralPath $artifactRoot -File -Recurse |
    Where-Object { $_.FullName -ne $sumsPath } |
    Sort-Object { $_.FullName.Substring($artifactRoot.Length).TrimStart('\', '/').Replace('\', '/') }
foreach ($file in $issuedFiles) {
    $relative = $file.FullName.Substring($artifactRoot.Length).TrimStart('\', '/').Replace('\', '/')
    $hash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    $hashLines += "$hash *$relative"
}
Write-Utf8File -Path $sumsPath -Content (($hashLines -join "`n") + "`n")

Write-Host "Created immutable build provenance in $artifactRoot"
Write-Host "source_commit=$sourceCommit"
Write-Host "source_tree=$sourceTree"
Write-Host "platform=$Platform"
Write-Host "issued_files=$($issuedFiles.Count)"
