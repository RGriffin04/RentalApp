# Test JWT Authentication Flow

Write-Host "🧪 Testing RentalApp JWT Authentication" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$apiUrl = "http://localhost:5000"

# Test 0: Check Docker is running
Write-Host "Test 0: Checking Docker services..." -ForegroundColor Yellow
try {
    $apiStatus = docker-compose ps api --format json 2>$null | ConvertFrom-Json
    if ($apiStatus.State -eq "running") {
        Write-Host "✅ API container is running`n" -ForegroundColor Green
    } else {
        Write-Host "⚠️  API container state: $($apiStatus.State)" -ForegroundColor Yellow
        Write-Host "   Starting services..." -ForegroundColor Cyan
        docker-compose up -d
        Start-Sleep -Seconds 10
    }
} catch {
    Write-Host "⚠️  Docker compose not running. Starting services..." -ForegroundColor Yellow
    docker-compose up -d
    Start-Sleep -Seconds 10
}

# Test 1: Check API is running
Write-Host "Test 1: Checking if API is running..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$apiUrl/auth/token" -Method OPTIONS -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop
    Write-Host "✅ API is running at $apiUrl`n" -ForegroundColor Green
} catch {
    Write-Host "❌ API is not running. Please start the API first:" -ForegroundColor Red
    Write-Host "   cd RentalApp.API" -ForegroundColor Gray
    Write-Host "   dotnet run`n" -ForegroundColor Gray
    exit 1
}

# Test 2: Register a test user
Write-Host "Test 2: Registering test user..." -ForegroundColor Yellow
$registerBody = @{
    email = "testuser@rental.app"
    password = "Test123!"
    firstName = "Test"
    lastName = "User"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$apiUrl/auth/register" -Method POST -Body $registerBody -ContentType "application/json" -ErrorAction Stop
    Write-Host "✅ User registered successfully" -ForegroundColor Green
    Write-Host "   Response: $($response.message)`n" -ForegroundColor Gray
} catch {
    if ($_.Exception.Response.StatusCode -eq 409) {
        Write-Host "ℹ️  User already exists (this is OK)`n" -ForegroundColor Cyan
    } else {
        Write-Host "⚠️  Registration failed: $($_.Exception.Message)`n" -ForegroundColor Yellow
    }
}

# Test 3: Login and get JWT token
Write-Host "Test 3: Testing login and JWT token..." -ForegroundColor Yellow
$loginBody = @{
    email = "testuser@rental.app"
    password = "Test123!"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$apiUrl/auth/token" -Method POST -Body $loginBody -ContentType "application/json" -ErrorAction Stop
    Write-Host "✅ Login successful!" -ForegroundColor Green
    Write-Host "   Email: $($loginResponse.email)" -ForegroundColor Gray
    Write-Host "   Name: $($loginResponse.firstName) $($loginResponse.lastName)" -ForegroundColor Gray
    Write-Host "   Roles: $($loginResponse.roles -join ', ')" -ForegroundColor Gray
    Write-Host "   Token: $($loginResponse.token.Substring(0, 50))..." -ForegroundColor Gray
    Write-Host "   Expires: $($loginResponse.expiresAt)`n" -ForegroundColor Gray

    $token = $loginResponse.token
} catch {
    Write-Host "❌ Login failed: $($_.Exception.Message)`n" -ForegroundColor Red
    exit 1
}

# Test 4: Get categories with JWT token
Write-Host "Test 4: Fetching categories with JWT token..." -ForegroundColor Yellow
$headers = @{
    Authorization = "Bearer $token"
}

try {
    $categories = Invoke-RestMethod -Uri "$apiUrl/api/categories" -Headers $headers -ErrorAction Stop
    Write-Host "✅ Categories fetched successfully!" -ForegroundColor Green
    Write-Host "   Found $($categories.Count) categories:" -ForegroundColor Gray
    foreach ($category in $categories) {
        Write-Host "   - $($category.name): $($category.description)" -ForegroundColor Gray
    }
    Write-Host ""
} catch {
    Write-Host "❌ Failed to fetch categories: $($_.Exception.Message)`n" -ForegroundColor Red
    exit 1
}

# Test 5: Get categories WITHOUT token (should fail)
Write-Host "Test 5: Testing without JWT token (should fail)..." -ForegroundColor Yellow
try {
    $categories = Invoke-RestMethod -Uri "$apiUrl/api/categories" -ErrorAction Stop
    Write-Host "⚠️  Categories fetched without token (security issue!)`n" -ForegroundColor Yellow
} catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "✅ Correctly rejected (401 Unauthorized)`n" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Unexpected error: $($_.Exception.Message)`n" -ForegroundColor Yellow
    }
}

# Test 6: Get items with JWT token
Write-Host "Test 6: Fetching items with JWT token..." -ForegroundColor Yellow
try {
    $items = Invoke-RestMethod -Uri "$apiUrl/api/items" -Headers $headers -ErrorAction Stop
    Write-Host "✅ Items fetched successfully!" -ForegroundColor Green
    if ($items.Count -eq 0) {
        Write-Host "   ℹ️  No items in database yet (create some in the MAUI app)`n" -ForegroundColor Cyan
    } else {
        Write-Host "   Found $($items.Count) items`n" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ Failed to fetch items: $($_.Exception.Message)`n" -ForegroundColor Red
}

# Summary
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "🎉 JWT Authentication Tests Complete!" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Test Credentials:" -ForegroundColor Yellow
Write-Host "  Email: testuser@rental.app" -ForegroundColor Gray
Write-Host "  Password: Test123!" -ForegroundColor Gray
Write-Host "`nYou can use these credentials in the MAUI app`n" -ForegroundColor Gray

Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Open Visual Studio" -ForegroundColor Gray
Write-Host "  2. Set StarterApp as startup project" -ForegroundColor Gray
Write-Host "  3. Run the app (F5)" -ForegroundColor Gray
Write-Host "  4. Login with the test credentials" -ForegroundColor Gray
Write-Host "  5. Check that categories load in the marketplace`n" -ForegroundColor Gray
