# MAUI App Connectivity Diagnostic

## ❌ Problem: Categories Not Appearing

### Root Causes Identified:

1. **No Seed Data in Database** ✅ FIXED
   - The database had no categories to display
   - Added seed data with 10 common categories in `AppDbContext.cs`
   - Created migration `SeedCategories`

2. **API May Not Be Running** ⚠️ TO CHECK
   - API needs to be running on `http://localhost:5000`
   - MAUI app is configured to connect to:
     - Android: `http://10.0.2.2:5000` (emulator special address)
     - iOS/Other: `http://localhost:5000`

3. **Database May Not Be Running** ⚠️ TO CHECK
   - PostgreSQL database needs to be accessible
   - Connection string: `Host=localhost;Username=app_user;Password=app_password;Database=appdb`

---

## 🔧 Steps to Fix:

### Step 1: Apply the Database Migration ✅ DONE
The seed data migration has been created. Now you need to apply it:

```powershell
cd "C:\software engineering CW\code\StarterApp"
dotnet ef database update --project StarterApp.Database --startup-project StarterApp.Migrations
```

⚠️ **Important**: This requires PostgreSQL to be running and accessible at `localhost:5432`

---

### Step 2: Start PostgreSQL Database

If you're using Docker:
```powershell
docker run --name rental-postgres -e POSTGRES_USER=app_user -e POSTGRES_PASSWORD=app_password -e POSTGRES_DB=appdb -p 5432:5432 -d postgis/postgis:latest
```

If PostgreSQL is already running in Docker:
```powershell
docker start rental-postgres
```

---

### Step 3: Start the API

1. **Set the API as the startup project** or run it from terminal:
```powershell
cd "C:\software engineering CW\code\StarterApp\RentalApp.API"
dotnet run
```

2. **Verify the API is running** by visiting in a browser:
   - http://localhost:5000/api/categories

   You should see JSON with the 10 categories.

---

### Step 4: Test the MAUI App

1. **For Android Emulator**:
   - The app uses `http://10.0.2.2:5000` (special emulator address that maps to host's localhost)
   - Make sure the API is running on `http://localhost:5000`

2. **For Windows/iOS**:
   - The app uses `http://localhost:5000`
   - Both MAUI and API should be on the same machine

3. **Run the MAUI app** and check if categories appear

---

## 🧪 Quick Test Checklist:

### Test 1: Database is Running
```powershell
# Try to connect to PostgreSQL
Test-NetConnection -ComputerName localhost -Port 5432
```
✅ Should show "TcpTestSucceeded: True"

### Test 2: API is Running and Can Connect to Database
```powershell
# Start the API
cd "C:\software engineering CW\code\StarterApp\RentalApp.API"
dotnet run
```
✅ Should show "Now listening on: http://localhost:5000"
✅ No database connection errors in the console

### Test 3: Categories Endpoint Returns Data
Open browser and visit: http://localhost:5000/api/categories

✅ Should return JSON like:
```json
[
  {"id":1,"name":"Electronics","description":"Electronic devices and gadgets"},
  {"id":2,"name":"Tools","description":"Power tools, hand tools, and equipment"},
  ...
]
```

### Test 4: MAUI App Can Reach API
Check the Visual Studio Output window when running the MAUI app for:
- ❌ Connection timeout errors
- ❌ 404 Not Found errors
- ✅ Successful HTTP responses

---

## 🎯 Current Configuration:

### API Base URL (from `ApiConfig.cs`):
```csharp
#if ANDROID
    BaseUrl = "http://10.0.2.2:5000"  // Android emulator
#else
    BaseUrl = "http://localhost:5000"  // Windows/iOS/Mac
#endif
```

### API Actual Port (from `launchSettings.json`):
```json
"applicationUrl": "http://localhost:5000"
```

✅ Configuration matches - no changes needed to URL settings

---

## 🐛 Common Issues:

### Issue: "Connection refused" or timeout
**Cause**: API is not running
**Fix**: Start the API (see Step 3)

### Issue: "No such host is known" on Android
**Cause**: Using `localhost` instead of `10.0.2.2`
**Fix**: Configuration is already correct in `ApiConfig.cs`

### Issue: Categories endpoint returns empty array `[]`
**Cause**: Database migration not applied
**Fix**: Run `dotnet ef database update` (see Step 1)

### Issue: "Cannot connect to PostgreSQL server"
**Cause**: PostgreSQL is not running
**Fix**: Start PostgreSQL Docker container (see Step 2)

---

## 📋 Seeded Categories:

1. Electronics
2. Tools
3. Vehicles
4. Sports
5. Home & Garden
6. Photography
7. Music
8. Party & Events
9. Camping & Outdoor
10. Other

---

## Next Steps:

1. ✅ **Seed data has been added to the code**
2. ⏳ **Apply the migration** (Step 1)
3. ⏳ **Start PostgreSQL** (Step 2)
4. ⏳ **Start the API** (Step 3)
5. ⏳ **Test the MAUI app** (Step 4)
