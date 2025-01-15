$repoRoot = "$PSScriptRoot/../.."

dotnet restore $repoRoot

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
