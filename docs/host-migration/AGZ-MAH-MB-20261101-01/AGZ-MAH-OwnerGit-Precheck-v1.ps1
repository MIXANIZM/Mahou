#requires -Version 5.1
[CmdletBinding()]
param([switch]$SelfTest)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$sourcePath = 'W:\AGZ\Repositories\Mahou'
$targetPath = 'W:\AGZ\Projects\mahou\Repository'
$taskDirectory = 'W:\AGZ\Temp\Projects\mahou\AGZ-MAH-MB-20261101-01'
$expectedPrincipal = 'HUAWEI-ULTRA-7\user'

function Invoke-ReadOnlyGit {
    param([string[]]$gitArgumentList, [int]$maxSeconds = 60)
    $gitExe = (Get-Command git.exe -ErrorAction Stop).Source
    $startInfo = New-Object System.Diagnostics.ProcessStartInfo
    $startInfo.FileName = $gitExe
    $startInfo.Arguments = $gitArgumentList -join ' '
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $startInfo
    [void]$process.Start()
    $outTask = $process.StandardOutput.ReadToEndAsync()
    $errTask = $process.StandardError.ReadToEndAsync()
    $finished = $process.WaitForExit($maxSeconds * 1000)
    if (-not $finished) {
        try { $process.Kill() } catch {}
        [void]$process.WaitForExit(5000)
    }
    $stdoutValue = $outTask.GetAwaiter().GetResult()
    $stderrValue = $errTask.GetAwaiter().GetResult()
    $exitCodeValue = if ($finished) { $process.ExitCode } else { -9001 }
    $process.Dispose()
    return [pscustomobject]@{
        exit_code = $exitCodeValue
        stdout = $stdoutValue
        stderr = $stderrValue
        timed_out = (-not $finished)
    }
}

if ($SelfTest) {
    $testVersion = Invoke-ReadOnlyGit -gitArgumentList @('--version') -maxSeconds 10
    if ($testVersion.exit_code -ne 0 -or $testVersion.stdout -notmatch '^git version') {
        throw 'SELFTEST_GIT_VERSION_FAILED'
    }
    $invalid = Invoke-ReadOnlyGit -gitArgumentList @('--no-optional-locks','-C','W:\AGZ\Temp\Projects\mahou\__no_such_fixture__','rev-parse','HEAD') -maxSeconds 10
    if ($invalid.exit_code -eq 0) { throw 'SELFTEST_NONEXISTENT_REPO_UNEXPECTED_PASS' }
    Write-Output 'SELFTEST_PASS: git native read, exit and stderr handling; no source accessed'
    exit 0
}

$principal = [Security.Principal.WindowsIdentity]::GetCurrent().Name
if ($principal -ine $expectedPrincipal) { throw ('USER_AUTH_REQUIRED: '+$principal) }
if (-not (Test-Path -LiteralPath $sourcePath -PathType Container)) { throw 'SOURCE_NOT_FOUND' }
if (Test-Path -LiteralPath $targetPath) { throw 'TARGET_ALREADY_EXISTS_STOP' }
$sourceItem = Get-Item -LiteralPath $sourcePath -Force
if (($sourceItem.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) { throw 'SOURCE_REPARSE_STOP' }
if (-not (Test-Path -LiteralPath (Join-Path $sourcePath '.git') -PathType Container)) { throw 'DOT_GIT_NOT_DIRECTORY_STOP' }
if (Test-Path -LiteralPath $taskDirectory) {
    $taskItem = Get-Item -LiteralPath $taskDirectory -Force
    if (($taskItem.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) { throw 'TASK_DIR_REPARSE_STOP' }
} else {
    [void][IO.Directory]::CreateDirectory($taskDirectory)
}

$gitBaseArguments = @('--no-optional-locks','-c','core.fsmonitor=false','-C',$sourcePath)
$tests = @(
    [pscustomobject]@{name='HEAD';options=@('rev-parse','HEAD');limit=20},
    [pscustomobject]@{name='TOPLEVEL';options=@('rev-parse','--show-toplevel');limit=20},
    [pscustomobject]@{name='COMMON_DIR';options=@('rev-parse','--git-common-dir');limit=20},
    [pscustomobject]@{name='WORKTREES';options=@('worktree','list','--porcelain');limit=30},
    [pscustomobject]@{name='STATUS_ALL';options=@('status','--porcelain=v1','--untracked-files=all');limit=45},
    [pscustomobject]@{name='STASH';options=@('stash','list');limit=20},
    [pscustomobject]@{name='REFS';options=@('for-each-ref','--format=%(refname):%(objectname)');limit=35},
    [pscustomobject]@{name='FSCK';options=@('fsck','--full','--no-reflogs');limit=120}
)
$results = [ordered]@{}
$failedChecks = New-Object 'System.Collections.Generic.List[string]'
foreach ($test in $tests) {
    $nativeArguments = @($gitBaseArguments) + @($test.options)
    $result = Invoke-ReadOnlyGit -gitArgumentList $nativeArguments -maxSeconds $test.limit
    $results[$test.name] = $result
    Write-Output ('GIT_'+$test.name+'_EXIT='+$result.exit_code)
    if ($result.exit_code -ne 0) { [void]$failedChecks.Add($test.name) }
}
$report = [ordered]@{
    batch_id='AGZ-MAH-MB-20261101-01'
    operator=$principal
    observed_utc=[DateTime]::UtcNow.ToString('o')
    source=$sourcePath
    target=$targetPath
    target_did_not_exist=$true
    action='READ_ONLY_GIT_PRECHECK_ONLY'
    copy_performed=$false
    original_preserved_by_this_script=$true
    git_checks=$results
    checks_with_nonzero_exit=@($failedChecks.ToArray())
    status='GIT_READ_DONE_PENDING_FULL_SHA_DEPENDENCY_PRECHECK'
    five_evidence_created=$false
    kopia_started=$false
}
$reportPath = Join-Path $taskDirectory 'AGZ-MAH-OwnerGit-Precheck-v1.json'
$reportText = $report | ConvertTo-Json -Depth 8
[IO.File]::WriteAllText($reportPath,$reportText,(New-Object System.Text.UTF8Encoding($false)))
Write-Output ('RECEIPT='+$reportPath)
Write-Output ('RECEIPT_SHA256='+(Get-FileHash -LiteralPath $reportPath -Algorithm SHA256).Hash)
if ($failedChecks.Count -gt 0) {
    Write-Output ('GIT_READ_PARTIAL_OR_FAILED='+($failedChecks -join ','))
    exit 2
}
Write-Output 'GIT_READ_PASS_ONLY: full SHA-256, hardlink, reparse and consumer PRECHECK remain pending'
exit 0
