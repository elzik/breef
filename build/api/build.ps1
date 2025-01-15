$repoRoot = "$PSScriptRoot/../.."

try {
    dotnet build $repoRoot `
        --configuration Release `
        --no-restore
} catch {
    $global:LASTEXITCODE = 1
    Write-Error $_
}
