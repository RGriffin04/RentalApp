# 🎯 JWT Implementation - Quick Reference

## ✅ What Was Implemented:

### Architecture Change:
```
Before: MAUI → PostgreSQL (Direct)
After:  MAUI → API (JWT) → PostgreSQL
```

All database operations now go through the API with JWT token authentication.

---

## 🐳 Docker Setup:

The entire backend now runs in Docker:
- **PostgreSQL** with PostGIS (port 5432)
- **EF Migrations** (runs automatically)
- **RentalApp API** (port 5000)

---

## 🚀 Quick Start (2 Steps):

### 1. Start Everything with Docker
```powershell
.\start-api.ps1
```

This will:
- ✅ Start PostgreSQL
- ✅ Run migrations (create tables + seed categories)
- ✅ Start the API on http://localhost:5000
- ✅ Show service status and logs

### 2. Run MAUI App
- Open Visual Studio
- Press F5
- Login with test credentials
- Categories should load! 🎉

---

## 🧪 Optional: Test JWT Authentication

```powershell
.\test-jwt-auth.ps1
```

This creates a test user: `testuser@rental.app` / `Test123!`

---

## 🛑 Stop Services

```powershell
.\stop-api.ps1
```

Or:
```powershell
docker-compose down
```

---

## 📋 What Runs in Docker:

| Service | Port | Description |
|---------|------|-------------|
| **db** | 5432 | PostgreSQL + PostGIS |
| **migrate** | - | EF Core migrations (runs once) |
| **api** | 5000 | RentalApp API (hot reload enabled) |

---

## 🔧 Common Commands:

```powershell
# Start services
.\start-api.ps1

# View API logs
docker-compose logs -f api

# Restart API only
docker-compose restart api

# Stop services
.\stop-api.ps1

# Stop and remove everything
docker-compose down -v
```

---

## 📋 Modified Files:

1. `StarterApp/MauiProgram.cs` - Switched to `HttpAuthenticationService`
2. `StarterApp/Services/HttpAuthenticationService.cs` - HTTP auth with JWT
3. `StarterApp/Services/IAuthenticationService.cs` - Added `GetAuthToken()`
4. `StarterApp/Services/AuthenticationService.cs` - Added `GetAuthToken()` stub
5. `StarterApp/Services/BaseHttpService.cs` - Sends JWT with requests

---

## 🔒 JWT Token Flow:

```
1. User logs in
   ↓
2. MAUI → POST /auth/token → API
   ↓
3. API validates & returns JWT token
   ↓
4. MAUI stores token
   ↓
5. All future requests include:
   Authorization: Bearer {token}
   ↓
6. API validates token before processing
```

---

## 🧪 Testing:

### Manual API Test:
```powershell
# Login
$login = @{ email="testuser@rental.app"; password="Test123!" } | ConvertTo-Json
$response = Invoke-RestMethod -Uri "http://localhost:5000/auth/token" -Method POST -Body $login -ContentType "application/json"
$token = $response.token

# Get categories with token
$headers = @{ Authorization = "Bearer $token" }
Invoke-RestMethod -Uri "http://localhost:5000/api/categories" -Headers $headers
```

### MAUI App Test:
1. Run app
2. Login with: `testuser@rental.app` / `Test123!`
3. Navigate to marketplace
4. Check categories appear ✅

---

## 🐛 Troubleshooting:

| Issue | Solution |
|-------|----------|
| "Connection refused" | Start API: `cd RentalApp.API && dotnet run` |
| "401 Unauthorized" | Check token is being sent in Authorization header |
| "No categories" | Run: `docker ps` → verify PostgreSQL is running |
| Login fails | Use test credentials or register new user |

---

## 📊 API Endpoints:

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/auth/token` | POST | No | Login & get JWT token |
| `/auth/register` | POST | No | Register new user |
| `/api/categories` | GET | Yes | Get all categories |
| `/api/items` | GET | Yes | Browse items |
| `/api/items/nearby` | GET | Yes | Location-based search |
| `/api/rentals` | GET | Yes | View rentals |

---

## 🔑 Test Credentials:

**Email:** `testuser@rental.app`  
**Password:** `Test123!`

Created by `test-jwt-auth.ps1` script.

---

## ✅ Success Indicators:

- [ ] API running on http://localhost:5000
- [ ] PostgreSQL running in Docker
- [ ] Test script passes all tests
- [ ] MAUI app can login
- [ ] Categories load in app
- [ ] Items can be created/viewed
- [ ] Rentals work end-to-end

---

## 📝 Configuration:

### API Base URL (ApiConfig.cs):
- **Windows/iOS:** `http://localhost:5000`
- **Android Emulator:** `http://10.0.2.2:5000`

### JWT Settings (appsettings.json):
```json
{
  "Jwt": {
    "Secret": "CHANGE_ME_TO_A_LONG_RANDOM_SECRET_AT_LEAST_32_CHARS",
    "Issuer": "RentalApp.Api",
    "Audience": "RentalApp",
    "ExpiryMinutes": 60
  }
}
```

---

## 🎉 You're All Set!

Run `.\start-api.ps1` then press F5 in Visual Studio to test the app!
