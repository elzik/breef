$semver = & $PSScriptRoot/get-semver.ps1 | Select-Object -Last 1
Write-Output "SemVer: $semver"

& $PSScriptRoot/restore.ps1
& $PSScriptRoot/build.ps1 -Version $semver
& $PSScriptRoot/build-docker.ps1 -Version $semver
& $PSScriptRoot/test.ps1
