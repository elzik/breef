dotnet tool install --global GitVersion.Tool --version 6.1.0

$gitVersionOutput = dotnet-gitversion
$gitVersionJson = $gitVersionOutput | ConvertFrom-Json

Write-Output $($gitVersionJson.SemVer)