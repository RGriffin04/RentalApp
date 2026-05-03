# MAUI Authentication Architecture Guide

## 🔍 Problem Discovered:

The MAUI app has **TWO authentication architectures**:

1. **Database Authentication** (Currently Active)
   - ✅ Works without API running
   - ✅ Direct PostgreSQL connection
   - ❌ Doesn't use your REST API
   - ❌ No JWT tokens

2. **API Authentication** (Now Available)
   - ❌ Requires API to be running
   - ✅ Uses REST API endpoints
   - ✅ JWT token-based
   - ✅ Consistent with the rest of the app

---

## 📋 Current Architecture:

### What Uses the Database Directly:
- ✅ `AuthenticationService` - Login/Register/Logout

### What Uses the API:
- ✅ `ItemService` - Categories, Items, Nearby Search
- ✅ `RentalService` - Rentals CRUD
- ✅ `RatingService` - Reviews/Ratings
- ✅ `LocationService` - Geocoding

This is why:
- Login works even when API is stopped ✅
- Categories don't load when API is stopped ❌

---

## 🔄 Two Options:

### Option 1: Keep Database Authentication (Current)

**Pros:**
- ✅ Works offline for login
- ✅ Faster login (no HTTP roundtrip)
- ✅ No API required for authentication

**Cons:**
- ❌ Inconsistent architecture (Auth uses DB, everything else uses API)
- ❌ MAUI app needs PostgreSQL connection string
- ❌ No JWT tokens for API calls

**When to use:** Development/testing when you want login to work without starting the API

---

### Option 2: Use HTTP Authentication (New)

**Pros:**
- ✅ Consistent architecture (everything uses API)
- ✅ JWT tokens for secure API calls
- ✅ MAUI app doesn't need database connection
- ✅ Better for production deployment

**Cons:**
- ❌ Requires API to be running
- ❌ Doesn't work offline

**When to use:** Production or when you want the "proper" architecture

---

## 🔧 How to Switch to HTTP Authentication:

### Step 1: Edit `MauiProgram.cs`

**Current (Database Authentication):**
```csharp
// Option 1: Direct database authentication (current - works offline but doesn't use API)
builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();

// Option 2: HTTP API authentication (uncomment to use API for login)
// Comment out Option 1 above and uncomment the line below to use API authentication
// builder.Services.AddSingleton<IAuthenticationService, HttpAuthenticationService>();
```

**Change to (HTTP Authentication):**
```csharp
// Option 1: Direct database authentication (current - works offline but doesn't use API)
// builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();

// Option 2: HTTP API authentication (uncomment to use API for login)
// Comment out Option 1 above and uncomment the line below to use API authentication
builder.Services.AddSingleton<IAuthenticationService, HttpAuthenticationService>();
```

### Step 2: Make Sure API is Running

```powershell
cd "C:\software engineering CW\code\StarterApp\RentalApp.API"
dotnet run
```

Verify API is accessible: http://localhost:5000/auth/token

### Step 3: Test Login

Run the MAUI app and try logging in. If the API is not running, you'll get a connection error.

---

## 📝 What Changed:

### New Files Created:
1. **`StarterApp/Services/HttpAuthenticationService.cs`** - HTTP-based authentication
2. This guide document

### Modified Files:
1. **`StarterApp/Services/IAuthenticationService.cs`** - Added `GetAuthToken()` method
2. **`StarterApp/Services/AuthenticationService.cs`** - Implemented `GetAuthToken()` (returns null for DB auth)
3. **`StarterApp/Services/BaseHttpService.cs`** - Now uses auth token from service
4. **`StarterApp/MauiProgram.cs`** - Added comments to switch between auth types

---

## 🎯 Recommended Setup:

### For Development (Flexible):
Keep **Database Authentication** (Option 1) so you can:
- Test login without starting the API
- Start API only when testing categories/items/rentals

### For Production (Consistent):
Use **HTTP Authentication** (Option 2) so:
- Everything goes through the API
- JWT tokens secure all requests
- MAUI app doesn't need database connection string

---

## 🐛 Troubleshooting:

### Login works but categories don't load:
**Cause:** Using database auth but API isn't running  
**Fix:** Start the API:
```powershell
cd RentalApp.API
dotnet run
```

### Login fails with "connection refused":
**Cause:** Using HTTP auth but API isn't running  
**Fix:** Either:
1. Start the API, OR
2. Switch back to database authentication in `MauiProgram.cs`

### Categories endpoint returns 401 Unauthorized:
**Cause:** Using database auth so no JWT token is sent  
**Fix:** Either:
1. Switch to HTTP authentication in `MauiProgram.cs`, OR
2. Make categories endpoint `[AllowAnonymous]` in the API (temporary workaround)

---

## ✅ Current Status:

- [x] Database authentication working
- [x] HTTP authentication service created
- [x] Auth token support added to HTTP services
- [x] Both options available in `MauiProgram.cs`
- [ ] Choose which authentication method you want to use
- [ ] Start API if using HTTP authentication
- [ ] Test end-to-end flow with categories

---

## 📊 Architecture Diagram:

### Current (Database Authentication):
```
MAUI App
├─ Login/Register ──> PostgreSQL (Direct)
└─ Items/Rentals/Ratings ──> API ──> PostgreSQL
```

### With HTTP Authentication:
```
MAUI App
├─ Login/Register ──> API ──> PostgreSQL
└─ Items/Rentals/Ratings ──> API ──> PostgreSQL
```

---

## 🚀 Next Steps:

1. **Decide which authentication method you want**
2. **If using HTTP auth:** Make sure API is always running when testing
3. **If keeping database auth:** Just ensure API runs for categories/items/rentals
4. **Test the full flow:** Login → View Categories → Browse Items

Would you like to switch to HTTP authentication or keep the current database authentication?
