$repoRoot = "$PSScriptRoot/../.."

try {
    dotnet build $repoRoot `
        --configuration Release `
        --no-restore -ErrorAction Stop
} catch {
    $global:LASTEXITCODE = 1
}
