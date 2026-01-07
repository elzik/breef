param (
    [Parameter(Mandatory=$true)]
    [string]$Version
)

Write-Output "Will be built as version: $Version"

$repoRoot =  Resolve-Path "$PSScriptRoot/../.."

docker build `
    -t "ghcr.io/elzik/elzik-breef-api:$Version" `
    -f "$repoRoot/src/Elzik.Breef.Api/Dockerfile" `
    "$repoRoot"

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
