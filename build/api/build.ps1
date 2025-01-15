$repoRoot = "$PSScriptRoot/../.."

dotnet build $repoRoot `
    --configuration Release `
    --no-restore

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
