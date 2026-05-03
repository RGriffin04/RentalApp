# Quick Start Script for RentalApp with Docker

Write-Host "🚀 RentalApp - Docker Compose Setup" -ForegroundColor Cyan
Write-Host "====================================`n" -ForegroundColor Cyan

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

# Step 1: Check Docker
Write-Host "📦 Step 1: Checking Docker..." -ForegroundColor Yellow
$dockerRunning = docker ps 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Docker is not running. Please start Docker Desktop." -ForegroundColor Red
    exit 1
}
Write-Host "✅ Docker is running`n" -ForegroundColor Green

# Step 2: Build and start all services
Write-Host "🏗️  Step 2: Building and starting services..." -ForegroundColor Yellow
Write-Host "   This may take a few minutes on first run...`n" -ForegroundColor Cyan

docker-compose up -d --build

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to start services" -ForegroundColor Red
    exit 1
}

Write-Host "`n✅ Services started successfully!`n" -ForegroundColor Green

# Step 3: Wait for services to be healthy
Write-Host "⏳ Step 3: Waiting for services to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Check database
$dbStatus = docker-compose ps db --format json | ConvertFrom-Json
if ($dbStatus.State -eq "running") {
    Write-Host "✅ PostgreSQL is running" -ForegroundColor Green
} else {
    Write-Host "⚠️  PostgreSQL status: $($dbStatus.State)" -ForegroundColor Yellow
}

# Check API
$apiStatus = docker-compose ps api --format json | ConvertFrom-Json
if ($apiStatus.State -eq "running") {
    Write-Host "✅ API is running" -ForegroundColor Green
} else {
    Write-Host "⚠️  API status: $($apiStatus.State)" -ForegroundColor Yellow
}

Write-Host "`n" -NoNewline

# Step 4: Show service info
Write-Host "📋 Service Information:" -ForegroundColor Cyan
Write-Host "   🐘 PostgreSQL: localhost:5432" -ForegroundColor Gray
Write-Host "   🌐 API: http://localhost:5000" -ForegroundColor Gray
Write-Host "`n"

# Step 5: Show logs
Write-Host "📜 Recent API Logs:" -ForegroundColor Cyan
docker-compose logs --tail=20 api

Write-Host "`n"
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "✅ RentalApp is ready!" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "API Endpoints:" -ForegroundColor Yellow
Write-Host "  - Auth: http://localhost:5000/auth/token" -ForegroundColor Gray
Write-Host "  - Categories: http://localhost:5000/api/categories" -ForegroundColor Gray
Write-Host "  - Items: http://localhost:5000/api/items`n" -ForegroundColor Gray

Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Test API: .\test-jwt-auth.ps1" -ForegroundColor Gray
Write-Host "  2. Run MAUI app: Press F5 in Visual Studio" -ForegroundColor Gray
Write-Host "  3. View logs: docker-compose logs -f api" -ForegroundColor Gray
Write-Host "  4. Stop services: docker-compose down`n" -ForegroundColor Gray

Write-Host "Useful Commands:" -ForegroundColor Yellow
Write-Host "  docker-compose ps              - View running services" -ForegroundColor Gray
Write-Host "  docker-compose logs -f api     - Follow API logs" -ForegroundColor Gray
Write-Host "  docker-compose restart api     - Restart API" -ForegroundColor Gray
Write-Host "  docker-compose down            - Stop all services`n" -ForegroundColor Gray

