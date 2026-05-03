# RentalApp Navigation Structure

## Overview
The app uses Shell-based navigation with a flyout menu for authenticated users.

## Navigation Flow

### 1. **App Startup**
- App starts on `LoginPage` (no flyout visible)
- User can navigate to `RegisterPage` to create an account

### 2. **After Login**
- User is redirected to `//ItemsListPage` (Browse Items)
- Flyout menu becomes visible with navigation options

### 3. **Main Navigation (Flyout Menu)**

#### **Marketplace** Section:
- **Browse Items** (`ItemsListPage`)
  - View all marketplace items
  - Search and filter by category/availability
  - Navigate to item details
  - Floating action button to create new item

- **Nearby Items** (`NearbyItemsPage`)
  - Location-based search
  - Use GPS or manual coordinates
  - Adjustable search radius

#### **My Account** Section:
- **My Items** (`MyItemsPage`)
  - Same as Browse Items, filtered to user's items
  - Can edit/delete own items

- **My Rentals** (`RentalsPage`)
  - Two tabs: "My Rentals" (as renter) / "My Listings" (as owner)
  - Manage rental requests and active rentals
  - Approve/complete/cancel actions

### 4. **Detail Pages (Push Navigation)**

These pages are accessed via navigation from list pages:

- **Item Detail** (`ItemDetailPage`)
  - View full item details, ratings, location
  - Request rental with date selection
  - Edit/delete (if owner)

- **Create/Edit Item** (`CreateItemPage` / `EditItemPage`)
  - Create new item or edit existing
  - Optional location (GPS or manual)
  - Category selection, pricing

### 5. **Logout**
- Logout menu item in flyout
- Returns user to `LoginPage`
- Flyout is hidden again

## Route Registration

### Shell Routes (in AppShell.xaml):
- `LoginPage` - Hidden from flyout
- `ItemsListPage` - Main landing page
- `NearbyItemsPage`
- `MyItemsPage` - Reuses ItemsListPage with filter
- `RentalsPage`

### Programmatic Routes (in AppShell.xaml.cs):
- `RegisterPage` - Registration form
- `ItemDetailPage` - Item details and rental request
- `CreateItemPage` - Create new item
- `EditItemPage` - Edit existing item (same page as Create)

## Navigation Commands

### From ItemsListPage:
- Tap item → Navigate to `ItemDetailPage?itemId={id}`
- Toolbar "Nearby" → Navigate to `NearbyItemsPage`
- Toolbar "My Items" → Navigate to `MyItemsPage`
- Toolbar "Rentals" → Navigate to `RentalsPage`
- FAB (+) → Navigate to `CreateItemPage`

### From ItemDetailPage:
- "Edit" button (owner) → Navigate to `EditItemPage?itemId={id}`
- "Request Rental" → Navigate to rental creation (future enhancement)

### From NearbyItemsPage:
- Tap item → Navigate to `ItemDetailPage?itemId={id}`

### From RentalsPage:
- "Leave Review" → Navigate to review page (future enhancement)

## Flyout Behavior

**Disabled (Hidden):**
- LoginPage
- RegisterPage

**Enabled (Visible):**
- All authenticated pages (ItemsListPage, NearbyItemsPage, RentalsPage, etc.)

## Tips for Development

1. **Adding New Pages:**
   - Add to `AppShell.xaml` if it should be in the main menu
   - Register in `AppShell.xaml.cs` constructor if it's a detail/modal page
   - Use `//` prefix for absolute navigation (e.g., `//ItemsListPage`)
   - Use relative paths for push navigation (e.g., `ItemDetailPage?itemId=5`)

2. **Query Parameters:**
   - Use `[QueryProperty(nameof(PropertyName), "queryKey")]` on ViewModels
   - Pass parameters in navigation: `NavigateToAsync("Page?key=value")`

3. **Back Navigation:**
   - Use `NavigateBackAsync()` in NavigationService
   - Or `await Shell.Current.GoToAsync("..")`

## Current Limitations

- No persistent authentication (user must login each time app starts)
- Icons referenced in flyout may not exist (shop.png, location.png, etc.)
- Some navigation paths (reviews, rental details) are placeholders for future work
