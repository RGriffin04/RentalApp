# ✅ RentalApp - Complete Docker Setup

## 🎯 What Changed:

### Before:
- ❌ API had to be run manually with `dotnet run`
- ❌ PostgreSQL in Docker, API on host
- ❌ Manual migration management

### After:
- ✅ **Everything runs in Docker**
- ✅ One command to start: `.\start-api.ps1`
- ✅ Automatic migrations on startup
- ✅ Hot reload for API development
- ✅ Consistent environment for all developers

---

## 🐳 Docker Compose Services:

### 1. **db** - PostgreSQL + PostGIS
```yaml
Image: postgis/postgis:16-3.4
Port: 5432:5432
Volume: postgres_data (persistent)
Health Check: Enabled
```

### 2. **migrate** - EF Core Migrations
```yaml
Runs: dotnet run --project StarterApp.Migrations
Depends on: db (healthy)
Runs: Once on startup
```

### 3. **api** - RentalApp API
```yaml
Port: 5000:8080
URL: http://localhost:5000
Hot Reload: Enabled (dotnet watch)
Depends on: db + migrate complete
Environment: Development
```

---

## 📁 Files Modified/Created:

### Modified:
1. **`docker-compose.yml`**
   - Added `api` service
   - Updated `migrate` service connection string
   - Configured environment variables

2. **`start-api.ps1`**
   - Now uses `docker-compose up`
   - Shows service status
   - Displays logs

3. **`test-jwt-auth.ps1`**
   - Checks Docker services before testing
   - Auto-starts if not running

4. **`QUICK_START.md`**
   - Updated for Docker workflow

### Created:
1. **`DOCKER_SETUP.md`** - Complete Docker documentation
2. **`stop-api.ps1`** - Stop services script

---

## 🚀 Quick Start Guide:

### Step 1: Start Everything
```powershell
.\start-api.ps1
```

**What happens:**
1. ✅ Builds Docker images (first time only)
2. ✅ Starts PostgreSQL
3. ✅ Waits for database to be healthy
4. ✅ Runs migrations automatically
5. ✅ Starts API with hot reload
6. ✅ Shows service status and logs

### Step 2: Verify Services
```powershell
docker-compose ps
```

**Expected output:**
```
NAME                 SERVICE   STATUS        PORTS
starterapp-api-1     api       running       0.0.0.0:5000->8080/tcp
starterapp-db-1      db        running (healthy)  0.0.0.0:5432->5432/tcp
starterapp-migrate-1 migrate   exited (0)
```

### Step 3: Test API
```powershell
.\test-jwt-auth.ps1
```

**Expected:**
- ✅ API is running
- ✅ User registered
- ✅ Login successful
- ✅ 10 categories fetched

### Step 4: Run MAUI App
1. Open Visual Studio
2. Press F5
3. Login with: `testuser@rental.app` / `Test123!`
4. Categories should load! 🎉

---

## 🔄 Development Workflow:

### Making API Changes:

1. **Edit code** in `RentalApp.API/`
2. **Save file**
3. **Watch logs:**
   ```powershell
   docker-compose logs -f api
   ```
4. **See hot reload:**
   ```
   dotnet watch ⌚ File changed: AuthController.cs
   dotnet watch 🔥 Hot reload succeeded
   ```
5. **Test immediately** - No manual restart needed!

### Making Database Changes:

1. **Update models** in `StarterApp.Database/Models/`
2. **Create migration:**
   ```powershell
   dotnet ef migrations add YourMigrationName --project StarterApp.Database --startup-project StarterApp.Migrations
   ```
3. **Restart services:**
   ```powershell
   docker-compose restart migrate api
   ```
   Or manually apply:
   ```powershell
   docker-compose run --rm migrate
   ```

---

## 📊 Service Architecture:

```
┌─────────────────────────────────────┐
│          Docker Compose             │
│                                     │
│  ┌──────────┐    ┌──────────┐     │
│  │    db    │◄───│ migrate  │     │
│  │ PostgreSQL│    │EF Migrations│  │
│  └────┬─────┘    └──────────┘     │
│       │                             │
│       │         ┌──────────┐       │
│       └────────►│   api    │       │
│                 │RentalApp │       │
│                 └────┬─────┘       │
└──────────────────────┼─────────────┘
                       │
                       ↓ port 5000
               ┌───────────────┐
               │   MAUI App    │
               └───────────────┘
```

---

## 🎯 Environment Variables:

### API Container:
```bash
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__DefaultConnection=Host=db;Port=5432;Username=app_user;Password=app_password;Database=appdb
JWT_SECRET=CHANGE_ME_TO_A_LONG_RANDOM_SECRET_AT_LEAST_32_CHARS
```

**Note:** `db` in the connection string refers to the Docker service name, not `localhost`.

---

## 🔧 Useful Commands:

### Start/Stop
```powershell
# Start all services
.\start-api.ps1

# Stop all services
.\stop-api.ps1

# Or use docker-compose
docker-compose up -d       # Start in background
docker-compose down        # Stop and remove
docker-compose down -v     # Stop and remove + delete volumes
```

### Logs
```powershell
# View all logs
docker-compose logs

# Follow API logs
docker-compose logs -f api

# Last 50 lines of API logs
docker-compose logs --tail=50 api
```

### Service Management
```powershell
# View running services
docker-compose ps

# Restart API only
docker-compose restart api

# Rebuild API only
docker-compose up -d --build api

# Run migrations manually
docker-compose run --rm migrate
```

### Database Access
```powershell
# psql into database
docker-compose exec db psql -U app_user -d appdb

# Check categories
docker-compose exec db psql -U app_user -d appdb -c "SELECT * FROM category;"

# Backup database
docker-compose exec db pg_dump -U app_user appdb > backup.sql
```

---

## 🐛 Troubleshooting:

### Issue: API won't start
**Check logs:**
```powershell
docker-compose logs api
```

**Common causes:**
- Port 5000 in use
- Database not ready
- Build errors

**Solution:**
```powershell
docker-compose down
docker-compose up -d --build
```

### Issue: Migrations not running
**Check migration logs:**
```powershell
docker-compose logs migrate
```

**Manually run migrations:**
```powershell
docker-compose run --rm migrate
```

### Issue: Database connection failed
**Check database is healthy:**
```powershell
docker-compose ps db
```

**Should show:** `State: running (healthy)`

**Restart database:**
```powershell
docker-compose restart db
```

### Issue: Changes not reflecting
**Rebuild API:**
```powershell
docker-compose up -d --build api
```

**Clear everything and start fresh:**
```powershell
docker-compose down -v
docker-compose up -d --build
```

### Issue: Port 5000 already in use
**Find process using port:**
```powershell
Get-NetTCPConnection -LocalPort 5000 | Select OwningProcess
```

**Change port in docker-compose.yml:**
```yaml
ports:
  - "5001:8080"
```

**Update ApiConfig.cs:**
```csharp
public static readonly string BaseUrl = "http://localhost:5001";
```

---

## ✅ Benefits of Docker Setup:

### For Development:
- ✅ **One command setup** - `.\start-api.ps1`
- ✅ **Consistent environment** - Works same on all machines
- ✅ **Hot reload** - Changes reflected immediately
- ✅ **Automatic migrations** - No manual DB updates
- ✅ **Easy cleanup** - `docker-compose down`

### For Team:
- ✅ **No .NET installation needed** - Docker handles it
- ✅ **Same PostgreSQL version** - No "works on my machine"
- ✅ **Infrastructure as code** - `docker-compose.yml` in Git
- ✅ **Easy onboarding** - New developers up in minutes

### For Production:
- ✅ **Container-ready** - Easy to deploy to cloud
- ✅ **Scalable** - Can add load balancers, replicas
- ✅ **Portable** - Runs anywhere Docker runs

---

## 📝 Configuration Files:

### docker-compose.yml
```yaml
services:
  db:     PostgreSQL + PostGIS
  migrate: EF Core migrations
  api:     RentalApp API (hot reload)
```

### Scripts:
- **`start-api.ps1`** - Start all services
- **`stop-api.ps1`** - Stop all services  
- **`test-jwt-auth.ps1`** - Test JWT flow

---

## 🎉 Success Checklist:

- [x] Docker Compose configured
- [x] API service added
- [x] Automatic migrations enabled
- [x] Hot reload configured
- [x] Startup script created
- [x] Test script updated
- [x] Documentation complete
- [ ] **Run:** `.\start-api.ps1`
- [ ] **Test:** `.\test-jwt-auth.ps1`
- [ ] **MAUI App:** Press F5
- [ ] **Verify:** Categories load

---

## 🚀 Next Steps:

### Now:
```powershell
.\start-api.ps1
```

### Then:
```powershell
.\test-jwt-auth.ps1
```

### Finally:
Press **F5** in Visual Studio and login!

---

**Your RentalApp now runs completely in Docker! 🐳**
