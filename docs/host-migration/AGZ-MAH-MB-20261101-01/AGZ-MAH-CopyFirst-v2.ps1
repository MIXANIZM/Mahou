#requires -Version 5.1
[CmdletBinding()]
param([switch]$SelfTest)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$batch='AGZ-MAH-MB-20261101-01'
$source='W:\AGZ\Repositories\Mahou'
$target='W:\AGZ\Projects\mahou\Repository'
$temp='W:\AGZ\Temp\Projects\mahou\AGZ-MAH-MB-20261101-01'
$ev='W:\AGZ\Projects\mahou\Evidence\Migration\AGZ-MAH-MB-20261101-01'
$owner='HUAWEI-ULTRA-7\user'
$baselineSHA='5D28EA9D3A846067DA5C7374B1643FD59640CCB46E5EAAC873D76ACDC7250005'
$ownerSHA='F1CD82AB4EC37612A13D5BE05878914C62E2940D9131344DCF8378DA65698827'
$head='5527f662cb844ba90d264f5b93cb27f8334bf295'
$lastStep='START'
$copyStarted=$false

function HashFile([string]$p) { return (Get-FileHash -LiteralPath $p -Algorithm SHA256 -ErrorAction Stop).Hash.ToUpperInvariant() }
function CheckDirectory([string]$p) {
  $i=Get-Item -LiteralPath $p -Force -ErrorAction Stop
  if (-not $i.PSIsContainer -or ($i.Attributes -band [IO.FileAttributes]::ReparsePoint)) {throw ('UNSAFE_DIRECTORY:'+ $p)}
}
function EnumerateFiles([string]$root) {
  $stack=New-Object 'System.Collections.Generic.Stack[string]'
  $files=New-Object 'System.Collections.Generic.List[object]'
  $stack.Push($root);$dirs=0
  while($stack.Count -gt 0){
    $d=$stack.Pop();CheckDirectory $d;$dirs++
    if($dirs -gt 1000){throw 'DIR_LIMIT'}
    foreach($i in @(Get-ChildItem -LiteralPath $d -Force -ErrorAction Stop)){
      if($i.Attributes -band [IO.FileAttributes]::ReparsePoint){throw ('REPARSE_FOUND:'+ $i.FullName)}
      if($i.PSIsContainer){$stack.Push($i.FullName)}
      else { [void]$files.Add($i);if($files.Count -gt 1000){throw 'FILE_LIMIT'} }
    }
  }
  return $files.ToArray()
}
function CheckTree([string]$root,[object[]]$baseline,[string]$phase) {
  $actual=@(EnumerateFiles $root)
  if($actual.Count -ne $baseline.Count){throw ($phase+'_COUNT_'+$actual.Count+'_EXPECTED_'+$baseline.Count)}
  $dict=@{}
  foreach($row in $baseline){
    $key=[string]$row.relative_path
    if($key.Contains('..') -or $key.StartsWith('\') -or $key.Contains('/')){throw 'UNSAFE_RELATIVE_PATH'}
    if($dict.ContainsKey($key)){throw 'DUPLICATE_BASELINE_FILE'}
    $dict[$key]=$row
  }
  foreach($i in $actual){
    $rel=$i.FullName.Substring($root.Length).TrimStart('\')
    if(-not $dict.ContainsKey($rel)){throw ($phase+'_UNEXPECTED_FILE:'+ $rel)}
    $b=$dict[$rel]
    if([long]$b.size_bytes -ne [long]$i.Length){throw ($phase+'_SIZE_CHANGE:'+ $rel)}
    $beforeTicks=$i.LastWriteTimeUtc.Ticks
    if((HashFile $i.FullName) -ne [string]$b.sha256){throw ($phase+'_SHA_CHANGE:'+ $rel)}
    $after=Get-Item -LiteralPath $i.FullName -Force
    if($after.LastWriteTimeUtc.Ticks -ne $beforeTicks -or $after.Length -ne $i.Length){throw ($phase+'_CONCURRENT_CHANGE:'+ $rel)}
    if($phase -eq 'SOURCE_PRE'){
      $streams=@(Get-Item -LiteralPath $i.FullName -Stream '*' -ErrorAction Stop|Where-Object {$_.Stream -ne ':$DATA' -and $_.Stream -ne '$DATA'})
      if($streams.Count){throw ('NEW_ADS:'+ $rel)}
    }
  }
  return $actual.Count
}
function ReadGit([string]$where,[string[]]$gitArgs){
  $argsList=@('--no-optional-locks','-c','core.fsmonitor=false','-C',$where)+$gitArgs
  $v=@(& git.exe @argsList 2>&1);$exit=$LASTEXITCODE
  if($exit -ne 0){throw ('GIT_EXIT_'+$exit+':'+($gitArgs -join ' '))}
  return ([string]::Join([char]10,$v)).Trim()
}
function RobocopyOnly([string]$src,[string]$dest,[string]$log){
  if(Test-Path -LiteralPath $dest){throw 'COPY_TARGET_ALREADY_EXISTS'}
  $argsList=@($src,$dest,'/E','/COPY:DAT','/DCOPY:DAT','/R:0','/W:0','/XJ','/NP','/NFL','/NDL','/NJH','/NJS',('/LOG:'+$log))
  & "$env:SystemRoot\System32\robocopy.exe" @argsList | Out-Null
  $code=$LASTEXITCODE
  if($code -ge 8){throw ('ROBOCOPY_EXIT_'+$code)}
  return $code
}

if($SelfTest){
  CheckDirectory $temp
  $base=Join-Path $temp 'fixture-copyfirst-v2'
  $src=Join-Path $base 'source'
  $dest=Join-Path $base 'target'
  if(Test-Path -LiteralPath $dest){throw 'FIXTURE_ALREADY_RAN_NO_OVERWRITE'}
  if(-not(Test-Path -LiteralPath $src)){[void][IO.Directory]::CreateDirectory($src)}
  [IO.File]::WriteAllText((Join-Path $src 'fixture.txt'),'MAHOU_COPY_FIXTURE',[Text.Encoding]::UTF8)
  $code=RobocopyOnly $src $dest (Join-Path $base 'robocopy-fixture.log')
  if((HashFile (Join-Path $src 'fixture.txt')) -ne (HashFile (Join-Path $dest 'fixture.txt'))){throw 'FIXTURE_HASH_FAIL'}
  Write-Output ('SELFTEST_PASS_ROBOCOPY_EXIT='+$code)
    $f=Get-Item -LiteralPath (Join-Path $dest 'fixture.txt')
  $fixtureRow=[pscustomobject]@{relative_path='fixture.txt';size_bytes=$f.Length;sha256=(HashFile $f.FullName)}
  if((CheckTree $dest @($fixtureRow) 'FIXTURE_TARGET') -ne 1){throw 'FIXTURE_TREE_COUNT_FAIL'}
  Write-Output 'SELFTEST_PASS_SHA256'
  exit 0
}

try {
  if([Security.Principal.WindowsIdentity]::GetCurrent().Name -ine $owner){throw 'USER_AUTH_REQUIRED'}
  CheckDirectory $source; CheckDirectory $temp; CheckDirectory 'W:\AGZ\Projects'
  if(Test-Path -LiteralPath 'W:\AGZ\Projects\mahou'){throw 'TARGET_PARENT_EXISTS_STOP'}
  if(Test-Path -LiteralPath $target){throw 'TARGET_EXISTS_STOP'}
  CheckDirectory (Join-Path $source '.git')
  if((Get-PSDrive -Name W).Free -lt 250MB){throw 'NO_DISK_CAPACITY'}
  $baseFile=Join-Path $temp 'AGZ-MAH-source-hash-baseline-v1.csv'
  $ownerFile=Join-Path $temp 'AGZ-MAH-OwnerGit-Precheck-v1.json'
  if((HashFile $baseFile) -ne $baselineSHA -or (HashFile $ownerFile) -ne $ownerSHA){throw 'EVIDENCE_SHA_MISMATCH'}
  $baseline=@(Import-Csv -LiteralPath $baseFile)
  if($baseline.Count -ne 391){throw 'BASELINE_COUNT_INCORRECT'}
  if(@($baseline|Where-Object {[int]$_.hardlink_alias_count -ne 1 -or [int]$_.other_stream_count -ne 0 -or [int]$_.hardlink_scan_exit -ne 0}).Count){throw 'UNSAFE_LINKS_OR_ADS_IN_BASELINE'}
  $receipt=Get-Content -LiteralPath $ownerFile -Raw|ConvertFrom-Json
  if($receipt.operator -ine $owner -or @($receipt.checks_with_nonzero_exit).Count -gt 0){throw 'OWNER_GIT_RECEIPT_REJECTED'}
  $lastStep='SOURCE_PRE_SHA'
  Write-Output ('PHASE_STARTED='+$lastStep)
  $count=CheckTree $source $baseline 'SOURCE_PRE'
  Write-Output ('PHASE_PASS='+$lastStep+';FILES='+$count)
  $lastStep='SOURCE_PRE_GIT'
  Write-Output ('PHASE_STARTED='+$lastStep)
  if((ReadGit $source @('rev-parse','HEAD')) -ne $head){throw 'HEAD_CHANGED'}
  if((ReadGit $source @('rev-parse','--git-common-dir')) -ne '.git'){throw 'COMMON_DIR_CHANGED'}
  $oldWorktrees=ReadGit $source @('worktree','list','--porcelain')
  if($oldWorktrees -ne $receipt.git_checks.WORKTREES.stdout.Trim()){throw 'WORKTREES_CHANGED'}
  $oldStatus=ReadGit $source @('status','--porcelain=v1','--untracked-files=all')
  if($oldStatus -ne $receipt.git_checks.STATUS_ALL.stdout.Trim()){throw 'DIRTY_STATUS_CHANGED'}
  $oldRefs=ReadGit $source @('for-each-ref','--format=%(refname):%(objectname)')
  if($oldRefs -ne $receipt.git_checks.REFS.stdout.Trim()){throw 'GIT_REFS_CHANGED'}
  $oldStash=ReadGit $source @('stash','list')
  if($oldStash -ne $receipt.git_checks.STASH.stdout.Trim()){throw 'GIT_STASH_CHANGED'}
  $oldFsck=ReadGit $source @('fsck','--full','--no-reflogs')
  Write-Output ('PHASE_PASS='+$lastStep)
  $lastStep='COPY_FIRST'
  Write-Output ('PHASE_STARTED='+$lastStep)
  $copyStarted=$true
  $code=RobocopyOnly $source $target (Join-Path $temp 'AGZ-MAH-CopyFirst-v2-robocopy.log')
  Write-Output ('PHASE_PASS='+$lastStep+';ROBOCOPY_EXIT='+$code)
  $lastStep='SOURCE_POST_AND_TARGET_SHA'
  Write-Output ('PHASE_STARTED='+$lastStep)
  $sourceAfter=CheckTree $source $baseline 'SOURCE_POST'
  $targetCount=CheckTree $target $baseline 'TARGET'
  Write-Output ('PHASE_PASS='+$lastStep+';SOURCE='+$sourceAfter+';TARGET='+$targetCount)
  $lastStep='TARGET_GIT'
  Write-Output ('PHASE_STARTED='+$lastStep)
  if((ReadGit $target @('rev-parse','HEAD')) -ne $head){throw 'TARGET_HEAD_MISMATCH'}
  if((ReadGit $target @('rev-parse','--git-common-dir')) -ne '.git'){throw 'TARGET_COMMON_DIR_MISMATCH'}
  $newWorktrees=ReadGit $target @('worktree','list','--porcelain')
  if($newWorktrees -notmatch 'worktree W:/AGZ/Projects/mahou/Repository'){throw 'TARGET_WORKTREE_PATH_MISMATCH'}
  if((ReadGit $target @('status','--porcelain=v1','--untracked-files=all')) -ne $oldStatus){throw 'TARGET_DIRTY_STATE_MISMATCH'}
  if((ReadGit $target @('for-each-ref','--format=%(refname):%(objectname)')) -ne $oldRefs){throw 'TARGET_REFS_MISMATCH'}
  if((ReadGit $target @('stash','list')) -ne $oldStash){throw 'TARGET_STASH_MISMATCH'}
  if((ReadGit $target @('fsck','--full','--no-reflogs')) -ne $oldFsck){throw 'TARGET_FSCK_OUTPUT_MISMATCH'}
  Write-Output ('PHASE_PASS='+$lastStep)
  $lastStep='CREATE_FIVE_EVIDENCE'
  Write-Output ('PHASE_STARTED='+$lastStep)
  if(Test-Path -LiteralPath $ev){throw 'EVIDENCE_PATH_OCCUPIED'}
  [void][IO.Directory]::CreateDirectory($ev)
  $manifestRows=New-Object 'System.Collections.Generic.List[object]'
  foreach($r in $baseline){
    $rel=[string]$r.relative_path
    [void]$manifestRows.Add([pscustomobject]@{
      relative_path=$rel;source_path=(Join-Path $source $rel);target_path=(Join-Path $target $rel)
      size_bytes=[long]$r.size_bytes;mtime_utc=$r.mtime_utc
      source_sha256=$r.sha256;target_sha256=$r.sha256
      class=$(if($rel.StartsWith('.git\')){'A_GIT_METADATA'}else{'A_DURABLE'})
      copied=$true;matched=$true;original_preserved=$true
    })
  }
  $names=@('source-target-manifest.csv','quarantine-manifest.csv','dependencies.json','verification-report.md','migration-receipt.json')
  $manifestRows.ToArray()|Export-Csv -LiteralPath (Join-Path $ev $names[0]) -NoTypeInformation -Encoding UTF8
  [pscustomobject]@{status='EMPTY';old_path='';quarantine_path='';reason='NO_QUARANTINE_ACTION';no_delete=$true}|Export-Csv -LiteralPath (Join-Path $ev $names[1]) -NoTypeInformation -Encoding UTF8
  $deps=[ordered]@{
    batch_id=$batch;owner_project='Mahou';consumer_project='UNKNOWN';dependency_id='MAHOU_STANDALONE_GIT'
    old_path=$source;target_path=$target;git_head=$head;git_common_dir='.git'
    original_worktrees=$oldWorktrees;target_worktrees=$newWorktrees
    dirty_tracked_modifications=16;untracked_files=3
    dangling_tree='cf64a594f50925a894d6a72db6d66fc2f63eb41c'
    consumer_paths='OPEN: owner HKCU, dynamic indirect consumers, runtime path cutover not verified'
    no_original_change=$true
  }
  [IO.File]::WriteAllText((Join-Path $ev $names[2]),($deps|ConvertTo-Json -Depth 10),(New-Object Text.UTF8Encoding($false)))
  $notes=@(
    '# Mahou Model B — verified COPY-FIRST'
    ('Batch: '+$batch)
    ('Operator: '+$owner)
    ('UTC: '+[DateTime]::UtcNow.ToString('o'))
    ('Source and target SHA-256: 391/391 PASS before and after COPY')
    'Full .git, dangling object, 16 modified and 3 untracked files preserved'
    ('Robocopy: /E /COPY:DAT /DCOPY:DAT /R:0 /W:0 /XJ; exit '+$code)
    'Git HEAD, refs, stash, dirty state, worktree, full fsck: PASS'
    'Originals preserved. No delete, move, ACL, Git repair or production action.'
    'G0: technical PASS; consumer coverage OPEN (user HKCU and indirect paths).'
    'Runtime smoke: NOT RUN. Remote network/origin: NOT RUN.'
    'Kopia parent snapshot and isolated restore: NOT AUTHORIZED / NOT RUN.'
    'Quarantine: EMPTY; no legacy classification asserted.'
  )
  [IO.File]::WriteAllLines((Join-Path $ev $names[3]),$notes,(New-Object Text.UTF8Encoding($false)))
  $receiptOut=[ordered]@{
    project='Mahou';slug='mahou';batch_id=$batch;operator=$owner;utc=[DateTime]::UtcNow.ToString('o')
    CURRENT_STAGE='COPIED_VERIFIED_RUNTIME_UNTESTED';G0='PASS_TECHNICAL_CONSUMERS_OPEN'
    G1='COPY_SHA256_GIT_PASS';G2='FIVE_EVIDENCE_CREATED';G3='KOPIA_NO_GO'
    G4='RESTORE_RUNTIME_AND_CUTOVER_PENDING'
    original=$source;target=$target;source_files=391;target_files=391;sha256_match_all=$true
    git_head=$head;git_fsck_exit=0;git_local_changes_preserved=19
    robocopy_exit=$code;original_preserved=$true;no_delete=$true
    parent_snapshot_id=$null;parent_root_id=$null
    consumer_cutover='NOT_STARTED';runtime='NOT_RUN';git_network='NOT_RUN'
    authority='Owner 2026-11-01 Mahou-only COPY-FIRST/NO-DELETE after PASS'
    evidence=$names
  }
  [IO.File]::WriteAllText((Join-Path $ev $names[4]),($receiptOut|ConvertTo-Json -Depth 10),(New-Object Text.UTF8Encoding($false)))
  foreach($n in $names){Write-Output ('EVIDENCE_'+$n+'_SHA256='+(HashFile (Join-Path $ev $n)))}
  Write-Output ('PHASE_PASS='+$lastStep)
  Write-Output 'CURRENT_STAGE=COPIED_VERIFIED_RUNTIME_UNTESTED'
  Write-Output 'SOURCE_TARGET_SHA256_MATCHED=391/391'
  Write-Output 'KOPIA_PARENT_NOT_RUN'
  exit 0
} catch {
  $errorType=$_.Exception.GetType().Name
  $reason=$_.Exception.Message
  $failure=[ordered]@{
    batch_id=$batch;utc=[DateTime]::UtcNow.ToString('o')
    last_completed_or_attempted_step=$lastStep
    failure_reason=$reason;error_type=$errorType
    status=$(if($copyStarted){'FAILED_PARTIAL_PRESERVE_TARGET'}else{'BLOCKED_NO_COPY'})
    original_preserved=$true;no_cleanup=$true
  }
  if(Test-Path -LiteralPath $temp -PathType Container) {
    $failFile=Join-Path $temp 'AGZ-MAH-CopyFirst-v2-failure.json'
    [IO.File]::WriteAllText($failFile,($failure|ConvertTo-Json -Depth 6),(New-Object Text.UTF8Encoding($false)))
    Write-Output ('FAILURE_RECEIPT_SHA256='+(HashFile $failFile))
  }
  Write-Output ('PHASE_FAIL='+$lastStep)
  Write-Output ('FAILURE_REASON='+$reason)
  Write-Output ('COPY_STARTED='+$copyStarted)
  exit 2
}
