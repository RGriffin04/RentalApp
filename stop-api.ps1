# Stop RentalApp Docker Services

Write-Host "🛑 Stopping RentalApp Services..." -ForegroundColor Cyan
Write-Host "==================================`n" -ForegroundColor Cyan

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

# Check Docker
$dockerRunning = docker ps 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ℹ️  Docker is not running`n" -ForegroundColor Cyan
    exit 0
}

# Stop services
Write-Host "Stopping services..." -ForegroundColor Yellow
docker-compose stop

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Services stopped`n" -ForegroundColor Green
} else {
    Write-Host "⚠️  Failed to stop services`n" -ForegroundColor Yellow
}

# Show status
Write-Host "Service Status:" -ForegroundColor Cyan
docker-compose ps

Write-Host "`nTo remove containers completely:" -ForegroundColor Yellow
Write-Host "  docker-compose down`n" -ForegroundColor Gray

Write-Host "To remove containers and volumes:" -ForegroundColor Yellow
Write-Host "  docker-compose down -v`n" -ForegroundColor Gray
