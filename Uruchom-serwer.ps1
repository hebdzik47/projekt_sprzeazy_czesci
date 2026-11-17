param([string]$ListenUrls = 'http://127.0.0.1:5180')
$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath $PSScriptRoot
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:DOTNET_CLI_HOME = Join-Path $PSScriptRoot '.dotnet'
Write-Host "Strona: $ListenUrls (pozostaw to okno otwarte)"
dotnet run --no-launch-profile --no-build --project backend/AutoPartsSales.Api --urls $ListenUrls
