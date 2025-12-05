# AmuleRemoteControl Master Plan

**Version:** 2.0 → 2.5
**Duration:** 17 weeks (17 sprints × 1 week)
**Last Updated:** 2025-12-04
**Status:** 🚧 In Progress (Sprint 3 Ready)

---

## 📑 Table of Contents

1. [Executive Summary](#executive-summary)
2. [Project Context](#project-context)
3. [Architecture Overview](#architecture-overview)
4. [Development Phases](#development-phases)
5. [Sprint Plan](#sprint-plan)
6. [Release Strategy](#release-strategy)
7. [Testing Strategy](#testing-strategy)
8. [Risk Management](#risk-management)
9. [Success Metrics](#success-metrics)
10. [Appendices](#appendices)

---

## 📊 Current Progress

### Sprint Status

| Sprint | Phase | Status | Completion Date | Notes |
|--------|-------|--------|-----------------|-------|
| **Sprint 1** | Phase 1: Critical Fixes | ✅ **COMPLETE** | 2025-12-04 | Multi-language support implemented. All hardcoded cultures removed. 5 cultures supported. |
| **Sprint 2** | Phase 1: Critical Fixes | ✅ **COMPLETE** | 2025-12-04 | Thread-safe state management. Standardized EventHandler pattern. Memory leak audit complete. |
| Sprint 3 | Phase 1: Critical Fixes | ⬜ Pending | - | Atomic writes & validation |

### Completed Tasks

**Sprint 1 Deliverables (✅ Complete - 2025-12-04):**
- ✅ Task 1.1: ICultureProvider implementation - removed all hardcoded "it-IT" references
- ✅ Task 1.2: Culture selection UI with 5 supported languages (en-US, it-IT, de-DE, fr-FR, es-ES)
- ✅ Task 1.3: Comprehensive testing checklist created (Sprint1_TestChecklist.md)
- ✅ Build successful with 0 errors
- ✅ Files created: ICultureProvider.cs, CultureProvider.cs
- ✅ Total: 18 hours invested

**Sprint 2 Deliverables (✅ Complete - 2025-12-04):**
- ✅ Task 1.4: Thread-safe state management with lock objects in aMuleRemoteService and AccessService
- ✅ Task 1.5: Standardized event patterns - all events now use EventHandler? instead of Action?
- ✅ Task 1.6: Memory leak audit - added IDisposable to ServerHome.razor, verified all components properly dispose
- ✅ Build successful with 0 errors
- ✅ Files created: Sprint2_MemoryLeakAudit.md, Sprint2_TestChecklist.md
- ✅ Files modified: aMuleRemoteService.cs, AccessService.cs, IAccessService.cs, MainLayout.razor, ServerHome.razor, CLAUDE.md
- ✅ Total: 18 hours invested

### Phase 1 Progress: 67% Complete (2/3 sprints)

---

## 📊 Executive Summary

### Mission

Transform AmuleRemoteControl from a functional but fragile application into a robust, maintainable, and feature-rich aMule remote management solution through systematic refactoring and strategic feature additions.

### Current State (v2.0)

- ✅ **Functional:** Core features work (downloads, uploads, search, servers)
- ✅ **Multi-platform:** Android, iOS, macOS, Windows targets
- ⚠️ **Fragile:** HTML parsing breaks on aMule updates
- ⚠️ **Limited:** Hardcoded Italian culture, no dark mode, basic UX
- ⚠️ **Technical Debt:** Global state, non-thread-safe events, commented code

### Target State (v2.5)

- ✅ **Robust:** Modular HTML parsing with version detection
- ✅ **International:** Multi-language support with user selection
- ✅ **Secure:** Validated deep linking, thread-safe state management
- ✅ **Modern:** Dark mode, notifications, statistics dashboard
- ✅ **Maintainable:** Clean architecture, comprehensive testing, documentation

### Key Metrics

| Metric | Current | Target | Improvement |
|--------|---------|--------|-------------|
| **Lines of Code** | ~3,500 | ~4,200 | +20% (features) |
| **Test Coverage** | 0% | 80%+ | +80% |
| **Code Smells** | 17 major | 0 | -100% |
| **Supported Cultures** | 1 (it-IT) | 5+ | +400% |
| **Critical Bugs** | 6 | 0 | -100% |
| **Architecture Score** | C+ | A | Quality++ |

### Investment

- **Total Duration:** 17 weeks (4.25 months)
- **Total Effort:** 370 hours (solo developer)
- **Weekly Commitment:** ~22 hours/week
- **Releases:** 5 versions (v2.1 through v2.5)

---

## 🎯 Project Context

### Background

AmuleRemoteGui is a .NET 10.0 MAUI Blazor Hybrid application that provides remote control for aMule P2P file-sharing servers. The app runs primarily on Android, allowing users to manage downloads, uploads, searches, and server connections through a mobile interface.

**Key Technology Stack:**
- .NET 10.0 MAUI Blazor Hybrid
- Radzen.Blazor 8.3.6 (UI components)
- HtmlAgilityPack 1.12.4 (HTML parsing)
- Serilog 4.3.0 (logging)
- Plugin.Fingerprint 2.1.5 (biometric auth)

### Problem Statement

The application currently suffers from:

1. **Critical Issues:**
   - Hardcoded Italian culture breaks non-Italian users
   - Fragile HTML parsing with magic numbers
   - Non-thread-safe event patterns
   - Password sent as query parameter (HTTP context)
   - File I/O without atomic writes
   - Global mutable singleton for deep linking

2. **Major Issues:**
   - Inconsistent event patterns across services
   - Potential memory leaks from event subscriptions
   - No null checking in HTML parsing
   - Incomplete encryption implementation
   - Circular configuration dependencies

3. **Minor Issues:**
   - 40+ lines of commented dead code
   - Magic numbers with no documentation
   - No input validation for user data
   - Inconsistent error handling

### Strategic Goals

1. **Stability First:** Fix critical architectural issues before adding features
2. **Maintainability:** Create modular, testable code with clear patterns
3. **User Experience:** Add modern features users expect (dark mode, notifications)
4. **Future-Proof:** Design for aMule version changes and future enhancements
5. **Android Focus:** Optimize for Android with iOS as future consideration

### Constraints

- **Solo Developer:** All work done by single developer
- **Weekly Sprints:** 1-week sprint cadence with weekly commits
- **Platform Priority:** Android only (iOS maybe future)
- **aMule Version:** Support current version only
- **Backward Compatibility:** Maintain settings compatibility where possible
- **UI Framework:** Keep Radzen.Blazor (no migration to MudBlazor)

### Success Criteria

✅ **Technical:**
- Zero critical bugs
- 80%+ test coverage for services
- All hardcoded values moved to configuration
- Thread-safe state management
- Modular HTML parsing architecture

✅ **User Experience:**
- Multi-language support (5+ cultures)
- Dark mode with theme persistence
- Download completion notifications
- Statistics dashboard with charts
- Batch operations for downloads

✅ **Process:**
- Weekly commits with clear messages
- Phase-based releases (v2.1, v2.2, v2.3, v2.4, v2.5)
- Updated documentation (CLAUDE.md, XML comments)
- Zero commented dead code

---

## 🏗️ Architecture Overview

### Current Architecture

```
┌─────────────────────────────────────────────┐
│         MAUI Container (MainPage.xaml)      │
│              BlazorWebView Host             │
└─────────────────────┬───────────────────────┘
                      │
┌─────────────────────▼───────────────────────┐
│     Blazor UI Layer (Razor Components)      │
│  Home, Login, Downloading, Upload, Search   │
│         Settings, Servers, Logs             │
└─────────────────────┬───────────────────────┘
                      │
┌─────────────────────▼───────────────────────┐
│         Service Layer (DI Scoped)           │
│  IAmuleRemoteServices │ IAccessService      │
│  IUtilityServices     │ INetworkHelper      │
│  SessionStorageAccessor │ Ed2kUrl (Singleton)│
└─────────────────────┬───────────────────────┘
                      │
┌─────────────────────▼───────────────────────┐
│    Data/Infrastructure Layer                │
│  HTML Parsing (HtmlAgilityPack)            │
│  File Storage (JSON in AppDataDirectory)   │
│  HTTP Client (NetworkHelper)               │
│  Platform-Specific (Android Deep Linking)  │
└─────────────────────────────────────────────┘
```

### Target Architecture (v2.5)

```
┌─────────────────────────────────────────────┐
│         MAUI Container (MainPage.xaml)      │
│              BlazorWebView Host             │
└─────────────────────┬───────────────────────┘
                      │
┌─────────────────────▼───────────────────────┐
│     Blazor UI Layer (Razor Components)      │
│  + Dark Mode Support                        │
│  + Batch Operations                         │
│  + Statistics Dashboard                     │
│  + Download Notifications                   │
└─────────────────────┬───────────────────────┘
                      │
┌─────────────────────▼───────────────────────┐
│         Service Layer (DI Scoped)           │
│  IAmuleRemoteServices (uses parsers)       │
│  IAccessService (thread-safe events)       │
│  IUtilityServices (culture-aware)          │
│  INetworkHelper                            │
│  + IDeepLinkService (event bus)            │
│  + ICultureProvider (configurable)         │
│  + IThemeService (dark/light)              │
│  + INotificationService (downloads)        │
│  + IAnalyticsService (statistics)          │
└─────────────────────┬───────────────────────┘
                      │
┌─────────────────────▼───────────────────────┐
│           Parser Layer (NEW)                │
│  IDownloadParser │ IUploadParser            │
│  IServerParser   │ IStatsParser             │
│  ISearchParser   │ IPreferencesParser       │
│  XPathConfiguration (version-aware)        │
└─────────────────────┬───────────────────────┘
                      │
┌─────────────────────▼───────────────────────┐
│    Data/Infrastructure Layer                │
│  HTML Parsing (modular, testable)          │
│  File Storage (atomic writes)              │
│  HTTP Client (Result<T> pattern)           │
│  Platform-Specific (validated deep linking)│
│  SQLite (statistics history)               │
└─────────────────────────────────────────────┘
```

### Key Architectural Changes

1. **Parser Layer Extraction:** HTML parsing logic separated from service layer
2. **Event Bus Pattern:** Replace singleton with IDeepLinkService event bus
3. **Thread-Safe Events:** All state properties use locking mechanisms
4. **Result<T> Pattern:** Error handling with Result<T> instead of exceptions
5. **Culture Abstraction:** ICultureProvider for internationalization
6. **Theme Service:** IThemeService for dark mode management
7. **SQLite Integration:** Local database for statistics history

---

## 📅 Development Phases

### Phase 1: Critical Fixes (Sprints 1-3, v2.1)

**Goal:** Eliminate critical bugs that affect all users

**Duration:** 3 weeks (30 hours)

**Deliverables:**
- ✅ Multi-language support (en-US, it-IT, de-DE, fr-FR, es-ES)
- ✅ Thread-safe state management
- ✅ Atomic file writes for settings
- ✅ Input validation for all user inputs

**Release:** v2.1 - "Stability Release"

---

### Phase 2: Deep Linking Improvements (Sprints 4-6, v2.2)

**Goal:** Secure and robust ed2k:// link handling

**Duration:** 3 weeks (56 hours)

**Deliverables:**
- ✅ Ed2k URL validation and parsing
- ✅ Event-based deep linking (no singleton)
- ✅ User confirmation dialog
- ✅ Analytics tracking
- ✅ Optional magnet link support

**Release:** v2.2 - "Deep Linking Release"

---

### Phase 3: HTML Parsing Refactoring (Sprints 7-10, v2.3)

**Goal:** Modular, testable, version-aware parsing

**Duration:** 4 weeks (86 hours)

**Deliverables:**
- ✅ Parser layer with clean interfaces
- ✅ XPath configuration externalized
- ✅ HTML test fixtures for all pages
- ✅ aMule version detection
- ✅ 80%+ test coverage for parsers

**Release:** v2.3 - "Architecture Release"

---

### Phase 4: Error Handling & Stability (Sprints 11-12, v2.4)

**Goal:** Production-ready error handling and polish

**Duration:** 2 weeks (46 hours)

**Deliverables:**
- ✅ Result<T> pattern throughout
- ✅ Custom exception types
- ✅ Connection diagnostics page
- ✅ Comprehensive logging
- ✅ Code cleanup (remove dead code)

**Release:** v2.4 - "Production Release"

---

### Phase 5: Feature Enhancements (Sprints 13-17, v2.5)

**Goal:** Modern UX features users expect

**Duration:** 5 weeks (152 hours)

**Deliverables:**
- ✅ Dark mode with theme switching
- ✅ Download completion notifications
- ✅ Statistics dashboard with charts
- ✅ Batch operations (multi-select)

**Release:** v2.5 - "Feature Release"

---

## 🗓️ Sprint Plan

### Sprint Summary Table

| Sprint | Phase | Tasks | Hours | Release |
|--------|-------|-------|-------|---------|
| 1 | Phase 1: Critical Fixes | 1.1-1.3 | 18 | - |
| 2 | Phase 1: Critical Fixes | 1.4-1.6 | 18 | - |
| 3 | Phase 1: Critical Fixes | 1.7-1.10 | 20 | **v2.1** |
| 4 | Phase 2: Deep Linking | 2.1-2.3 | 18 | - |
| 5 | Phase 2: Deep Linking | 2.4-2.7 | 22 | - |
| 6 | Phase 2: Deep Linking | 2.8-2.10 | 16 | **v2.2** |
| 7 | Phase 3: Parsing Refactor | 3.1-3.3 | 18 | - |
| 8 | Phase 3: Parsing Refactor | 3.4-3.6 | 20 | - |
| 9 | Phase 3: Parsing Refactor | 3.7-3.9 | 22 | - |
| 10 | Phase 3: Parsing Refactor | 3.10-3.12 | 22 | **v2.3** |
| 11 | Phase 4: Error Handling | 4.1-4.4 | 26 | - |
| 12 | Phase 4: Error Handling | 4.5-4.8 | 20 | **v2.4** |
| 13 | Phase 5: Features | 5.1-5.3 | 20 | - |
| 14 | Phase 5: Features | 5.4-5.6 | 26 | - |
| 15 | Phase 5: Features | 5.7-5.9 | 32 | - |
| 16 | Phase 5: Features | 5.10-5.12 | 36 | - |
| 17 | Phase 5: Features | 5.13-5.15 | 38 | **v2.5** |

**Total:** 17 sprints, 370 hours, 5 releases

---

### Detailed Sprint Breakdown

---

## 🔴 PHASE 1: CRITICAL FIXES

**Goal:** Fix bugs affecting all users
**Duration:** Sprints 1-3 (3 weeks)
**Release:** v2.1 - "Stability Release"

---

### Sprint 1: Culture Internationalization

| ID | Title | Description | Est. Hours | Priority | Risk |
|----|-------|-------------|-----------|----------|------|
| **1.1** | **Fix Culture Hardcoding - Implementation** | Remove all hardcoded "it-IT" culture references from `UtilityServices.cs` (lines 51, 64, 91), `aMuleRemoteService.cs` (lines 83, 295-301), `MainLayout.razor` (line 119). Create `ICultureProvider` interface with `GetCulture()` and `SetCulture(string)` methods. Implement `CultureProvider` class that reads from `GlobalSettings.json`. Register in DI as scoped service. | 8 | 🔴 Critical | Low |
| **1.2** | **Fix Culture Hardcoding - Settings UI** | Add culture selection dropdown to `Setting.razor`. Display culture options: English (en-US), Italian (it-IT), German (de-DE), French (fr-FR), Spanish (es-ES). Store selected culture in `GlobalSettings.json` with key "Culture". Load on app startup and apply to `CultureInfo.CurrentCulture`. Add culture name display to settings page. | 6 | 🔴 Critical | Low |
| **1.3** | **Fix Culture Hardcoding - Testing** | Test number formatting (1000 → "1,000" in en-US, "1.000" in de-DE). Test currency formatting with each culture. Test date formatting. Verify aMule parsing still works (download speeds, file sizes). Test culture switching without app restart. Create test checklist document. | 4 | 🔴 Critical | Medium |

**Sprint 1 Total:** 18 hours
**Commit Message:** `feat: Add multi-language support with culture selection (fixes #1)`

---

### Sprint 2: Thread Safety & Events

| ID | Title | Description | Est. Hours | Priority | Risk |
|----|-------|-------------|-----------|----------|------|
| **1.4** | **Thread-Safe State Management** | Add `private readonly object _statusLock = new object();` to `aMuleRemoteService.cs`. Wrap all property getters/setters in `lock (_statusLock) { ... }`. Apply to: `Status` (line 16-38), `DownSpeed` (line 40-51), `UpSpeed` (line 53-64). Do same for `AccessService.IsAuthorized` (line 9-15). Verify events still fire inside lock. Add inline comments explaining thread safety. | 8 | 🔴 Critical | Medium |
| **1.5** | **Standardize Event Patterns** | Convert `AccessService.OnChange` (line 13, type `Action?`) to `EventHandler? OnChange`. Update all subscribers in `MainLayout.razor` and `LoginPage.razor`. Standardize all event invocations to use `EventName?.Invoke(this, EventArgs.Empty)` pattern. Document standard pattern in CLAUDE.md under "State Management Pattern". | 6 | 🟠 High | Low |
| **1.6** | **Memory Leak Audit** | Audit all Razor components (`*.razor` files) for event subscriptions. Verify each subscribing component implements `IDisposable`. Check unsubscribe in `Dispose()` method. Add disposal to: `DownloadingHome.razor`, `UploadFileHome.razor`, `ServerHome.razor`, `SearchPage.razor`, `LoginPage.razor`. Create checklist of all components with their disposal status. | 4 | 🟠 High | Medium |

**Sprint 2 Total:** 18 hours
**Commit Message:** `fix: Implement thread-safe state management and standardize event patterns (fixes #2)`

---

### Sprint 3: Atomic Writes & Validation

| ID | Title | Description | Est. Hours | Priority | Risk |
|----|-------|-------------|-----------|----------|------|
| **1.7** | **Atomic File Writes - Helper** | Create `WriteJsonAtomically<T>(string path, T data)` method in `UtilityServices.cs`. Implementation: (1) Generate temp file with `Path.GetTempFileName()`, (2) Write JSON to temp file with error handling, (3) `File.Move(temp, target, overwrite: true)` for atomic operation, (4) Delete temp file in finally block. Add XML documentation comments. Log all operations with Serilog. | 6 | 🔴 Critical | Low |
| **1.8** | **Atomic File Writes - Migration** | Replace all `File.WriteAllText()` calls with `WriteJsonAtomically()` in `UtilityServices.cs`: `WriteNetworkSettingJson()` (line 167), `WriteGlobalSettingJson()` (line 291), `WriteCustomSettingJson()` (line 354), `WriteLoginSettingJson()` (line 422). Test all settings save operations. Verify files don't corrupt on app crash (simulate with debugger break). | 4 | 🔴 Critical | Medium |
| **1.9** | **Input Validation - Service Layer** | Add validation to `SearchFiles(string searchText)` in `aMuleRemoteService.cs` (line 577): check `IsNullOrWhiteSpace`, max length 100 chars, `HtmlEncode()` for XSS prevention. Add validation to `PostDownloadCommand(string fileId, string command)` (line 666): validate fileId format (numeric), validate command is in allowed list ["pause", "resume", "delete", "cancel", "priority"]. Return `null` or empty response on validation failure. Log validation errors. | 6 | 🟠 High | Low |
| **1.10** | **Input Validation - Testing** | Create test cases document with 20+ scenarios: empty search text, 101-char search, XSS attempts (`<script>alert('xss')</script>`), SQL injection attempts, special characters (é, ñ, ü), numeric strings, whitespace-only. Test invalid file IDs: negative numbers, non-numeric, very large numbers. Test invalid commands. Verify all return appropriate errors. Document test results. | 4 | 🟠 High | Medium |

**Sprint 3 Total:** 20 hours
**Commit Message:** `fix: Add atomic file writes and comprehensive input validation (fixes #3)`
**Release:** 🚀 **v2.1 - "Stability Release"**

---

## 🔵 PHASE 2: DEEP LINKING IMPROVEMENTS

**Goal:** Secure and robust ed2k:// link handling
**Duration:** Sprints 4-6 (3 weeks)
**Release:** v2.2 - "Deep Linking Release"

---

### Sprint 4: Ed2k URL Parser

| ID | Title | Description | Est. Hours | Priority | Risk |
|----|-------|-------------|-----------|----------|------|
| **2.1** | **Ed2k URL Parser - Interface** | Create `IEd2kUrlParser` interface in `Components/Interfaces/` with methods: `Result<Ed2kLink> Parse(string url)` and `bool IsValid(string url)`. Create `Ed2kLink` record with properties: `string FileName`, `long FileSize`, `string FileHash`, `string[] Sources` (optional). Create `Ed2kParseError` enum: InvalidFormat, InvalidHash, InvalidSize, MissingRequired. Add XML documentation to all types. | 4 | 🟠 High | Low |
| **2.2** | **Ed2k URL Parser - Implementation** | Implement `Ed2kUrlParser` class. Valid format: `ed2k://\|file\|name\|size\|hash\|/` or `ed2k://\|file\|name\|size\|hash\|h=hashset\|/`. Use regex pattern: `^ed2k://\|file\|([^\|]+)\|(\d+)\|([A-F0-9]{32})\|`. Validate hash is exactly 32 hex chars (MD4). Validate size is positive long. Parse optional sources (pipe-separated server IPs). Handle URL encoding. Return `Result<Ed2kLink>` with specific error messages. Add 50+ lines of inline comments. | 8 | 🟠 High | High |
| **2.3** | **Ed2k URL Parser - Testing** | Create `Ed2kUrlParserTests.cs` in test project. Test cases: (1) Valid standard link, (2) Valid with hashset, (3) Valid with sources, (4) URL-encoded filename, (5) Missing pipes, (6) Invalid hash length, (7) Non-hex hash, (8) Negative size, (9) Zero size, (10) Missing file name, (11) Special chars in name (ü, é, ñ), (12) Very large file size (>1TB), (13) Malformed prefix, (14) Injection attempts, (15) Empty string, (16) Null input. Achieve >95% code coverage. Document all test cases. | 6 | 🟠 High | Medium |

**Sprint 4 Total:** 18 hours
**Commit Message:** `feat: Add ed2k URL parser with comprehensive validation (refs #4)`

---

### Sprint 5: Deep Link Service & Integration

| ID | Title | Description | Est. Hours | Priority | Risk |
|----|-------|-------------|-----------|----------|------|
| **2.4** | **Deep Link Service - Event Bus** | Create `IDeepLinkService` interface with `event EventHandler<DeepLinkEventArgs> LinkReceived` and `void NotifyLinkReceived(string url, DeepLinkSource source)`. Create `DeepLinkEventArgs` record: `string Url`, `DeepLinkSource Source`, `DateTime ReceivedAt`, `Ed2kLink? ParsedLink`. Create `DeepLinkSource` enum: ExternalApp, Browser, InAppShare. Implement `DeepLinkService` class. Register in `MauiProgram.cs` as scoped service. Add XML docs. | 6 | 🟠 High | Medium |
| **2.5** | **Deep Link Service - MainActivity Integration** | Update `MainActivity.cs`: Inject `IDeepLinkService` and `IEd2kUrlParser` via DI. In `HandleAppLink(string url)`: (1) Log with Serilog "Deep link received", (2) Decode URL, (3) Call `_parser.Parse(decoded)`, (4) If validation fails, log warning and show toast "Invalid ed2k link", return early, (5) Call `_deepLinkService.NotifyLinkReceived(decoded, DeepLinkSource.ExternalApp)`, (6) `PushModalAsync(new MainPage())`, (7) Wrap all in try-catch with error logging. Remove all references to `Ed2kUrl` singleton (lines 44-48, 76-80). | 6 | 🟠 High | Medium |
| **2.6** | **Deep Link Service - Home.razor Refactor** | Update `Home.razor`: Remove `Ed2kUrlService` injection. Inject `IDeepLinkService`. In `OnInitialized()`, subscribe to `_deepLinkService.LinkReceived` event. Event handler: check `e.ParsedLink != null`, store in component state, navigate to `/addlink` with state parameter. Implement `IDisposable` to unsubscribe. Remove lines 33-39 (old Ed2kUrl check). Update `AddLink.razor` to receive deep link data from navigation state instead of global singleton. | 4 | 🟠 High | Low |
| **2.7** | **Deep Link Service - Testing** | Test deep link flow end-to-end on Android: (1) Share ed2k link from browser while app closed → app launches and shows AddLink, (2) Share link while app running → navigates to AddLink, (3) Click link in browser → app opens, (4) Rapidly share 3 links in succession → all handled, (5) Share invalid link → toast shown, app doesn't crash, (6) Share malformed link → toast shown. Test with real aMule links and malicious attempts. Document test matrix with pass/fail. | 6 | 🔴 Critical | High |

**Sprint 5 Total:** 22 hours
**Commit Message:** `refactor: Replace Ed2kUrl singleton with event-based IDeepLinkService (fixes #5)`

---

### Sprint 6: Deep Link Enhancements

| ID | Title | Description | Est. Hours | Priority | Risk |
|----|-------|-------------|-----------|----------|------|
| **2.8** | **User Confirmation Dialog** | Update `AddLink.razor`: When deep link received, show RadzenDialog with: Title "Add Download?", Content showing file name, file size (formatted), source ("Shared from external app"), Buttons: [Cancel] [Add Download]. On Cancel, clear state and stay on page. On Add, call `PostLink()` to send to aMule. Add checkbox "Don't ask again" that stores preference in `CustomSettings.json`. If preference set, skip dialog and add directly. Style dialog with Radzen theming. | 4 | 🟡 Medium | Low |
| **2.9** | **Deep Link Analytics** | Add analytics tracking to `DeepLinkService`: Create `DeepLinkAnalytics` class that stores events in `CustomSettings.json` under "DeepLinkStats" key. Track: `TotalReceived` (int), `TotalAccepted` (int), `TotalRejected` (int), `LastReceivedAt` (DateTime), `SourceBreakdown` (Dictionary<DeepLinkSource, int>). Increment counters on each event. Create `DiagnosticsPage.razor` that displays these stats in RadzenCards. Add link to diagnostics from Settings page. | 4 | 🟢 Low | Low |
| **2.10** | **Support Magnet Links (Optional)** | Extend `IntentFilter` in `MainActivity.cs` to include `DataScheme = "magnet"` (line 16). Create `IMagnetLinkParser` interface. Implement basic magnet parser: `magnet:?xt=urn:btih:HASH&dn=NAME&tr=TRACKER`. Parse into `MagnetLink` record. In `DeepLinkService`, detect scheme and route to appropriate parser. For magnet links, show message "Magnet links not yet supported by aMule" with option to copy to clipboard. Log magnet link attempts for future feature planning. | 8 | 🟢 Low | Medium |

**Sprint 6 Total:** 16 hours
**Commit Message:** `feat: Add user confirmation dialog and analytics for deep links (refs #6)`
**Release:** 🚀 **v2.2 - "Deep Linking Release"**

---

## 🟢 PHASE 3: HTML PARSING REFACTORING

**Goal:** Modular, testable, version-aware parsing
**Duration:** Sprints 7-10 (4 weeks)
**Release:** v2.3 - "Architecture Release"

---

### Sprint 7: Parser Foundation

| ID | Title | Description | Est. Hours | Priority | Risk |
|----|-------|-------------|-----------|----------|------|
| **3.1** | **XPath Configuration - Extract** | Create `XPathConfiguration.cs` class with properties for all XPath selectors: `DownloadTableXPath`, `UploadTableXPath`, `ServerTableXPath`, `StatsXPath`, `SearchResultsXPath`, `PreferencesTableXPath`. Extract hardcoded Skip() indices to named constants: `DOWNLOAD_TABLE_INDEX = 6`, `UPLOAD_TABLE_INDEX = 8`, etc. Create `xpaths.json` configuration file in Resources/Raw. Load JSON in XPathConfiguration constructor. Create version-specific sections in JSON: "2.3.2", "2.3.3", "default". Add XML comments explaining each XPath. | 8 | 🔴 Critical | Medium |
| **3.2** | **Parser Interfaces - Design** | Create parser interfaces in `Components/Interfaces/`: `IDownloadParser` with `List<DownloadFile> Parse(string html)`, `IUploadParser` with `List<UploadFile> Parse(string html)`, `IServerParser` with `List<Server> Parse(string html)`, `IStatsParser` with `Stats Parse(string html)`, `ISearchParser` with `SearchResult Parse(string html)`, `IPreferencesParser` with `Dictionary<string, string> Parse(string html)`. All return types wrapped in `Result<T>`. Add `IAmuleVersionDetector` with `string DetectVersion(string html)`. Register all in DI as scoped. | 4 | 🔴 Critical | Low |
| **3.3** | **HTML Test Fixtures** | Create `TestFixtures/AmuleHtml/` folder. Capture real HTML from running aMule 2.3.2 instance: `downloads.html`, `uploads.html`, `servers.html`, `stats.html`, `search.html`, `preferences.html`, `version-footer.html`. Create variations: `downloads-empty.html`, `downloads-single.html`, `downloads-100items.html`, `downloads-malformed.html` (missing columns), `downloads-paused.html`, `downloads-completed.html`. Save as embedded resources. Document HTML structure in `HTML_STRUCTURE.md`. | 6 | 🟠 High | Low |

**Sprint 7 Total:** 18 hours
**Commit Message:** `refactor: Extract XPath configuration and create parser interfaces (refs #7)`

---

### Sprint 8: Download & Upload Parsers

| ID | Title | Description | Est. Hours | Priority | Risk |
|----|-------|-------------|-----------|----------|------|
| **3.4** | **Download Parser - Implementation** | Create `DownloadParser.cs` implementing `IDownloadParser`. Move `ParseDownloading()` logic from `aMuleRemoteService.cs` (lines 195-282) to this class. Inject `XPathConfiguration`. Replace `Skip(6).Take(1)` with `xpathConfig.DownloadTableXPath`. Add null-coalescing operators for all `FirstOrDefault()` calls. Extract magic numbers to named constants: `FILE_ID_INDEX = 0`, `FILE_NAME_INDEX = 1`, etc. Add inline comments for each parsing step. Handle edge cases: empty list, single item, 100+ items. Return `Result<List<DownloadFile>>` with descriptive errors. | 8 | 🔴 Critical | High |
| **3.5** | **Download Parser - Testing** | Create `DownloadParserTests.cs`. Test with fixtures: (1) Empty downloads → returns empty list, (2) Single download → correctly parsed, (3) 100 downloads → all parsed, (4) Malformed HTML → returns error with message, (5) Missing columns → uses defaults, (6) Paused downloads → status = "Paused", (7) Completed downloads → progress = 100%, (8) Special chars in filename → correctly decoded, (9) Very large file size → parsed as long, (10) Null input → returns error. Use Xunit and FluentAssertions. Achieve 95%+ coverage. | 6 | 🔴 Critical | Medium |
| **3.6** | **Upload Parser - Implementation** | Create `UploadParser.cs` implementing `IUploadParser`. Move `ParseUpload()` logic from `aMuleRemoteService.cs` (lines 351-421) to this class. Apply same refactoring as download parser: inject XPathConfiguration, replace `Skip(8).Take(1)`, add null safety, extract constants. Handle columns: FileName, UploadSpeed, Uploaded (completed), Requests, Complete (times). Return `Result<List<UploadFile>>`. Add XML documentation. | 6 | 🟠 High | High |

**Sprint 8 Total:** 20 hours
**Commit Message:** `refactor: Implement modular download and upload parsers with tests (refs #8)`

---

### Sprint 9: Server, Stats & Search Parsers

| ID | Title | Description | Est. Hours | Priority | Risk |
|----|-------|-------------|-----------|----------|------|
| **3.7** | **Server & Stats Parsers** | Create `ServerParser.cs`: Move `ParseServers()` logic (lines 448-515). Parse columns: ServerName, IP, Port, Description, Users, Files, Priority, Ping, Fail, Static. Add connection status parsing. Create `StatsParser.cs`: Move `ParseStats()` logic (lines 526-554). Parse: ED2K status, KAD status, download speed, upload speed, users, files, shared count. Both return `Result<T>`. Add comprehensive null checks. Test with 10+ fixtures each. | 8 | 🟠 High | High |
| **3.8** | **Search Parser** | Create `SearchParser.cs`: Move `ParseSearch()` logic (lines 598-669). Parse columns: FileName, FileSize, FileHash, Sources, Type, FileId. Handle pagination if aMule supports it (check HTML for "next page" links). Add file type detection (video, audio, document, archive, other) based on extension. Return `Result<SearchResult>` with `List<SearchFile>` and `int TotalResults`. Test with various search result sizes. | 6 | 🟡 Medium | Medium |
| **3.9** | **Preferences Parser Refactor** | Create `PreferencesParser.cs`: Refactor `ParsePreferences()` (lines 774-1002). Current 200+ case statement is unmaintainable. Create `PreferenceMapping.json` with structure: `{ "row_name": { "key": "MaxUpload", "type": "int", "unit": "kB/s" } }`. Load JSON and use dictionary lookup instead of switch. Parse value and unit separately. Return `Result<Dictionary<string, PreferenceValue>>` where `PreferenceValue` has `RawValue`, `ParsedValue`, `Unit`. Reduce method from 200 lines to ~50 lines. | 8 | 🟡 Medium | High |

**Sprint 9 Total:** 22 hours
**Commit Message:** `refactor: Implement server, stats, search, and preferences parsers (refs #9)`

---

### Sprint 10: Parser Integration & Version Detection

| ID | Title | Description | Est. Hours | Priority | Risk |
|----|-------|-------------|-----------|----------|------|
| **3.10** | **Parser Integration** | Update `aMuleRemoteService.cs`: Inject all parser interfaces (IDownloadParser, IUploadParser, etc.). Replace `ParseDownloading()` calls with `_downloadParser.Parse(html)`. Replace `ParseUpload()`, `ParseServers()`, `ParseStats()`, `ParseSearch()`, `ParsePreferences()` with respective parsers. Handle `Result<T>` responses: if `!result.Success`, log error and return empty/default. Maintain backward compatibility: existing callers should still work. Remove now-empty parsing methods (preserve for 1 sprint as obsolete). Test all pages still load correctly. | 6 | 🔴 Critical | High |
| **3.11** | **aMule Version Detection** | Create `AmuleVersionDetector.cs` implementing `IAmuleVersionDetector`. Parse footer HTML (typically `footer.php` response) for version string: "aMule 2.3.2". Use regex: `aMule\s+([\d\.]+)`. Store detected version in memory. In `XPathConfiguration`, select XPath set based on version: if "2.3.2" use default, if "2.3.3" use alternate set (if needed), if unknown version use default and log warning. Create `VersionCompatibilityPage.razor` showing detected version and compatibility status. Add link from Settings. Show warning banner in MainLayout if unsupported version detected. | 8 | 🟠 High | Medium |
| **3.12** | **Parser Testing - Integration** | End-to-end testing with real aMule instance: (1) Connect to aMule 2.3.2, (2) Navigate to all pages (downloads, uploads, servers, search, preferences), (3) Verify all data displays correctly, (4) Add download via ed2k link, (5) Pause/resume download, (6) Search for file, (7) Connect to server, (8) View statistics, (9) Open preferences. Document any parsing failures. Test with mock HTML (no aMule required): run all parsers against all fixtures, verify 100% pass rate. Create test report document. Measure test coverage: target 80%+ for all parsers. | 8 | 🔴 Critical | High |

**Sprint 10 Total:** 22 hours
**Commit Message:** `feat: Integrate modular parsers with version detection and comprehensive testing (fixes #10)`
**Release:** 🚀 **v2.3 - "Architecture Release"**

---

## 🟣 PHASE 4: ERROR HANDLING & STABILITY

**Goal:** Production-ready error handling and polish
**Duration:** Sprints 11-12 (2 weeks)
**Release:** v2.4 - "Production Release"

---

### Sprint 11: Result Pattern & Error Types

| ID | Title | Description | Est. Hours | Priority | Risk |
|----|-------|-------------|-----------|----------|------|
| **4.1** | **Result<T> Pattern - Implementation** | Create `Result.cs` in `Components/Data/`: `public record Result<T>` with properties `bool Success`, `T? Data`, `string? Error`, `Exception? Exception`. Add factory methods: `public static Result<T> Ok(T data)`, `public static Result<T> Fail(string error)`, `public static Result<T> Fail(Exception ex)`. Add implicit conversions from T to Result<T>. Add `Match()` method for functional handling: `result.Match(onSuccess: data => ..., onFailure: error => ...)`. Add XML documentation with usage examples. | 4 | 🟡 Medium | Low |
| **4.2** | **Custom Exceptions** | Create exception types in `Components/Exceptions/`: `AmuleParsingException` (for HTML parsing failures, includes HTML snippet), `AmuleConnectionException` (for network failures, includes URL and status code), `AmuleAuthenticationException` (for login failures). All inherit from `AmuleException : Exception` base class. Add properties: `DateTime OccurredAt`, `string Context`. Add XML docs. Register in global exception handler if using one. | 4 | 🟡 Medium | Low |
| **4.3** | **Service Layer - Result<T> Migration** | Update all service methods to return `Result<T>`: `GetDownloading()` → `Result<List<DownloadFile>>`, `GetUploading()` → `Result<List<UploadFile>>`, `GetServers()` → `Result<List<Server>>`, `SearchFiles()` → `Result<SearchResult>`, `LoggedIn()` → `Result<bool>`, `GetStats()` → `Result<Stats>`. Wrap all network calls in try-catch. Return `Result<T>.Fail(ex)` on exception. Update internal error handling to use Result pattern instead of returning null. This is breaking change: update all 15+ callers. | 12 | 🟡 Medium | High |
| **4.4** | **UI Error Handling** | Update all Razor components to handle `Result<T>` responses: Check `result.Success` before accessing `result.Data`. On failure, show RadzenNotification with error message: `_notificationService.Notify(NotificationSeverity.Error, "Error", result.Error)`. Add error state to UI: show empty state with "Failed to load" message and "Retry" button. Update: `DownloadingHome.razor`, `UploadFileHome.razor`, `ServerHome.razor`, `SearchPage.razor`, `LoginPage.razor`, `PreferencePage.razor`. Create consistent error display component: `ErrorDisplay.razor`. | 6 | 🟡 Medium | Low |

**Sprint 11 Total:** 26 hours
**Commit Message:** `refactor: Implement Result<T> pattern and custom exceptions throughout (refs #11)`

---

### Sprint 12: Polish & Production Readiness

| ID | Title | Description | Est. Hours | Priority | Risk |
|----|-------|-------------|-----------|----------|------|
| **4.5** | **Logging Improvements** | Audit all `_logger.LogError(ex.Message)` calls and replace with `_logger.LogError(ex, "Context message")` to include full stack trace. Add structured logging properties: `_logger.LogInformation("Download started {FileName} {FileSize}", fileName, fileSize)`. Add performance logging for slow operations (>2 seconds): wrap parser calls with `Stopwatch`, log if >2s. Create log level configuration in `GlobalSettings.json`: "LogLevel": "Information". Add log viewer page that reads from Serilog file, displays last 100 lines with filtering by level. | 4 | 🟡 Medium | Low |
| **4.6** | **Connection Diagnostics Page** | Create `DiagnosticsPage.razor`: Section 1: Ping test - button "Ping aMule Server" that measures response time, shows success/failure with milliseconds. Section 2: Version info - displays detected aMule version, app version, .NET version, platform (Android/iOS/Windows). Section 3: Connection log - shows last 20 network requests with timestamp, endpoint, status code, duration. Section 4: Storage info - displays AppDataDirectory path, file list with sizes, total storage used. Section 5: Deep link stats - shows analytics from task 2.9. Add navigation link from Settings menu. | 8 | 🟢 Low | Low |
| **4.7** | **Code Cleanup - Remove Dead Code** | Delete all commented-out code: `AccessService.cs` lines 29-70 (40 lines), `MainActivity.cs` lines 26-33, 71-74, `aMuleRemoteService.cs` lines 40-43, `DownloadingHome.razor` lines 20-21. Search entire solution for `//TODO` comments: either implement or remove. Search for `#region` / `#endregion`: remove if not adding value. Run code formatter on all files. Remove unused `using` statements. Remove `Ed2kUrl.cs` file (no longer used after task 2.5). Update `.gitignore` if needed. | 4 | 🟢 Low | Low |
| **4.8** | **Documentation - XML Comments** | Add XML documentation comments (`///`) to all public methods in service interfaces: `IAmuleRemoteServices`, `IAccessService`, `IUtilityServices`, `INetworkHelper`, `IDeepLinkService`, all parser interfaces. Include `<summary>`, `<param>`, `<returns>`, `<exception>` tags. Add code examples in `<example>` tags for complex methods. Update `CLAUDE.md` with architecture changes from Phases 1-4: new parser layer diagram, event bus pattern, Result<T> pattern, thread safety notes. Update "Common Pitfalls" section with lessons learned. Add "Testing Strategy" section. Total 100+ XML comments to add. | 4 | 🟢 Low | Low |

**Sprint 12 Total:** 20 hours
**Commit Message:** `docs: Add comprehensive XML comments and improve logging (refs #12)`
**Release:** 🚀 **v2.4 - "Production Release"**

---

## 🟡 PHASE 5: FEATURE ENHANCEMENTS

**Goal:** Modern UX features users expect
**Duration:** Sprints 13-17 (5 weeks)
**Release:** v2.5 - "Feature Release"

---

### Sprint 13: Dark Mode Foundation

| ID | Title | Description | Est. Hours | Priority | Risk |
|----|-------|-------------|-----------|----------|------|
| **5.1** | **Theme Service - Interface** | Create `IThemeService` interface in `Components/Interfaces/`: Methods: `Theme GetCurrentTheme()`, `void SetTheme(Theme theme)`, `event EventHandler<Theme> ThemeChanged`. Create `Theme` enum: `Light`, `Dark`, `System` (follows OS). Create `ThemeColors` record with properties: `BackgroundColor`, `TextColor`, `PrimaryColor`, `SecondaryColor`, `SurfaceColor`, `ErrorColor`. Store theme preference in `CustomSettings.json` under key "Theme". Register as singleton in DI (shared across app). | 4 | 🟡 Medium | Low |
| **5.2** | **Theme Service - Implementation** | Implement `ThemeService.cs`: Load theme from settings on startup. Apply Radzen theme by calling `ThemeService.ChangeTheme()` method (Radzen has built-in Material, Material Dark themes). Map Theme enum to Radzen theme names: Light → "material", Dark → "material-dark", System → detect from `Application.Current.RequestedTheme`. Subscribe to OS theme changes if System selected. Fire `ThemeChanged` event when theme switches. Persist theme immediately to settings. Add null safety for Radzen ThemeService injection. | 6 | 🟡 Medium | Medium |
| **5.3** | **Theme Selector UI** | Add theme selector to `Setting.razor`: RadzenButtonGroup with three buttons: ☀️ Light, 🌙 Dark, 🔄 System. Highlight active theme. On click, call `_themeService.SetTheme(selectedTheme)`. Show preview of theme colors below buttons (small colored squares). Add animation on theme change (fade transition). Store in new settings tab "Appearance". Add "Preview" button that temporarily applies theme without saving. Test theme persists across app restarts. | 4 | 🟡 Medium | Low |

**Sprint 13 Total:** 14 hours
**Commit Message:** `feat: Add dark mode support with theme service and UI selector (refs #13)`

---

### Sprint 14: Download Notifications

| ID | Title | Description | Est. Hours | Priority | Risk |
|----|-------|-------------|-----------|----------|------|
| **5.4** | **Notification Service - Interface** | Create `INotificationService` interface (different from Radzen NotificationService): Methods: `void ShowDownloadComplete(string fileName, long fileSize)`, `void ShowDownloadFailed(string fileName, string reason)`, `void ShowConnectionLost()`, `void RequestPermissions()`. Create Android platform implementation using `Android.App.Notification` and `NotificationManager`. Handle notification channels for Android 8+. Register as singleton. Add notification icons to Resources/Images. | 6 | 🟡 Medium | Medium |
| **5.5** | **Background Monitoring Service** | Create `DownloadMonitoringService`: Background task that polls `GetDownloading()` every 30 seconds. Maintain dictionary of in-progress downloads with last-known state. Detect state changes: when `Status` changes from "Downloading" to "Completed", call `INotificationService.ShowDownloadComplete()`. Implement as hosted service with CancellationToken support. Add enable/disable toggle in Settings ("Enable download notifications"). Store enabled state in `CustomSettings.json`. Handle app backgrounding on Android (may need WorkManager for long-running task). | 10 | 🟠 High | High |
| **5.6** | **Notification Preferences** | Add notification settings to `Setting.razor` in Appearance tab: Toggle "Enable download notifications" (default: true), Toggle "Notify for all downloads" vs "Notify only for large files (>100MB)" (default: all), Toggle "Show notification sound" (default: true), Button "Test Notification" that shows sample notification. Request notification permissions on first enable (Android 13+). Show permission status. Add deep link from notification: tapping notification opens app to DownloadingHome page with specific download highlighted. Test notifications appear correctly when app in foreground, background, and killed. | 10 | 🟡 Medium | Medium |

**Sprint 14 Total:** 26 hours
**Commit Message:** `feat: Add download completion notifications with background monitoring (refs #14)`

---

### Sprint 15: Statistics Dashboard - Foundation

| ID | Title | Description | Est. Hours | Priority | Risk |
|----|-------|-------------|-----------|----------|------|
| **5.7** | **SQLite Database Setup** | Add `sqlite-net-pcl` NuGet package. Create `StatisticsDatabase.cs` with table models: `DownloadHistory` (Id, FileName, FileSize, StartedAt, CompletedAt, AvgSpeed), `DailyStats` (Date, TotalDownloaded, TotalUploaded, AvgDownSpeed, AvgUpSpeed, PeakDownSpeed, ConnectionUptime), `SpeedSample` (Timestamp, DownloadSpeed, UploadSpeed). Create database in `FileSystem.AppDataDirectory/statistics.db`. Implement CRUD methods: `AddDownload()`, `CompleteDownload()`, `AddSpeedSample()`, `GetDailyStats()`, `GetDownloadHistory()`. Add indexes on timestamp columns. Register as singleton. | 8 | 🟡 Medium | Medium |
| **5.8** | **Statistics Collection Service** | Create `StatisticsCollectionService`: Subscribe to `aMuleRemoteService.StatusChanged` event. On each status update (every 30s), call `_db.AddSpeedSample(timestamp, downSpeed, upSpeed)`. Subscribe to `DownloadMonitoringService` completion events, call `_db.CompleteDownload(fileName, stats)`. Calculate daily aggregates at midnight: query all speed samples for day, compute avg/peak speeds, store in `DailyStats` table. Run as background service. Add error handling for database operations (disk full, corruption). Implement database cleanup: delete speed samples older than 30 days, keep daily stats for 365 days. | 8 | 🟡 Medium | Medium |
| **5.9** | **Statistics Dashboard - Page Structure** | Create `StatisticsPage.razor`: Tab 1: "Overview" - KPI cards showing: Total downloads this month, Total data downloaded this month, Average download speed, Peak download speed, Total connection time. Tab 2: "Charts" - placeholder for Sprint 16. Tab 3: "History" - RadzenDataGrid with download history table (columns: FileName, Size, Duration, AvgSpeed, CompletedAt), sortable, filterable, paginated (20 per page). Tab 4: "Today" - detailed view of today's activity. Add navigation link to Statistics from main menu. Use Radzen Cards and DataGrid components. Style consistently with rest of app. | 8 | 🟡 Medium | Low |

**Sprint 15 Total:** 24 hours
**Commit Message:** `feat: Add SQLite database and statistics collection service (refs #15)`

---

### Sprint 16: Statistics Dashboard - Charts

| ID | Title | Description | Est. Hours | Priority | Risk |
|----|-------|-------------|-----------|----------|------|
| **5.10** | **Speed Chart - Real-Time** | In StatisticsPage "Charts" tab, add real-time line chart using `RadzenChart`: X-axis: Time (last 60 minutes), Y-axis: Speed (KB/s). Two lines: Download speed (blue), Upload speed (green). Update chart every 30 seconds with new speed sample from `aMuleRemoteService.Status`. Keep sliding window of last 60 data points. Add chart legend, grid lines, tooltips on hover. Add "Pause Updates" button to freeze chart. Export chart as image button (use Radzen export feature). Handle empty data gracefully (show "No data available" message). | 8 | 🟡 Medium | Medium |
| **5.11** | **Historical Charts** | Add three historical charts: (1) "Last 7 Days Download Speed" - bar chart showing average download speed per day, (2) "Last 30 Days Data Volume" - stacked bar chart showing downloaded vs uploaded bytes per day, (3) "Download Distribution" - pie chart showing file types (video, audio, documents, other) by count or size. Query data from `StatisticsDatabase.GetDailyStats()`. Use Radzen Chart components. Add date range picker to filter data. Add "This Week" / "This Month" / "This Year" quick filters. Style consistently. Test with mock data (1000+ records). | 12 | 🟡 Medium | Medium |
| **5.12** | **Statistics Export** | Add "Export Statistics" button to StatisticsPage: Export formats: CSV, JSON. CSV includes: all download history with columns (FileName, FileSize, StartedAt, CompletedAt, AvgSpeed). JSON includes: download history + daily stats + speed samples. Use `File.WriteAllTextAsync()` to save to Downloads folder (Android) or Documents (iOS). Show toast "Exported to {path}" with share button. Implement share functionality using MAUI Share API. Add date range filter for export (default: last 30 days). Compress large exports (>1MB) to ZIP. Test export with 10,000+ records. | 8 | 🟢 Low | Low |

**Sprint 16 Total:** 28 hours
**Commit Message:** `feat: Add real-time and historical charts to statistics dashboard (refs #16)`

---

### Sprint 17: Batch Operations

| ID | Title | Description | Est. Hours | Priority | Risk |
|----|-------|-------------|-----------|----------|------|
| **5.13** | **Multi-Select Download Grid** | Update `DownloadingHome.razor`: Enable RadzenDataGrid selection mode with `<RadzenDataGridColumn Selectable="true" />`. Add checkbox column. Bind selected items to component state: `List<DownloadFile> selectedDownloads`. Add "Select All" / "Deselect All" buttons in toolbar. Show selection count badge: "3 selected". Disable selection when no downloads available. Persist selection across data refreshes (by FileId). Add visual feedback: highlight selected rows with different background color. | 8 | 🟡 Medium | Low |
| **5.14** | **Batch Action Toolbar** | Add batch actions toolbar that appears when 1+ downloads selected: Buttons: [▶️ Resume All] [⏸️ Pause All] [🗑️ Delete All] [⬆️ Set Priority]. Implement actions: Call `PostDownloadCommand()` for each selected download sequentially with 100ms delay between requests (prevent overwhelming aMule). Show progress: "Pausing 3 downloads... (2/3)". Show results: "Successfully paused 3 downloads" or "Paused 2 of 3 downloads (1 failed)". Add confirmation dialog for destructive actions (Delete): "Delete 5 selected downloads?". Disable toolbar during action execution. | 10 | 🟡 Medium | Medium |
| **5.15** | **Advanced Batch Features** | Add advanced batch operations: (1) "Pause all completed" - pauses downloads at 100% that are still seeding, (2) "Resume all paused" - resumes only paused downloads (not stopped), (3) "Cancel all queued" - cancels downloads in queue state. Add batch priority change: dropdown with options (Low, Normal, High, Auto), applies to all selected. Add filter + batch: "Select all downloads >1GB", "Select all paused downloads". Add keyboard shortcuts: Ctrl+A (select all), Ctrl+D (deselect), Delete (delete selected). Add undo for batch delete (store in memory for 30 seconds with toast "Undo"). Test with 100+ downloads. Update CLAUDE.md with batch operations documentation. | 12 | 🟡 Medium | Medium |

**Sprint 17 Total:** 30 hours
**Commit Message:** `feat: Add batch operations for downloads with multi-select and advanced actions (refs #17)`
**Release:** 🚀 **v2.5 - "Feature Release"**

---

## 🚀 Release Strategy

### Version Naming Scheme

**Format:** `MAJOR.MINOR.PATCH`

- **MAJOR (2):** Breaking changes, major architecture overhaul
- **MINOR (1-5):** New features, non-breaking improvements
- **PATCH (0):** Bug fixes, minor tweaks (not used in this plan)

### Release Schedule

| Version | Sprint | Phase | Release Date | Type | Content |
|---------|--------|-------|--------------|------|---------|
| **v2.1** | 3 | Phase 1 Complete | Week 3 | 🔴 Critical | Multi-language support, thread safety, atomic writes, input validation |
| **v2.2** | 6 | Phase 2 Complete | Week 6 | 🔵 Enhancement | Ed2k validation, event-based deep linking, user confirmation |
| **v2.3** | 10 | Phase 3 Complete | Week 10 | 🟢 Architecture | Modular parsers, version detection, 80%+ test coverage |
| **v2.4** | 12 | Phase 4 Complete | Week 12 | 🟣 Stability | Result<T> pattern, error handling, diagnostics, code cleanup |
| **v2.5** | 17 | Phase 5 Complete | Week 17 | 🟡 Feature | Dark mode, notifications, statistics dashboard, batch operations |

### Release Process (Per Phase)

#### 1. Pre-Release Checklist

- [ ] All sprint tasks completed and tested
- [ ] No critical bugs in issue tracker
- [ ] All unit tests passing (80%+ coverage)
- [ ] Manual testing completed on Android device
- [ ] Documentation updated (CLAUDE.md, XML comments)
- [ ] Commit history clean (weekly commits from sprints)

#### 2. Build & Package

```bash
# Clean build
dotnet clean
dotnet restore

# Build for Android (release configuration)
dotnet build -f net10.0-android -c Release

# Generate APK
dotnet build -f net10.0-android -c Release -t:SignAndroidPackage

# Output: bin/Release/net10.0-android/com.companyname.amuleremotegui-Signed.apk
```

#### 3. Release Notes Template

```markdown
# AmuleRemoteControl v2.X - "Release Name"

**Release Date:** YYYY-MM-DD
**Phase:** Phase X Complete
**Build:** XX (increment ApplicationVersion in .csproj)

## 🎉 What's New

[List new features from this phase]

## 🐛 Bug Fixes

[List bugs fixed from this phase]

## 🔧 Improvements

[List improvements and refactorings]

## ⚠️ Breaking Changes

[List any breaking changes, if applicable]

## 📦 Installation

Download the APK from [Releases](link) and install on Android.

## 🔄 Upgrade Notes

[Any special upgrade instructions]

## 📝 Known Issues

[Any known issues to be addressed in future releases]

## 🙏 Acknowledgments

Built with .NET 10.0 MAUI Blazor and Radzen.Blazor 8.3.6
```

#### 4. Git Tagging

```bash
# Create annotated tag
git tag -a v2.1 -m "v2.1 - Stability Release"

# Push tag to remote
git push origin v2.1
```

#### 5. Distribution

- **GitHub Releases:** Upload APK to GitHub Releases with release notes
- **Internal Testing:** Share APK with test users via link
- **Google Play (Future):** Submit to Google Play Console (requires Play Store account)

### Commit Strategy

**Frequency:** Weekly (after each sprint)

**Commit Message Format:**
```
<type>: <short description> (<reference>)

<detailed description>

Tasks completed:
- Task X.Y: Description
- Task X.Z: Description

Testing: <testing notes>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `refactor`: Code refactoring (no behavior change)
- `docs`: Documentation only
- `test`: Adding tests
- `chore`: Maintenance tasks

**Example:**
```
feat: Add multi-language support with culture selection (fixes #1)

Implemented ICultureProvider interface and removed all hardcoded it-IT
culture references. Users can now select from 5 cultures: en-US, it-IT,
de-DE, fr-FR, es-ES.

Tasks completed:
- 1.1: Fix Culture Hardcoding - Implementation
- 1.2: Fix Culture Hardcoding - Settings UI
- 1.3: Fix Culture Hardcoding - Testing

Testing: Verified number formatting, currency, and dates for all cultures.
Confirmed aMule parsing still works correctly.
```

### Branching Strategy

**Main Branch:** `main` (always deployable)

**Feature Branches:** Not used (solo developer, weekly commits to main)

**Hotfix Process:**
1. If critical bug found in released version
2. Create branch `hotfix/v2.x.1` from tag `v2.x`
3. Fix bug, test, commit
4. Merge to `main`
5. Create new tag `v2.x.1`
6. Release hotfix version

### Rollback Plan

If release has critical bug:

1. **Immediate:** Remove APK download link from release page
2. **Communication:** Post issue in GitHub Issues warning users
3. **Investigation:** Determine root cause and impact
4. **Decision:**
   - If fixable in <1 day: Create hotfix (v2.x.1)
   - If requires >1 day: Revert to previous version tag, postpone release
5. **Fix:** Implement fix, test thoroughly
6. **Re-release:** Increment patch version (v2.x.1)

---

## 🧪 Testing Strategy

### Test Pyramid

```
           /\
          /  \        E2E Tests (Manual)
         /----\       ~10 tests, critical flows
        /      \
       /--------\     Integration Tests
      /          \    ~30 tests, service interactions
     /------------\
    /              \  Unit Tests
   /----------------\ ~100 tests, parsers, utilities
```

### Unit Testing (80%+ Coverage Goal)

**Target Files:**
- All parser implementations (IDownloadParser, IUploadParser, etc.)
- Ed2kUrlParser
- UtilityServices formatting methods
- DeepLinkService event handling

**Framework:** xUnit + FluentAssertions

**Mock Strategy:**
- HTML mocking: Use saved HTML fixtures from task 3.3
- Network mocking: Mock INetworkHelper responses
- File system mocking: Use in-memory file system or temp directories

**Example Test:**
```csharp
[Fact]
public void DownloadParser_WithValidHtml_ReturnsDownloadList()
{
    // Arrange
    var html = LoadFixture("downloads-single.html");
    var parser = new DownloadParser(new XPathConfiguration());

    // Act
    var result = parser.Parse(html);

    // Assert
    result.Success.Should().BeTrue();
    result.Data.Should().HaveCount(1);
    result.Data[0].FileName.Should().Be("Ubuntu.iso");
    result.Data[0].FileSize.Should().Be("699400192");
}
```

### Integration Testing

**Target Scenarios:**
- Full deep linking flow: Intent → MainActivity → DeepLinkService → Home → AddLink
- Authentication flow: LoginPage → AccessService → NetworkHelper → aMule
- Download management: DownloadingHome → aMuleRemoteService → Parsers → UI update
- Settings persistence: Setting.razor → UtilityServices → File system → Reload

**Approach:**
- Use TestHost for Blazor component testing
- Mock external dependencies (aMule HTTP responses)
- Test service interactions via DI container

### End-to-End Testing (Manual)

**Test Matrix (Android Device Required):**

| Test ID | Scenario | Steps | Expected Result | Status |
|---------|----------|-------|-----------------|--------|
| E2E-001 | Fresh Install | Install APK, open app | Shows login page, no crashes | ⬜ |
| E2E-002 | First Login | Enter server IP, password, login | Connects to aMule, navigates to downloads | ⬜ |
| E2E-003 | View Downloads | Navigate to downloads page | Shows download list with correct data | ⬜ |
| E2E-004 | Pause Download | Click pause on download | Download pauses, status updates | ⬜ |
| E2E-005 | Resume Download | Click resume on paused download | Download resumes, speed shown | ⬜ |
| E2E-006 | Search Files | Navigate to search, enter query | Shows search results | ⬜ |
| E2E-007 | Add via Search | Click download on search result | Download starts | ⬜ |
| E2E-008 | Deep Link - Cold Start | Share ed2k link while app closed | App launches, shows confirmation, adds download | ⬜ |
| E2E-009 | Deep Link - Warm Start | Share ed2k link while app running | App comes to foreground, shows confirmation | ⬜ |
| E2E-010 | Invalid Deep Link | Share malformed ed2k link | Shows error toast, doesn't crash | ⬜ |
| E2E-011 | Change Culture | Go to settings, change to German | UI updates, numbers formatted correctly | ⬜ |
| E2E-012 | Dark Mode | Toggle dark mode in settings | Theme switches immediately | ⬜ |
| E2E-013 | Download Notification | Wait for download to complete | Notification appears | ⬜ |
| E2E-014 | Statistics View | Navigate to statistics page | Shows charts and history | ⬜ |
| E2E-015 | Batch Operations | Select 3 downloads, pause all | All 3 pause, success message | ⬜ |
| E2E-016 | App Backgrounding | Use app, press home, return | State preserved, no crashes | ⬜ |
| E2E-017 | Connection Loss | Disconnect WiFi while using | Shows error, reconnects when WiFi returns | ⬜ |
| E2E-018 | Settings Persistence | Change settings, close app, reopen | Settings persisted | ⬜ |

**Testing Frequency:**
- After each sprint: Smoke test (E2E-001 to E2E-007)
- Before each release: Full regression test (all E2E tests)
- After critical bug fix: Targeted test (affected scenario + regression)

### Performance Testing

**Metrics:**

| Metric | Current | Target | Critical |
|--------|---------|--------|----------|
| **App Startup Time** | ~2s | <2s | <3s |
| **Login Response** | ~1s | <1s | <2s |
| **Download List Load** | ~1s | <1s | <3s |
| **Search Response** | ~2s | <2s | <5s |
| **Parse 100 Downloads** | ~500ms | <500ms | <1s |
| **Theme Switch** | ~200ms | <200ms | <500ms |
| **Database Query** | ~50ms | <50ms | <200ms |

**Tools:**
- Stopwatch timing in code (logged to Serilog)
- Android Profiler (CPU, memory, network)
- Manual observation during testing

### Test Data

**Mock aMule HTML Fixtures (Created in Sprint 7):**
- `downloads-empty.html` - No downloads
- `downloads-single.html` - One download
- `downloads-10.html` - Ten downloads (typical)
- `downloads-100.html` - 100 downloads (stress test)
- `downloads-paused.html` - Mix of paused/active
- `downloads-completed.html` - Completed downloads
- `downloads-malformed.html` - Missing columns
- `uploads-*.html` - Similar variations
- `servers-*.html` - Similar variations

**Real ed2k Links (For Testing):**
```
Valid:
ed2k://|file|Ubuntu-20.04.iso|2877227008|5E0A6F1D2C3B4A5D6E7F8A9B0C1D2E3F|/

With hashset:
ed2k://|file|Test.avi|104857600|ABCD1234ABCD1234ABCD1234ABCD1234|h=ZYXW9876ZYXW9876ZYXW9876ZYXW9876|/

Invalid:
ed2k://|file|NoHash.zip|1024||/
ed2k://|file||1024|ABCD1234|/
malformed-link-without-pipes
```

---

## ⚠️ Risk Management

### High-Risk Items

| Risk ID | Risk | Probability | Impact | Mitigation | Owner |
|---------|------|-------------|--------|------------|-------|
| **R-001** | HTML parsing breaks on aMule update | Medium | High | Modular parser with version detection (Phase 3), comprehensive test fixtures, monitor aMule releases | Developer |
| **R-002** | Android permission changes break notifications | Low | Medium | Follow Android best practices, test on multiple Android versions (8-14), handle permission denial gracefully | Developer |
| **R-003** | SQLite database corruption | Low | Medium | Implement database backup before writes, add corruption detection and recovery, test with large datasets | Developer |
| **R-004** | Deep linking security vulnerability | Low | High | Comprehensive URL validation (Phase 2), user confirmation dialog, input sanitization, security audit | Developer |
| **R-005** | Thread race conditions cause crashes | Medium | High | Implement proper locking (Phase 1), test concurrent scenarios, use thread-safe collections | Developer |
| **R-006** | Performance degradation with 1000+ downloads | Medium | Medium | Implement virtualization in RadzenDataGrid, pagination, lazy loading, performance testing | Developer |
| **R-007** | Breaking changes in Radzen.Blazor updates | Low | Medium | Pin Radzen version in .csproj, test updates in separate branch before merging, monitor release notes | Developer |
| **R-008** | Settings file corruption loses config | Medium | Medium | Atomic writes (Phase 1), backup on every change, recovery mechanism to regenerate defaults | Developer |
| **R-009** | Background service killed by Android | Medium | Low | Use WorkManager for critical background tasks, implement restart mechanism, reduce polling frequency | Developer |
| **R-010** | iOS platform differences break features | Low | High | Focus on Android first (stated constraint), document iOS limitations, test on iOS simulator before release | Developer |

### Risk Response Strategies

**For Each Risk:**

1. **R-001: HTML Parsing Breaks**
   - **Prevention:** Version detection (task 3.11), modular parsers (Phase 3)
   - **Detection:** Unit tests fail, user reports parsing errors
   - **Response:** Update XPath configuration, release hotfix within 24 hours
   - **Fallback:** Show cached data with "outdated" warning

2. **R-002: Notification Permissions**
   - **Prevention:** Request permissions properly, handle denials
   - **Detection:** Notification not shown, permission check fails
   - **Response:** Show in-app message explaining how to enable
   - **Fallback:** Disable notification feature gracefully

3. **R-003: Database Corruption**
   - **Prevention:** WAL mode for SQLite, atomic writes
   - **Detection:** SQLite exception on open/query
   - **Response:** Delete corrupted database, regenerate from scratch, log incident
   - **Fallback:** Statistics feature shows "no data" but app still works

4. **R-004: Deep Linking Security**
   - **Prevention:** Ed2k parser validation (Phase 2), user confirmation
   - **Detection:** Security researcher report, automated vulnerability scan
   - **Response:** Release hotfix immediately, notify users
   - **Fallback:** Disable deep linking until fixed

5. **R-005: Thread Race Conditions**
   - **Prevention:** Locking mechanisms (Phase 1), thread-safe collections
   - **Detection:** Rare crashes, data corruption, hard to reproduce
   - **Response:** Add more logging, reproduce in debugger, fix and test thoroughly
   - **Fallback:** Reduce concurrent operations, add delays

### Dependency Risks

**External Dependencies:**

| Dependency | Version | Risk | Mitigation |
|------------|---------|------|------------|
| **.NET MAUI** | 10.0 | Breaking changes in updates | Pin version, test updates in separate branch |
| **Radzen.Blazor** | 8.3.6 | API changes, bugs | Pin version, monitor release notes |
| **HtmlAgilityPack** | 1.12.4 | Low risk, stable | Update only for security fixes |
| **Serilog** | 4.3.0 | Low risk, stable | Update periodically |
| **Plugin.Fingerprint** | 2.1.5 | Android biometric API changes | Test on multiple devices |
| **aMule Web Interface** | Unknown | HTML structure changes | Version detection, flexible XPath |

**Mitigation:** Pin all NuGet package versions in .csproj, update only intentionally with testing

---

## 📈 Success Metrics

### Technical Metrics

| Metric | Baseline (v2.0) | Target (v2.5) | Measurement Method |
|--------|-----------------|---------------|-------------------|
| **Code Coverage** | 0% | 80%+ | Run `dotnet test --collect:"XPlat Code Coverage"` |
| **Critical Bugs** | 6 | 0 | GitHub Issues tagged "critical" |
| **Code Smells** | 17 major | 0 | Manual code review checklist |
| **Commented Code Lines** | 100+ | 0 | Grep for `//` in .cs files |
| **Average Response Time** | 2s | <1s | Stopwatch timing in Serilog |
| **Crash Rate** | Unknown | <1% | Android Vitals / user reports |
| **APK Size** | ~15MB | <20MB | Build output |

### User Experience Metrics

| Metric | Baseline | Target | Measurement |
|--------|----------|--------|-------------|
| **Supported Languages** | 1 (it-IT) | 5+ | Settings UI |
| **Theme Options** | 1 (light) | 3 (light/dark/system) | Settings UI |
| **User-Reported Bugs** | Unknown | <5 per month | GitHub Issues |
| **Setup Time (First Use)** | ~2 min | <1 min | Manual timing |
| **Downloads Managed** | Unlimited | Unlimited + batch ops | Feature parity |

### Process Metrics

| Metric | Target | Measurement |
|--------|--------|-------------|
| **Sprint Completion Rate** | 100% | Tasks completed / planned |
| **Commit Frequency** | Weekly | Git log analysis |
| **Release Cadence** | Every 3-4 weeks | GitHub Releases |
| **Documentation Coverage** | 100% public APIs | XML comments present |
| **Test Pass Rate** | 100% | xUnit test runner |

### Quality Gates (Per Release)

Before tagging release, all must be ✅:

- [ ] All sprint tasks for phase completed
- [ ] Zero critical bugs open
- [ ] Test coverage ≥80% for new code
- [ ] All E2E smoke tests passing
- [ ] Performance metrics within targets
- [ ] CLAUDE.md updated with changes
- [ ] Release notes drafted
- [ ] APK builds successfully
- [ ] Manual testing completed on Android device

---

## 📚 Appendices

### Appendix A: Technology Stack Details

**Framework & Language:**
- .NET 10.0 (Released Nov 2024)
- C# 12 with nullable reference types enabled
- MAUI Blazor Hybrid (Razor components in WebView)

**UI & Components:**
- Radzen.Blazor 8.3.6 (100+ components, Material Design)
- Custom CSS for theme overrides (wwwroot/css/)

**Data & Persistence:**
- System.Text.Json 10.0.0 (JSON serialization)
- SQLite (via sqlite-net-pcl, Phase 5)
- File-based settings (AppDataDirectory)
- SecureStorage API (passwords)

**Networking:**
- HttpClient with IHttpClientFactory
- Gzip/Deflate automatic decompression
- 30-second timeout

**Parsing:**
- HtmlAgilityPack 1.12.4 (XPath/LINQ queries)
- Custom parsers (Phase 3)

**Logging:**
- Serilog 4.3.0 (structured logging)
- File sink (rolling daily, 5-day retention)
- Debug sink (Visual Studio output)

**Platform Services:**
- Plugin.Fingerprint 2.1.5 (biometric auth)
- Android WorkManager (background tasks, Phase 5)
- Android Notifications (local notifications)

### Appendix B: File Structure

```
AmuleRemoteControl/
├── Components/
│   ├── Data/
│   │   ├── AmuleModel/           # Domain models (DownloadFile, Server, etc.)
│   │   ├── Setting/              # Settings models (NetworkSetting, LoginData)
│   │   ├── Ed2kUrl.cs           # [DEPRECATED] Remove in Sprint 5
│   │   └── Result.cs            # [NEW] Sprint 11
│   ├── Interfaces/
│   │   ├── IAmuleRemoteServices.cs
│   │   ├── IAccessService.cs
│   │   ├── IUtilityServices.cs
│   │   ├── INetworkHelper.cs
│   │   ├── IDeepLinkService.cs   # [NEW] Sprint 5
│   │   ├── ICultureProvider.cs   # [NEW] Sprint 1
│   │   ├── IThemeService.cs      # [NEW] Sprint 13
│   │   ├── INotificationService.cs # [NEW] Sprint 14
│   │   └── Parsers/              # [NEW] Sprint 7-9
│   │       ├── IDownloadParser.cs
│   │       ├── IUploadParser.cs
│   │       ├── IServerParser.cs
│   │       ├── IStatsParser.cs
│   │       ├── ISearchParser.cs
│   │       └── IPreferencesParser.cs
│   ├── Service/
│   │   ├── aMuleRemoteService.cs  # Refactored in Phase 3
│   │   ├── AccessService.cs       # Updated in Phase 1
│   │   ├── UtilityServices.cs     # Updated in Phase 1
│   │   ├── NetworkHelper.cs
│   │   ├── SessionStorageAccessor.cs
│   │   ├── DeepLinkService.cs     # [NEW] Sprint 5
│   │   ├── CultureProvider.cs     # [NEW] Sprint 1
│   │   ├── ThemeService.cs        # [NEW] Sprint 13
│   │   ├── StatisticsDatabase.cs  # [NEW] Sprint 15
│   │   └── Parsers/               # [NEW] Sprint 7-9
│   │       ├── DownloadParser.cs
│   │       ├── UploadParser.cs
│   │       ├── ServerParser.cs
│   │       ├── StatsParser.cs
│   │       ├── SearchParser.cs
│   │       └── PreferencesParser.cs
│   ├── Pages/
│   │   ├── Home.razor             # Updated in Sprint 5
│   │   ├── Access/LoginPage.razor
│   │   ├── Settings/Setting.razor # Updated in Sprint 1, 6, 13, 14
│   │   ├── aMule/
│   │   │   ├── DownloadingHome.razor # Updated in Sprint 17
│   │   │   ├── UploadFileHome.razor
│   │   │   ├── ServerHome.razor
│   │   │   ├── SearchPage.razor
│   │   │   ├── AddLink.razor      # Updated in Sprint 6
│   │   │   ├── PreferencePage.razor
│   │   │   └── LogAmule.razor
│   │   ├── DiagnosticsPage.razor  # [NEW] Sprint 12
│   │   └── StatisticsPage.razor   # [NEW] Sprint 15-16
│   ├── Layout/
│   │   └── MainLayout.razor       # Updated in Sprint 1, 2
│   └── Routes.razor
├── Platforms/
│   └── Android/
│       ├── MainActivity.cs        # Updated in Sprint 5
│       ├── MainApplication.cs
│       ├── AndroidManifest.xml
│       └── Services/              # [NEW] Sprint 14
│           └── AndroidNotificationService.cs
├── Resources/
│   ├── Raw/
│   │   ├── xpaths.json           # [NEW] Sprint 7
│   │   └── PreferenceMapping.json # [NEW] Sprint 9
│   └── Images/                   # Notification icons added Sprint 14
├── TestFixtures/                 # [NEW] Sprint 7
│   └── AmuleHtml/
│       ├── downloads-*.html
│       ├── uploads-*.html
│       ├── servers-*.html
│       └── ...
├── Tests/                        # [NEW] Sprint 4+
│   ├── ParserTests/
│   │   ├── DownloadParserTests.cs
│   │   ├── UploadParserTests.cs
│   │   └── ...
│   ├── ServiceTests/
│   │   ├── DeepLinkServiceTests.cs
│   │   └── ...
│   └── IntegrationTests/
├── wwwroot/
│   ├── index.html
│   ├── css/
│   │   ├── app.css
│   │   └── theme-dark.css        # [NEW] Sprint 13
│   └── Images/
├── App.xaml / App.xaml.cs
├── MainPage.xaml / MainPage.xaml.cs
├── MauiProgram.cs                # Updated each sprint (DI registrations)
├── AmuleRemoteControl.csproj     # Updated Sprint 7, 15 (NuGet packages)
├── CLAUDE.md                     # Updated continuously
├── MasterPlan.md                 # This document
└── README.md

```

### Appendix C: Configuration Files

**GlobalSettings.json** (AppDataDirectory)
```json
{
  "Settings": [
    { "Key": "NetworkSettingFileName", "Value": "NetworkSetting.json" },
    { "Key": "LoginSettings", "Value": "LoginSettings.json" },
    { "Key": "Culture", "Value": "en-US" },
    { "Key": "LogLevel", "Value": "Information" }
  ]
}
```

**NetworkSetting.json**
```json
{
  "Profiles": [
    {
      "Name": "Local aMule",
      "Address": "192.168.1.100",
      "Port": "4711",
      "IsActive": true
    },
    {
      "Name": "Remote Server",
      "Address": "amule.example.com",
      "Port": "4711",
      "IsActive": false
    }
  ]
}
```

**CustomSettings.json**
```json
{
  "LastLoginDateTime": "2025-12-04T10:30:00Z",
  "Theme": "Dark",
  "NotificationsEnabled": true,
  "NotifyAllDownloads": true,
  "NotificationSound": true,
  "SkipDeepLinkConfirmation": false,
  "DeepLinkStats": {
    "TotalReceived": 42,
    "TotalAccepted": 38,
    "TotalRejected": 4,
    "LastReceivedAt": "2025-12-04T12:15:00Z"
  }
}
```

**LoginSettings.json**
```json
{
  "RememberPsw": true,
  "UseBiometric": false,
  "AutomaticLogin": true,
  "Password": ""
}
```
*Note: Actual password stored in SecureStorage, not JSON*

**xpaths.json** (Resources/Raw, new in Sprint 7)
```json
{
  "versions": {
    "2.3.2": {
      "downloadTable": "//table[@class='downloads']",
      "downloadTableIndex": 6,
      "uploadTable": "//table[@class='uploads']",
      "uploadTableIndex": 8,
      "serverTable": "//table[@class='servers']",
      "statsDiv": "//div[@id='stats']"
    },
    "default": {
      "downloadTable": "//table",
      "downloadTableIndex": 6,
      "uploadTable": "//table",
      "uploadTableIndex": 8
    }
  }
}
```

### Appendix D: Key Decisions Log

| Date | Decision | Rationale | Impact |
|------|----------|-----------|--------|
| 2025-12-04 | Keep Radzen.Blazor instead of migrating to MudBlazor | RadzenDataGrid is superior for data-heavy app, migration effort (40-60h) not justified, MudBlazor has documented performance issues with large datasets | No UI framework change, focus effort on architecture |
| 2025-12-04 | Support current aMule version only | Reduces complexity, version detection can be added later if needed | Simpler XPath configuration, faster development |
| 2025-12-04 | Use HTML mocking instead of real aMule for tests | More reliable, faster tests, reproducible failures, no external dependency | Requires capturing good fixtures (Sprint 7) |
| 2025-12-04 | Maintain backward compatibility for settings | Users shouldn't lose configuration on upgrade | Requires migration code if format changes |
| 2025-12-04 | Weekly commits after each sprint | Provides regular checkpoints, easier to track progress, reduces merge conflicts (solo dev) | Requires discipline to commit weekly |
| 2025-12-04 | Phase-based releases (v2.1-2.5) | Users get improvements incrementally, easier to test and rollback | More release overhead but better UX |
| 2025-12-04 | Android-only focus (iOS future) | Limited resources (solo dev), Android is primary platform for aMule users | Faster development, iOS can be added later |
| 2025-12-04 | SQLite for statistics storage | Structured queries, better performance than JSON for time-series data, built-in aggregation | Adds dependency but worth it for Phase 5 features |
| 2025-12-04 | Result<T> pattern instead of exceptions | Explicit error handling, better performance, clearer API contracts | Breaking change (Phase 4) requires updating all callers |
| 2025-12-04 | Event bus for deep linking instead of singleton | Testable, follows SOLID principles, no global state | Requires more code but better architecture |

### Appendix E: Glossary

**Terms Used in This Plan:**

- **aMule:** Open-source P2P file-sharing application (eMule clone)
- **ed2k:** eDonkey2000 protocol link format for P2P files
- **Blazor Hybrid:** Blazor components hosted in native MAUI WebView
- **Deep Linking:** Opening app from external URL (ed2k:// scheme)
- **XPath:** XML Path Language for selecting HTML nodes
- **DI:** Dependency Injection (service registration pattern)
- **RadzenDataGrid:** Radzen's data grid component (table with features)
- **Scoped Service:** DI lifetime where instance created per request
- **Singleton Service:** DI lifetime where single instance shared across app
- **Result<T>:** Pattern for explicit success/failure return values
- **HTML Scraping:** Parsing HTML to extract data (aMule has no API)
- **Magic Number:** Hardcoded numeric value with no explanation
- **Code Smell:** Code pattern that suggests deeper problem
- **Race Condition:** Bug caused by timing of concurrent operations
- **Atomic Write:** File write operation that can't be interrupted

**Acronyms:**

- **MAUI:** Multi-platform App UI (.NET cross-platform framework)
- **APK:** Android Package (Android app file format)
- **UI/UX:** User Interface / User Experience
- **DI:** Dependency Injection
- **JSON:** JavaScript Object Notation (data format)
- **CSV:** Comma-Separated Values (export format)
- **HTTP:** HyperText Transfer Protocol
- **HTTPS:** HTTP Secure (encrypted)
- **TCP:** Transmission Control Protocol
- **E2E:** End-to-End (full user flow testing)
- **XML:** eXtensible Markup Language
- **KB/MB/GB:** Kilobyte / Megabyte / Gigabyte
- **KB/s:** Kilobytes per second (speed)

### Appendix F: Contact & Support

**Developer:**
- GitHub: [Repository Link]
- Issues: [GitHub Issues Link]

**Documentation:**
- CLAUDE.md: Architecture and development guide (updated continuously)
- README.md: User-facing installation and usage
- This Document (MasterPlan.md): Comprehensive development plan

**aMule Resources:**
- Official Site: http://www.amule.org/
- Web Interface Guide: [Link to aMule wiki]

---

## 📝 Document Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-12-04 | Developer | Initial master plan created based on architecture analysis |
| 1.1 | 2025-12-04 | Developer | Updated with Sprint 1 completion status. Added Current Progress section. Status changed to "In Progress (Sprint 2)" |
| 1.2 | 2025-12-04 | Developer | Sprint 2 completed. Thread-safe state management, standardized events, memory leak audit done. Phase 1 is 67% complete (2/3 sprints). Ready for Sprint 3. |

---

**Plan Status:** 🚧 In Progress - Ready for Sprint 3

**Next Steps:**
1. ✅ ~~Begin Sprint 1 (Culture Internationalization)~~ - COMPLETE
2. ✅ ~~Complete Sprint 2 (Thread Safety & Events)~~ - COMPLETE
3. 🚧 Begin Sprint 3 (Atomic Writes & Validation) - NEXT
4. 🚀 Release v2.1 after Sprint 3 completion
5. ⬜ Continue with Phase 2 sprints

---

*This master plan is a living document and should be updated as the project evolves. Major changes should be documented in the revision history.*
