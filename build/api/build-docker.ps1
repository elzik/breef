$repoRoot =  Resolve-Path "$PSScriptRoot/../.."

docker build `
    -t ghcr.io/elzik/elzik-breef-api:latest `
        -f "$repoRoot/src/Elzik.Breef.Api/Dockerfile" `
    "$repoRoot/src"
