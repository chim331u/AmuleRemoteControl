# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

AmuleRemoteGui (Mule Remote) is a .NET MAUI Blazor Hybrid application that provides remote control for aMule P2P file-sharing servers. The app runs on Android (primary) and Windows, allowing users to manage downloads, uploads, searches, and server connections through a mobile interface.

**Key Technology Stack:**
- .NET 9.0 (net9.0-android)
- MAUI Blazor Hybrid (Razor components hosted in WebView)
- Radzen.Blazor 7.1.3 (UI component library)
- HtmlAgilityPack (HTML parsing for aMule web interface)
- Serilog (logging)
- Plugin.Fingerprint (biometric authentication)

## Build & Run Commands

### Build for Android
```bash
dotnet build -f net9.0-android
```

### Build for Windows (on Windows machines only)
```bash
dotnet build -f net8.0-windows10.0.19041.0
```

### Run on Android Emulator/Device
```bash
dotnet build -t:Run -f net9.0-android
```

### Clean Build
```bash
dotnet clean
dotnet build
```

### Debug Build with Logs
The app uses Serilog with file logging enabled. Logs are written to:
- Location: `FileSystem.CacheDirectory/serilog_[date].log`
- Rolling interval: Daily
- Retention: 5 days

In DEBUG mode, Blazor WebView developer tools are available (`builder.Services.AddBlazorWebViewDeveloperTools()`)

## Architecture Overview

### Layered Architecture

```
MAUI Container (MainPage.xaml + BlazorWebView)
    ↓
Blazor UI Layer (Razor Components + Radzen)
    ↓
Service Layer (Dependency Injected)
    ↓
Data Layer (Models + File-based Configuration)
    ↓
Platform-Specific Layer (Android Deep Linking)
```

### Key Services (Dependency Injection)

All services are **Scoped** unless noted otherwise. Registration in `MauiProgram.cs`:

1. **IAmuleRemoteServices** (`aMuleRemoteService`)
   - Core aMule communication via HTML scraping
   - Downloads, uploads, search, server management
   - Statistics polling with event-driven state updates
   - Methods: `GetDownloading()`, `PostDownloadCommand()`, `SearchFiles()`, `GetStats()`, `GetPreferences()`
   - State: `Status` (Stats), `DownSpeed` (string) with change events

2. **IAccessService** (`AccessService`)
   - Authentication and authorization
   - Validates password against aMule web interface
   - Observable state: `IsAuthorized` property with `OnChange` event
   - Methods: `LoggedIn(LoginData)`, `LoggedOut()`, `CheckUrl()`

3. **IUtilityServices** (`UtilityServices`)
   - Settings persistence (network profiles, global settings, login data)
   - Formatting utilities (file sizes, currency, dates)
   - Biometric authentication support
   - Secure password storage via `SecureStorage.Default`
   - Methods: `ReadNetworkSettingJson()`, `WriteNetworkSettingJson()`, `SetApiUrl()`
   - Storage location: `FileSystem.AppDataDirectory/[SettingName].json`

4. **INetworkHelper** (`NetworkHelper`)
   - HTTP client management
   - GET/POST request handling with gzip decompression
   - Query parameter encoding
   - Returns HTML responses as strings

5. **SessionStorageAccessor**
   - JavaScript interop for browser sessionStorage
   - In-app logging stored in session memory
   - Lazy-loaded JS module

6. **Ed2kUrl** (Singleton)
   - Global state for deep-linked ed2k:// URLs
   - Property: `ed2kUrlData` (string)
   - Used by Android deep linking integration

### Directory Structure

```
AmuleRemoteGui/
├── Components/
│   ├── Routes.razor              # Router configuration
│   ├── _Imports.razor            # Global usings
│   ├── Layout/
│   │   └── MainLayout.razor      # Header/Footer navigation
│   ├── Pages/
│   │   ├── Home.razor            # Dashboard
│   │   ├── Access/LoginPage.razor
│   │   ├── Settings/Setting.razor
│   │   ├── Logging/Logger.razor
│   │   └── aMule/                # Core aMule pages
│   │       ├── DownloadingHome.razor
│   │       ├── UploadFileHome.razor
│   │       ├── ServerHome.razor
│   │       ├── SearchPage.razor
│   │       ├── AddLink.razor
│   │       ├── PreferencePage.razor
│   │       └── LogAmule.razor
│   ├── StatsComponent.razor      # Status display component
│   ├── Service/                  # Service implementations
│   ├── Interfaces/               # Service interfaces
│   ├── Data/
│   │   ├── AmuleModel/           # Domain models
│   │   └── Setting/              # Configuration models
├── Platforms/
│   └── Android/
│       ├── MainActivity.cs       # ed2k:// deep linking handler
│       ├── MainApplication.cs
│       └── AndroidManifest.xml
├── Resources/
├── wwwroot/
│   ├── index.html                # Blazor host page
│   ├── css/
│   └── Images/
├── App.xaml / App.xaml.cs
├── MainPage.xaml / MainPage.xaml.cs  # BlazorWebView host
└── MauiProgram.cs                # DI configuration
```

## Important Architectural Patterns

### 1. HTML Scraping Strategy
- aMule web interface is HTML-based (no REST API)
- Uses HtmlAgilityPack to parse HTML tables
- XPath/LINQ queries extract data from table rows
- String manipulation for IDs, speeds, percentages
- **Important:** Parsing logic is fragile to aMule UI changes
- Culture-aware number parsing respects CurrentCulture

### 2. State Management Pattern
- **Property with Event** pattern (not MVVM/Flux):
  ```csharp
  public Stats? Status
  {
      get => _status;
      set
      {
          _status = value;
          this.StatusChanged?.Invoke(this, EventArgs.Empty);
      }
  }
  public event EventHandler? StatusChanged;
  ```
- Components subscribe to service events
- Call `StateHasChanged()` when events fire
- No centralized state store

### 3. Multi-Profile Network Configuration
- Multiple aMule server profiles stored in JSON
- One marked as `IsActive`
- Dynamic API URL switching via `UtilityServices.SetApiUrl()`
- Allows switching between local/remote aMule instances

### 4. Authentication Flow
```
LoginPage → AccessService.LoggedIn(LoginData)
    ↓
NetworkHelper.SendRequest("?pass=...")
    ↓
Check HTML for "Enter password :"
    ↓
Set IsAuthorized → Trigger StateChanged
    ↓
Save credentials via UtilityServices (optional biometric)
```

Password sent as query parameter over HTTP (LAN security model).

### 5. Android Deep Linking
- `MainActivity` captures `ed2k://` protocol via IntentFilter
- Stores URL in `Ed2kUrl.ed2kUrlData` (singleton)
- Home page redirects to AddLink page when detected
- Enables share-to-app functionality for ed2k links

## Data Flow Examples

### Download Management
1. `DownloadingHome.razor` calls `aMuleRemoteService.GetDownloading()`
2. Service fetches `amuleweb-main-dload.php` HTML
3. Parses table rows into `List<DownloadFile>`
4. Component binds to RadzenDataGrid
5. User action triggers `PostDownloadCommand(fileId, command)`
6. Form-encoded POST sent to aMule server

### Settings Persistence
1. `Setting.razor` calls `UtilityServices.ReadNetworkSettingJson()`
2. Loads from `FileSystem.AppDataDirectory/NetworkSetting.json`
3. User modifies settings in UI
4. `WriteNetworkSettingJson()` saves to file
5. `SetApiUrl()` updates active API endpoint in `IAmuleRemoteServices`

### Status Updates (Event-Driven)
1. MainLayout subscribes to `_service.StatusChanged` and `_service.DownChanged`
2. Auto-refresh components poll `aMuleRemoteService.AutoStatus()`
3. Setting `Status` property triggers `StatusChanged` event
4. MainLayout event handler calls `StateHasChanged()`
5. UI re-renders with new connection status

## Development Notes

### Adding New aMule Features
1. Identify HTML endpoint in aMule web interface (e.g., `amuleweb-main-*.php`)
2. Create domain model in `Components/Data/AmuleModel/`
3. Add parsing method in `aMuleRemoteService` using HtmlAgilityPack
4. Create Razor page in `Components/Pages/aMule/`
5. Use Radzen components for consistent UI
6. Inject `IAmuleRemoteServices` and call parsing method
7. Bind data to RadzenDataGrid or RadzenCards

### Working with Settings
- All settings are JSON files in `FileSystem.AppDataDirectory`
- Use `IUtilityServices` methods for read/write
- Passwords stored separately in `SecureStorage.Default` (not in JSON)
- NetworkSetting supports multiple profiles with `IsActive` flag

### Event-Driven Updates
- When modifying service state properties, always invoke change events
- Components must call `StateHasChanged()` in event handlers
- Use `InvokeAsync()` if updating from background thread

### Platform-Specific Code
- Android code in `Platforms/Android/`
- Deep linking requires `MainActivity` modifications
- Permissions in `AndroidManifest.xml`
- Use `#if ANDROID` preprocessor directives when needed

### Logging
- Inject `ILogger<T>` for structured logging
- Use `Log.Information()`, `Log.Error()`, etc. (Serilog)
- Logs written to cache directory with daily rotation
- Session logs accessible via `SessionStorageAccessor` and Logger page

## Security Considerations

- HTTP (not HTTPS) for local network connections
- `android:usesCleartextTraffic="true"` in manifest
- Passwords sent as query parameters to aMule
- Biometric authentication available for app unlock
- Secure storage for saved passwords
- Assumes trusted LAN environment (no end-to-end encryption)

## Common Pitfalls

1. **HTML Parsing Fragility**: aMule UI changes break parsing logic. Test thoroughly after aMule updates.
2. **Culture-Specific Parsing**: Number parsing uses CurrentCulture. Italian culture (it-IT) common in codebase.
3. **Scoped Service Lifetimes**: Services recreated per request. Use events for cross-component state.
4. **JavaScript Interop Timing**: `SessionStorageAccessor` requires awaiting module import before use.
5. **Deep Linking State**: `Ed2kUrl.ed2kUrlData` is global static. Clear after use to prevent re-processing.
6. **File Paths**: Always use `FileSystem.AppDataDirectory` or `FileSystem.CacheDirectory` for cross-platform compatibility.

## Target Platforms

- **Primary**: Android 35 (net9.0-android)
- **Secondary**: Windows 10.0.19041.0 (net8.0-windows10.0.19041.0)
- **Potential**: Tizen (commented out in csproj)

Build commands must specify `-f` flag for target framework.
