Set-Location $PSScriptRoot/..

Write-Host "Updating database" -ForegroundColor Cyan

dotnet ef database update `
    --project MyStore.Infrastructure `
    --startup-project MyStore.Api

Write-Host "Database update completed" -ForegroundColor Green
