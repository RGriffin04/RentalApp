# 🐳 Running RentalApp API in Docker

## ✅ What's Configured:

The `docker-compose.yml` now includes three services:

1. **`db`** - PostgreSQL with PostGIS (port 5432)
2. **`migrate`** - Runs EF Core migrations automatically
3. **`api`** - RentalApp API (port 5000)

---

## 🚀 Quick Start:

### Option 1: Use the startup script (Recommended)
```powershell
.\start-api.ps1
```

This will:
- ✅ Check Docker is running
- ✅ Build and start all services
- ✅ Run migrations
- ✅ Start the API
- ✅ Show service status and logs

### Option 2: Manual Docker Compose
```powershell
# Build and start all services
docker-compose up -d --build

# View logs
docker-compose logs -f api

# Stop services
docker-compose down
```

---

## 📋 Service Details:

### PostgreSQL (db)
- **Image:** `postgis/postgis:16-3.4`
- **Port:** `5432:5432`
- **Connection:** `Host=db;Port=5432;Username=app_user;Password=app_password;Database=appdb`
- **Volume:** `postgres_data` (persistent storage)

### Migrations (migrate)
- **Runs:** EF Core migrations on startup
- **Depends on:** PostgreSQL healthy
- **Runs once:** `restart: no`

### RentalApp API (api)
- **Port:** `5000:8080` (Host:Container)
- **URL:** http://localhost:5000
- **Environment:** Development
- **Hot Reload:** Enabled with `dotnet watch`
- **Depends on:** PostgreSQL + Migrations complete

---

## 🔧 Docker Compose Configuration:

### API Service Environment Variables:
```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Development
  - ASPNETCORE_URLS=http://+:8080
  - ConnectionStrings__DefaultConnection=Host=db;Port=5432;Username=app_user;Password=app_password;Database=appdb
  - JWT_SECRET=CHANGE_ME_TO_A_LONG_RANDOM_SECRET_AT_LEAST_32_CHARS
```

### Volumes:
```yaml
volumes:
  - .:/workspace:cached           # Source code (hot reload)
  - nuget_cache:/root/.nuget/packages  # NuGet cache (faster builds)
```

### Ports:
```yaml
ports:
  - "5000:8080"  # Map host port 5000 to container port 8080
```

---

## 🎯 Common Commands:

### Start Services
```powershell
# Start all services (detached)
docker-compose up -d

# Start with logs visible
docker-compose up

# Rebuild and start
docker-compose up -d --build
```

### View Logs
```powershell
# All services
docker-compose logs

# API only (follow mode)
docker-compose logs -f api

# Last 50 lines
docker-compose logs --tail=50 api
```

### Manage Services
```powershell
# View running services
docker-compose ps

# Restart API only
docker-compose restart api

# Stop all services
docker-compose stop

# Stop and remove containers
docker-compose down

# Stop and remove everything (including volumes)
docker-compose down -v
```

### Access Containers
```powershell
# Shell into API container
docker-compose exec api bash

# Shell into PostgreSQL
docker-compose exec db bash

# Run psql in database
docker-compose exec db psql -U app_user -d appdb
```

---

## 🧪 Testing the Dockerized API:

### Test 1: Check API is running
```powershell
# Windows PowerShell
Invoke-RestMethod -Uri "http://localhost:5000/auth/token" -Method OPTIONS

# Expected: No error (API is reachable)
```

### Test 2: Run JWT authentication test
```powershell
.\test-jwt-auth.ps1
```

Expected output:
- ✅ API is running
- ✅ User registered
- ✅ Login successful
- ✅ Categories fetched with JWT token

### Test 3: Check database
```powershell
docker-compose exec db psql -U app_user -d appdb -c "SELECT * FROM category;"
```

Expected: 10 categories listed

---

## 🔄 Hot Reload:

The API runs with `dotnet watch`, so changes to `.cs` files will automatically rebuild and restart the API.

### How it works:
1. Edit a file in `RentalApp.API/`
2. Save the file
3. Watch the logs: `docker-compose logs -f api`
4. API rebuilds and restarts automatically

### To see hot reload in action:
```powershell
# Terminal 1: Watch API logs
docker-compose logs -f api

# Terminal 2: Make a code change
code RentalApp.API/Controllers/AuthController.cs
# Make a small change and save

# Terminal 1: You'll see:
# dotnet watch ⌚ File changed: AuthController.cs
# dotnet watch 🔥 Hot reload succeeded
```

---

## 🐛 Troubleshooting:

### Issue: API fails to start
**Check logs:**
```powershell
docker-compose logs api
```

**Common causes:**
- Port 5000 already in use
- Database not ready (check `docker-compose logs db`)
- Build errors (check compilation errors in logs)

**Solution:**
```powershell
# Stop everything
docker-compose down

# Rebuild from scratch
docker-compose up -d --build
```

---

### Issue: Database connection errors
**Symptoms:**
```
Npgsql.NpgsqlException: Failed to connect to db:5432
```

**Check database is healthy:**
```powershell
docker-compose ps db
```

**Should show:**
```
State: running (healthy)
```

**If unhealthy, check logs:**
```powershell
docker-compose logs db
```

---

### Issue: Migrations not running
**Check migration service:**
```powershell
docker-compose ps migrate
```

**View migration logs:**
```powershell
docker-compose logs migrate
```

**Manually run migrations:**
```powershell
docker-compose run --rm migrate
```

---

### Issue: Port 5000 already in use
**Find what's using port 5000:**
```powershell
Get-NetTCPConnection -LocalPort 5000 | Select OwningProcess
```

**Stop the conflicting process or change the port:**
```yaml
# In docker-compose.yml
ports:
  - "5001:8080"  # Use port 5001 instead
```

Then update `ApiConfig.cs`:
```csharp
public static readonly string BaseUrl = "http://localhost:5001";
```

---

### Issue: Changes not reflecting
**Rebuild the container:**
```powershell
docker-compose up -d --build api
```

**Clear NuGet cache:**
```powershell
docker-compose down
docker volume rm starterapp_nuget_cache
docker-compose up -d --build
```

---

## 📊 Service Startup Order:

```
1. db (PostgreSQL)
   ↓ waits for healthy
2. migrate (EF Core migrations)
   ↓ waits for completion
3. api (RentalApp API)
```

This ensures:
- ✅ Database is ready before migrations run
- ✅ Migrations complete before API starts
- ✅ API has all tables and seed data

---

## 🔒 Security Notes:

### Development Setup (Current):
- ⚠️ Database password in plain text
- ⚠️ JWT secret in docker-compose.yml
- ✅ OK for local development

### Production Setup (TODO):
```yaml
environment:
  - ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
  - JWT_SECRET=${JWT_SECRET}
```

Then use `.env` file:
```bash
DB_CONNECTION_STRING=Host=db;Port=5432;Username=app_user;Password=SecurePassword123;Database=appdb
JWT_SECRET=YourVerySecureRandomSecretKeyHere
```

---

## 📝 Environment Variables:

### Available in API container:
```bash
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__DefaultConnection=Host=db;Port=5432;Username=app_user;Password=app_password;Database=appdb
JWT_SECRET=CHANGE_ME_TO_A_LONG_RANDOM_SECRET_AT_LEAST_32_CHARS
```

### To add more:
Edit `docker-compose.yml` → `api` → `environment` section

---

## ✅ Benefits of Dockerized API:

- ✅ **Consistent environment** - Same setup on all machines
- ✅ **Easy setup** - One command to start everything
- ✅ **Isolated** - Doesn't interfere with local .NET installations
- ✅ **Hot reload** - Changes reflected immediately
- ✅ **Service dependencies** - PostgreSQL always starts first
- ✅ **Easy cleanup** - `docker-compose down` stops everything
- ✅ **Version control** - `docker-compose.yml` tracks infrastructure

---

## 🎉 You're All Set!

### Quick Start:
```powershell
.\start-api.ps1
```

### Test:
```powershell
.\test-jwt-auth.ps1
```

### Run MAUI App:
Press **F5** in Visual Studio

---

## 📚 Additional Resources:

- **Docker Compose Docs:** https://docs.docker.com/compose/
- **ASP.NET Core in Docker:** https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/
- **PostgreSQL Docker:** https://hub.docker.com/_/postgres
- **PostGIS Docker:** https://hub.docker.com/r/postgis/postgis
