$scriptPath = $MyInvocation.MyCommand.Definition
$repoRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $scriptPath))
$project = "$repoRoot/test/Janus.Core.Test/Janus.Core.Test.csproj"
Write-Host("Starting tests for $project")
dotnet test $project /p:CollectCoverage=true
