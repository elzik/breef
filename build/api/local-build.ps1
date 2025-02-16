& $PSScriptRoot/restore.ps1
$semver = & $PSScriptRoot/get-semver.ps1 | Select-Object -Last 1
Write-Output "SemVer: $semver"
& $PSScriptRoot/build.ps1
& $PSScriptRoot/build-docker.ps1
& $PSScriptRoot/test.ps1
