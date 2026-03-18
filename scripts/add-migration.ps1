param(
    [Parameter(Mandatory=$true)]
    [string]$name
)

Set-Location $PSScriptRoot/..

Write-Host "Create migration $name..." -ForegroundColor Cyan

dotnet ef migrations add $name `
    --project MyStore.Infrastructure `
    --startup-project MyStore.Api `
    --context ApplicationDbContext `
    --output-dir Persistence/Migrations

Write-Host "Done. Now execute script update-db.ps1" -ForegroundColor Green