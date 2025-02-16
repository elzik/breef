dotnet tool install --global GitVersion.Tool --version 6.1.0

$gitVersionOutput = dotnet-gitversion
Write-Output $gitVersionOutput

$gitVersionJson = $gitVersionOutput | ConvertFrom-Json
Write-Output $($gitVersionJson.SemVer)