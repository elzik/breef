param (
    [Parameter(Mandatory=$true)]
    [string]$Version
)

Write-Output "Will be built as version: $Version"

$repoRoot = "$PSScriptRoot/../.."

$assemblyVersion = "$($Version.Split('.')[0]).0.0.0"

dotnet build $repoRoot `
    --configuration Release `
    --no-restore `
    -p:Version=$Version `
    -p:AssemblyVersion=$assemblyVersion `
    -p:InformationalVersion=$Version

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
