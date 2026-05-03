# 🗺️ Location Permissions & Emulator Setup Guide

## ✅ What Was Fixed:

### 1. **BoolToTextConverter Missing**

**Problem:**
```xaml
<Button Text="{Binding IsEditMode, Converter={StaticResource BoolToTextConverter}, ...}"
```
Showed "False" because the converter didn't exist.

**Solution:**
- ✅ Created `BoolToTextConverter.cs`
- ✅ Registered in `App.xaml`

**How it works:**
```csharp
// IsEditMode = true  → "Update Item"
// IsEditMode = false → "Create Item"
ConverterParameter='Update Item|Create Item'
```

---

### 2. **Location Permissions Added**

**Updated:** `Platforms/Android/AndroidManifest.xml`

Added permissions:
```xml
<!-- Location Permissions -->
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-feature android:name="android.hardware.location" android:required="false" />
<uses-feature android:name="android.hardware.location.gps" android:required="false" />
<uses-feature android:name="android.hardware.location.network" android:required="false" />
```

---

## 🚀 How to Test Location in Android Emulator

### Step 1: Rebuild the App

Since you modified `AndroidManifest.xml`, you need to rebuild:

1. **Stop the app** (Shift+F5)
2. **Clean solution**: Build → Clean Solution
3. **Rebuild**: Build → Rebuild Solution
4. **Run**: Press F5

---

### Step 2: Set Emulator Location

The Android emulator doesn't have a real GPS, so you need to set a fake location:

#### **Option A: Using Extended Controls (Recommended)**

1. **While the emulator is running**, click the **three dots (...)** on the emulator toolbar
2. Go to **Location** tab
3. Enter coordinates or use the map:
   - **London coordinates:** `51.5074, -0.1278`
   - **Your custom location:** Search on the map
4. Click **Send**

#### **Option B: Using Command Line**

```powershell
# Set location to London
adb emu geo fix -0.1278 51.5074

# Or use different coordinates
adb emu geo fix <longitude> <latitude>
```

---

### Step 3: Grant Permission in App

When you click **"Use Current Location"** button:

1. App will request location permission
2. Android dialog appears: **"Allow RentalApp to access this device's location?"**
3. Click **"Allow"** or **"Allow only while using the app"**

---

### Step 4: Verify Location Works

After granting permission:
- ✅ Latitude field should populate
- ✅ Longitude field should populate
- ✅ Address field should populate (via reverse geocoding)

Example result:
```
Latitude: 51.5074
Longitude: -0.1278
Address: Westminster, London SW1A, UK
```

---

## 🐛 Troubleshooting:

### Issue: Permission dialog doesn't appear

**Cause:** App already denied permission  
**Fix:** Reset app permissions

```powershell
# Uninstall and reinstall
adb uninstall com.companyname.starterapp

# Then run app again from Visual Studio
```

Or manually in emulator:
1. Settings → Apps → RentalApp
2. Permissions → Location
3. Change to "Allow"

---

### Issue: Location is null/undefined

**Causes:**
1. Emulator location not set
2. Location services disabled

**Fix:**

**A. Set emulator location:**
- Emulator toolbar → ... → Location → Send coordinates

**B. Enable location services:**
1. Emulator Settings
2. Location
3. Toggle **Use location** ON

---

### Issue: "Unable to get current location"

**Check:**
1. Location permission granted
2. GPS enabled in emulator settings
3. Location set in emulator extended controls

**Debug:**
Add breakpoint in `LocationService.GetCurrentLocationAsync()` to see the actual error.

---

### Issue: Address is empty but coordinates work

**Cause:** Geocoding API failed (network issue or no internet)

**Fix:**
- Emulator needs internet access
- Check network in emulator (open Chrome)
- Geocoding requires Google Play Services (should work in standard emulator)

---

### Issue: Button still shows "False"

**Cause:** Old build cache  
**Fix:**
1. Stop app
2. Clean solution
3. Delete `bin` and `obj`:
   ```powershell
   Remove-Item -Recurse -Force StarterApp\bin, StarterApp\obj
   ```
4. Rebuild and run

---

## 📝 Permission Flow in Code:

### CreateItemViewModel.cs
```csharp
[RelayCommand]
private async Task UseCurrentLocationAsync()
{
    // 1. Check permission
    var hasPermission = await _locationService.CheckLocationPermissionAsync();

    // 2. Request if not granted
    if (!hasPermission)
        hasPermission = await _locationService.RequestLocationPermissionAsync();

    // 3. Abort if denied
    if (!hasPermission)
        return;

    // 4. Get location
    var location = await _locationService.GetCurrentLocationAsync();

    // 5. Get address (reverse geocoding)
    var address = await _locationService.GetAddressFromCoordinatesAsync(...);
}
```

### LocationService.cs
```csharp
public async Task<bool> RequestLocationPermissionAsync()
{
    // Shows Android permission dialog
    var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
    return status == PermissionStatus.Granted;
}
```

---

## 🗺️ Testing Different Locations:

### Popular UK Cities:
```powershell
# London
adb emu geo fix -0.1278 51.5074

# Manchester
adb emu geo fix -2.2426 53.4808

# Birmingham
adb emu geo fix -1.8904 52.4862

# Edinburgh
adb emu geo fix -3.1883 55.9533
```

### Format:
```
adb emu geo fix <longitude> <latitude>
```
⚠️ **Note:** Longitude first, then latitude!

---

## ✅ Success Checklist:

- [x] `BoolToTextConverter` created
- [x] Converter registered in `App.xaml`
- [x] Location permissions added to `AndroidManifest.xml`
- [ ] App rebuilt after manifest changes
- [ ] Emulator location set (Extended Controls)
- [ ] Permission granted in app
- [ ] Location populates successfully
- [ ] Button shows "Create Item" or "Update Item" (not "False")

---

## 🎯 Quick Test:

### 1. Set Emulator Location
```powershell
adb emu geo fix -0.1278 51.5074
```

### 2. Run App
Press F5 in Visual Studio

### 3. Navigate to Create Item
Login → Menu → Create Item

### 4. Click "Use Current Location"
- Permission dialog appears
- Click "Allow"
- Fields populate:
  - Latitude: `51.5074`
  - Longitude: `-0.1278`
  - Address: `Westminster, London...`

### 5. Check Button Text
Should show:
- **"Create Item"** when creating new
- **"Update Item"** when editing existing

---

## 🔒 Permissions Summary:

### Android Permissions Added:
```xml
ACCESS_COARSE_LOCATION    - Approximate location
ACCESS_FINE_LOCATION      - Precise GPS location
```

### MAUI Permission Used:
```csharp
Permissions.LocationWhenInUse
```

This requests permission only when app is in use (not background tracking).

---

## 📚 Additional Resources:

- **MAUI Geolocation:** https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/device/geolocation
- **MAUI Permissions:** https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/appmodel/permissions
- **Android Emulator GPS:** https://developer.android.com/studio/run/emulator-console#geo

---

**Now rebuild the app and test! The button should show correct text and location should work after granting permission.** 🎉
