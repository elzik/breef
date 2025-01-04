$repoRoot = "$PSScriptRoot/../.."

dotnet build $repoRoot `
    --configuration Release `
    --no-restore
