# ✅ MAUI App Now Uses API with JWT Tokens

## 🎯 What Changed:

### 1. **Authentication Architecture**
- ❌ **Before:** MAUI app connected directly to PostgreSQL database for login
- ✅ **After:** MAUI app calls REST API `/auth/token` endpoint and receives JWT token

### 2. **Service Registration**
- ❌ **Before:** `AuthenticationService` (direct database)
- ✅ **After:** `HttpAuthenticationService` (HTTP API with JWT)

### 3. **Database Dependency**
- ❌ **Before:** MAUI app had `AppDbContext` dependency
- ✅ **After:** Database context removed - all data goes through API

### 4. **JWT Token Flow**
- ✅ Login returns JWT token
- ✅ Token stored in `HttpAuthenticationService`
- ✅ All HTTP requests include `Authorization: Bearer {token}` header
- ✅ API validates token for protected endpoints

---

## 📋 Files Modified:

### StarterApp (MAUI)
1. **`MauiProgram.cs`**
   - ✅ Switched to `HttpAuthenticationService`
   - ✅ Removed `AppDbContext` registration (commented out)

2. **`Services/HttpAuthenticationService.cs`**
   - ✅ Fixed to use API's TokenResponse format
   - ✅ Stores JWT token
   - ✅ Sets Authorization header for all requests

3. **`Services/BaseHttpService.cs`**
   - ✅ Uses `GetAuthToken()` to add JWT to requests

### RentalApp.API
- ✅ No changes needed - already has JWT authentication

---

## 🔄 Complete Data Flow:

### Login Flow:
```
1. User enters email/password in MAUI app
2. MAUI app → POST /auth/token → API
3. API validates credentials against PostgreSQL
4. API generates JWT token
5. API ← returns Token + User Info
6. MAUI app stores token
7. All future requests include: Authorization: Bearer {token}
```

### Data Access Flow:
```
MAUI App → HTTP Request with JWT → API → PostgreSQL
                                    ↓
MAUI App ← JSON Response ← API ← Data from Database
```

---

## 🚀 How to Test:

### Step 1: Start PostgreSQL (Docker)
```powershell
cd "C:\software engineering CW\code\StarterApp"
docker-compose up -d db
```

**Verify:**
```powershell
docker ps | Select-String "postgis"
```

---

### Step 2: Start the API
```powershell
cd "C:\software engineering CW\code\StarterApp\RentalApp.API"
dotnet run
```

**Expected output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

**Verify endpoints:**
- http://localhost:5000/auth/token (POST - login)
- http://localhost:5000/api/categories (GET - requires auth)
- http://localhost:5000/api/items (GET - requires auth)

---

### Step 3: Test API with curl (Optional)

**Login:**
```powershell
$body = @{
    email = "test@example.com"
    password = "YourPassword123"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/auth/token" -Method POST -Body $body -ContentType "application/json"
```

**Expected response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-05-06T13:00:00Z",
  "email": "test@example.com",
  "firstName": "Test",
  "lastName": "User",
  "roles": ["User"]
}
```

**Get Categories (with token):**
```powershell
$token = "YOUR_TOKEN_HERE"
$headers = @{ Authorization = "Bearer $token" }
Invoke-RestMethod -Uri "http://localhost:5000/api/categories" -Headers $headers
```

---

### Step 4: Run MAUI App

1. **Set StarterApp as startup project**
2. **Select platform:**
   - Windows Machine (uses `http://localhost:5000`)
   - Android Emulator (uses `http://10.0.2.2:5000`)
3. **Press F5 to run**

---

### Step 5: Test Login

1. **Register a new account** (if you don't have one):
   - Click "Register"
   - Fill in details
   - Submit

2. **Login:**
   - Enter email/password
   - Click "Login"

**Expected:**
- ✅ API receives login request
- ✅ API returns JWT token
- ✅ MAUI app stores token
- ✅ Navigate to marketplace

---

### Step 6: Test Categories

1. **Navigate to Items List**
2. **Check category picker**

**Expected:**
- ✅ MAUI app sends GET /api/categories with JWT token
- ✅ API validates token
- ✅ API returns 10 categories
- ✅ Categories appear in picker

---

### Step 7: Verify JWT Token is Used

**Check Visual Studio Output Window:**

Look for HTTP requests like:
```
Request: GET http://localhost:5000/api/categories
Headers: Authorization: Bearer eyJhbGciOiJIUzI1...
```

If you don't see the Authorization header, the token isn't being sent.

---

## 🐛 Troubleshooting:

### Issue: Login fails with "connection refused"
**Cause:** API is not running  
**Fix:**
```powershell
cd RentalApp.API
dotnet run
```

---

### Issue: Login fails with "Invalid email or password"
**Cause:** No test user in database  
**Fix:** Register a new user first

---

### Issue: Categories return 401 Unauthorized
**Cause:** JWT token not being sent  
**Fix:** Check that:
1. Login succeeded (token stored)
2. `BaseHttpService.AddAuthorizationHeader()` is called
3. Token isn't expired

**Debug:**
Add breakpoint in `BaseHttpService.AddAuthorizationHeader()` to verify token exists.

---

### Issue: Categories endpoint not found (404)
**Cause:** Wrong API URL  
**Fix:** Check `ApiConfig.cs`:
- Android: `http://10.0.2.2:5000`
- Windows: `http://localhost:5000`

---

### Issue: API runs but MAUI can't connect
**Platform:** Android Emulator  
**Cause:** Using `localhost` instead of `10.0.2.2`  
**Fix:** Already configured correctly in `ApiConfig.cs`

---

## 🔒 JWT Token Details:

### Token Contents (decoded):
```json
{
  "sub": "123",  // User ID
  "email": "test@example.com",
  "given_name": "Test",
  "family_name": "User",
  "role": ["User"],
  "exp": 1715011200,  // Expiration timestamp
  "iss": "RentalApp.Api",
  "aud": "RentalApp"
}
```

### Token Expiration:
- **Default:** 60 minutes
- **Configured in:** `RentalApp.API/appsettings.json` → `Jwt:ExpiryMinutes`

### Token Storage:
- **Where:** `HttpAuthenticationService._authToken` (in-memory)
- **Lifetime:** Lost when app closes
- **Future:** Could add SecureStorage for persistence

---

## ✅ Success Checklist:

- [x] MAUI app uses `HttpAuthenticationService`
- [x] Database context removed from MAUI
- [x] JWT token returned on login
- [x] Token stored in authentication service
- [x] Authorization header added to all HTTP requests
- [x] Build successful
- [ ] PostgreSQL running
- [ ] API running on port 5000
- [ ] MAUI app can login
- [ ] Categories load with JWT auth
- [ ] Items can be browsed
- [ ] All features work through API

---

## 📊 Architecture Diagram:

### Before (Mixed):
```
MAUI App
├─ Login/Register ──────> PostgreSQL (Direct ❌)
└─ Items/Rentals ──────> API ──> PostgreSQL
```

### After (Consistent):
```
MAUI App
├─ Login/Register ──────> API ──> PostgreSQL ✅
│                         (JWT Token)
│
└─ Items/Rentals ──────> API ──> PostgreSQL ✅
   (with JWT Token)       (Token Validation)
```

---

## 🎉 Next Steps:

1. **Start API** (Step 2 above)
2. **Run MAUI app**
3. **Test login** → Should get JWT token
4. **Test categories** → Should load with auth
5. **Test full marketplace flow**

---

## 📝 Future Enhancements:

### Token Persistence
Currently tokens are lost when the app closes. To persist:

```csharp
// Save token
await SecureStorage.SetAsync("auth_token", token);

// Load token on startup
var token = await SecureStorage.GetAsync("auth_token");
```

### Token Refresh
Add token refresh endpoint to get new token before expiration:
```csharp
[HttpPost("refresh")]
public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
```

### Offline Mode
Cache data locally for offline browsing:
```csharp
builder.Services.AddSingleton<ICacheService, CacheService>();
```

---

## 🔗 Relevant Files:

- `StarterApp/Services/HttpAuthenticationService.cs` - HTTP auth implementation
- `StarterApp/Services/ApiConfig.cs` - API base URL configuration
- `StarterApp/Services/BaseHttpService.cs` - JWT token in requests
- `RentalApp.API/Controllers/AuthController.cs` - Login/Register endpoints
- `RentalApp.API/appsettings.json` - JWT configuration
- `StarterApp/MauiProgram.cs` - Service registration

---

**Your MAUI app is now fully configured to use the API with JWT authentication! 🎉**
