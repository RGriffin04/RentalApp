# PostGIS Fix and Database Setup Guide

## ✅ What Was Fixed:

1. **docker-compose.yml** - Changed from `postgres:16` to `postgis/postgis:16-3.4`
2. **AppDbContext.cs** - Added warning suppression (though we still need to create the migration properly)

---

## 🔧 Steps to Fix Everything:

### Step 1: Restart Docker with PostGIS Image

Run these commands in PowerShell:

```powershell
cd "C:\software engineering CW\code\StarterApp"

# Stop and remove old containers
docker-compose down

# Start the new PostGIS database container
docker-compose up -d db

# Wait for it to be healthy (about 10-15 seconds)
Start-Sleep -Seconds 15

# Verify it's running
docker ps
```

You should see `postgis/postgis:16-3.4` in the IMAGE column now.

---

### Step 2: Apply All Migrations

```powershell
cd "C:\software engineering CW\code\StarterApp"
dotnet ef database update --project StarterApp.Database --startup-project StarterApp.Migrations
```

**Expected output:**
```
Build started...
Build succeeded.
Applying migration '20260210141124_InitialCreate'.
Applying migration '20260502191447_AddRentalTables'.
Applying migration '20260503115021_AddPostGISLocationToItems'.
Done.
```

---

### Step 3: Create the SeedCategories Migration (In Visual Studio)

Since command-line migration creation isn't working, use Visual Studio:

1. **Tools → NuGet Package Manager → Package Manager Console**
2. Set **Default project** to `StarterApp.Database`
3. Run:
```powershell
Add-Migration SeedCategories -StartupProject StarterApp.Migrations
```
4. Then:
```powershell
Update-Database -StartupProject StarterApp.Migrations
```

---

### Alternative Step 3: Insert Categories via SQL (Faster)

If you don't want to mess with migrations right now, just insert the data directly:

```powershell
Get-Content "C:\software engineering CW\code\StarterApp\seed-categories.sql" | docker exec -i starterapp-db-1 psql -U app_user -d appdb
```

**Expected output:**
```
INSERT 0 10
setval
--------
    10
```

---

### Step 4: Verify Categories Were Inserted

```powershell
docker exec -i starterapp-db-1 psql -U app_user -d appdb -c 'SELECT * FROM "Categories";'
```

You should see all 10 categories listed.

---

### Step 5: Start the API

```powershell
cd "C:\software engineering CW\code\StarterApp\RentalApp.API"
dotnet run
```

**Open browser:** http://localhost:5000/api/categories

You should see JSON with all 10 categories.

---

### Step 6: Test the MAUI App

Run your MAUI app from Visual Studio and check if categories now appear in the picker!

---

## 🎯 Quick Test Commands:

```powershell
# Check if PostGIS container is running
docker ps | Select-String "postgis"

# Check if database has PostGIS extension
docker exec -i starterapp-db-1 psql -U app_user -d appdb -c '\dx'

# Check if categories exist
docker exec -i starterapp-db-1 psql -U app_user -d appdb -c 'SELECT COUNT(*) FROM "Categories";'
```

---

## 🐛 Troubleshooting:

### Error: "extension postgis is not available"
**Fix:** Make sure you ran `docker-compose down` then `docker-compose up -d db` to get the new PostGIS image

### Error: "Failed to connect"
**Fix:** Wait longer for the database to start (try 20-30 seconds)

### Categories still empty
**Fix:** Run the SQL script from Alternative Step 3 above

### Migration warning still appearing
**Fix:** This is okay if you're using the SQL script approach. If you want a proper migration, use Package Manager Console in Visual Studio.

---

## 📋 Summary:

1. ✅ **docker-compose.yml updated** to use PostGIS image
2. ⏳ **Restart Docker** with new image
3. ⏳ **Apply migrations** (or run SQL script)
4. ⏳ **Start API** and test
5. ⏳ **Test MAUI app**
