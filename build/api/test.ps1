$repoRoot =  Resolve-Path "$PSScriptRoot/../.."

dotnet test $repoRoot `
    --configuration Release `
    --no-build `
    --verbosity normal `
    --logger 'trx;LogFileName=test-results.trx' `
    -p:CollectCoverage=true `
    -p:CoverletOutput=TestResults/coverage.opencover.xml `
    -p:CoverletOutputFormat=opencover
dotnet tool update `
    --global dotnet-reportgenerator-globaltool `
    --version 5.*
reportgenerator `
    "-reports:$repoRoot/tests/Elzik.Breef.Api.Tests.Functional/TestResults/coverage.opencover.xml" `
    "-targetdir:$repoRoot/tests/TestResults" `
    "-reporttypes:Badges;Cobertura" `
    "-filefilters:-*.g.cs" # https://github.com/danielpalme/ReportGenerator/issues/457
