# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

AmuleRemoteGui (Mule Remote) is a .NET MAUI Blazor Hybrid application that provides remote control for aMule P2P file-sharing servers. The app runs on Android (primary) and Windows, allowing users to manage downloads, uploads, searches, and server connections through a mobile interface.

**Key Technology Stack:**
- .NET 10.0 (net10.0-android primary target)
- MAUI Blazor Hybrid (Razor components hosted in WebView)
- Radzen.Blazor 8.3.6 (UI component library)
- HtmlAgilityPack 1.12.4 (HTML parsing for aMule web interface)
- Serilog 4.3.0 (logging with file sink)
- Plugin.Fingerprint 2.1.5 (biometric authentication)

## Build & Run Commands

### Build for Android
```bash
dotnet build -f net10.0-android
```

### Build for iOS (on macOS only)
```bash
dotnet build -f net10.0-ios
```

### Build for macOS Catalyst (on macOS only)
```bash
dotnet build -f net10.0-maccatalyst
```

### Build for Windows (on Windows machines only)
```bash
dotnet build -f net10.0-windows10.0.19041.0
```

### Run on Android Emulator/Device
```bash
dotnet build -t:Run -f net10.0-android
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

All services are **Scoped** unless noted otherwise. Registration in `MauiProgram.cs` (see lines 29-54):

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
   - HTTP client management via IHttpClientFactory
   - GET/POST request handling with gzip decompression
   - Query parameter encoding
   - Returns HTML responses as strings
   - Configured with 30-second timeout
   - Automatic decompression: GZip | Deflate

5. **SessionStorageAccessor** (Scoped)
   - JavaScript interop for browser sessionStorage
   - In-app logging stored in session memory
   - Lazy-loaded JS module

6. **ICultureProvider** (Singleton)
   - Manages application culture/localization settings
   - Methods: `GetCulture()`, `SetCulture(string)`
   - Supports 5 cultures: en-US, it-IT, de-DE, fr-FR, es-ES
   - Persisted in GlobalSettings.json

7. **IEd2kUrlParser** (Scoped)
   - Parses and validates ed2k:// URLs
   - Method: `Result<Ed2kLink> Parse(string url)`
   - Returns structured Ed2kLink with FileName, FileSize, FileHash
   - Comprehensive validation (hash format, file size limits, URL encoding)

8. **IDeepLinkService** (Singleton)
   - Event-driven deep link handling (replaces obsolete Ed2kUrl singleton)
   - Event: `LinkReceived` with DeepLinkEventArgs
   - Method: `NotifyLinkReceived(string url, DeepLinkSource source)`
   - Coordinates MainActivity → Home → AddLink navigation flow

9. **Radzen Services** (Scoped)
   - `DialogService`: Modal dialogs and confirmations
   - `NotificationService`: Toast notifications
   - `TooltipService`: Tooltip management
   - `ContextMenuService`: Context menu handling

10. **IFingerprint** (Singleton)
   - Biometric authentication via Plugin.Fingerprint
   - Registered as `CrossFingerprint.Current`

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
- **Property with Event** pattern (not MVVM/Flux) with **Thread Safety** (v2.1+):
  ```csharp
  // Thread-safe lock object
  private readonly object _statusLock = new object();

  public Stats? Status
  {
      get
      {
          lock (_statusLock)
          {
              return _status;
          }
      }
      set
      {
          lock (_statusLock)
          {
              _status = value;
              // Fire event inside lock to ensure consistency
              this.StatusChanged?.Invoke(this, EventArgs.Empty);
          }
      }
  }
  public event EventHandler? StatusChanged;
  ```
- **Standard Event Pattern:** All events use `EventHandler?` (not `Action?`)
  - Event signature: `public event EventHandler? EventName;`
  - Invocation: `EventName?.Invoke(this, EventArgs.Empty);`
  - Subscribers: `service.EventName += OnEventHandler;` where handler is `void OnEventHandler(object? sender, EventArgs e)`
- **Thread Safety:** All state properties use lock objects to prevent race conditions
  - Each service has a dedicated lock object (e.g., `_statusLock`, `_authLock`)
  - Both getter and setter are protected by locks
  - Events are fired inside the lock to ensure atomic state changes
- Components subscribe to service events
- Call `StateHasChanged()` when events fire, wrapped in `InvokeAsync()` for thread safety
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

### 5. Android Deep Linking (Event-Driven Architecture)
- `MainActivity` captures `ed2k://` protocol via IntentFilter
- Validates URL using `IEd2kUrlParser.Parse()` with Result<T> pattern
- Fires `IDeepLinkService.LinkReceived` event (no global state)
- Home.razor subscribes to event and navigates to AddLink page
- Supports cold start (app closed) and warm start (app running) scenarios
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
5. **Deep Linking State**: Deep links are event-driven via `IDeepLinkService` (no global state). Components must subscribe/unsubscribe properly to prevent memory leaks.
6. **File Paths**: Always use `FileSystem.AppDataDirectory` or `FileSystem.CacheDirectory` for cross-platform compatibility.

## Target Platforms

- **Primary**: Android 24+ (net10.0-android, minimum SDK 24)
- **Secondary**: Windows 10.0.17763.0+ (net10.0-windows10.0.19041.0)
- **Additional**: iOS 15.0+ (net10.0-ios), macOS Catalyst 15.0+ (net10.0-maccatalyst)

Build commands must specify `-f` flag for target framework. iOS and macOS Catalyst builds require macOS development environment.

## Project Status

- **Current Version**: 2.4 (build 24)
- **Development Phase**: Sprint 5 completed (Deep Link Service & Integration)
- **Architecture Improvements**: Event-driven deep linking, thread-safe state management, multi-language support
- **Master Plan**: See MasterPlan.md for detailed roadmap (targeting v2.5)