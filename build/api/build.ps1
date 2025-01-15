$repoRoot = "$PSScriptRoot/../.."

try {
    dotnet build $repoRoot `
        --configuration Release `
        --no-restore -ErrorAction Stop
} catch {
    Write-Error "Build failed with error: $_"
    throw
}
