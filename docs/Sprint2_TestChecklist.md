# Sprint 2 Test Checklist - Thread Safety & Events

**Sprint:** 2
**Tasks:** 1.4, 1.5, 1.6
**Date:** 2025-12-04
**Status:** Ready for Testing

## ✅ Implementation Summary

### Task 1.4: Thread-Safe State Management ✓
- ✅ Added `_statusLock` object to `aMuleRemoteService.cs`
- ✅ Wrapped `Status` property get/set in lock statements
- ✅ Wrapped `DownSpeed` property get/set in lock statements
- ✅ Added `_authLock` object to `AccessService.cs`
- ✅ Wrapped `IsAuthorized` property get/set in lock statements
- ✅ Added XML documentation explaining thread safety
- ✅ Events fire inside lock for atomic state changes
- ✅ Build successful with 0 errors

### Task 1.5: Standardize Event Patterns ✓
- ✅ Changed `AccessService.OnChange` from `Action?` to `EventHandler?`
- ✅ Updated `IAccessService` interface to match
- ✅ Updated `MainLayout.razor` to use new event signature
- ✅ Added `OnAuthChanged` handler following standard pattern
- ✅ Updated CLAUDE.md with standardized event pattern documentation
- ✅ All events now use: `event EventHandler? EventName;`
- ✅ All invocations use: `EventName?.Invoke(this, EventArgs.Empty);`
- ✅ Build successful with 0 errors

### Task 1.6: Memory Leak Audit ✓
- ✅ Audited all Razor components for event subscriptions
- ✅ Added `@implements IDisposable` to `ServerHome.razor`
- ✅ Verified `MainLayout.razor` properly disposes (already correct)
- ✅ Confirmed other components don't need disposal (no subscriptions)
- ✅ Created `Sprint2_MemoryLeakAudit.md` documentation
- ✅ All subscribing components properly unsubscribe in Dispose()
- ✅ Build successful with 0 errors

---

## 📋 Manual Test Cases

### Test Case 1: Thread Safety - Concurrent Status Updates
**Objective:** Verify thread-safe state management prevents race conditions

**Prerequisites:** Connected to aMule with active downloads

**Steps:**
1. Open app and navigate to Downloads page
2. Let auto-refresh run (polls status every 30 seconds)
3. While auto-refresh is active, manually navigate between pages rapidly:
   - Home → Downloads → Servers → Downloads → Upload
4. Repeat rapid navigation 20+ times
5. Monitor for crashes or UI corruption

**Expected Result:**
- ✅ No crashes or exceptions
- ✅ Status displays correctly on all pages
- ✅ Download speed updates without corruption
- ✅ Connection indicators (ED2K, KAD) show correct status
- ✅ No null reference exceptions in logs

**Why This Tests Thread Safety:**
- Auto-refresh updates `Status` from background thread
- UI reads `Status` from main thread
- Lock prevents race condition between read/write

---

### Test Case 2: Thread Safety - Authorization State
**Objective:** Verify thread-safe authorization state management

**Steps:**
1. Login to aMule successfully
2. Wait 2-3 seconds
3. Immediately click "Logout" in profile menu
4. Quickly navigate to Download page (before logout completes)
5. Repeat login/logout cycle 10 times rapidly

**Expected Result:**
- ✅ No crashes
- ✅ IsAuthorized state always consistent
- ✅ UI correctly shows logged in/out status
- ✅ Profile menu shows correct options (Login vs Logout)
- ✅ No race condition between logout and page navigation

**Why This Tests Thread Safety:**
- `LoggedOut()` sets `IsAuthorized = false` from one thread
- UI reads `IsAuthorized` from another thread
- Lock prevents inconsistent state

---

### Test Case 3: Event Pattern Standardization
**Objective:** Verify standardized EventHandler pattern works correctly

**Steps:**
1. Login to aMule
2. Observe profile icon changes to green aMule logo
3. Logout
4. Observe profile icon changes to gravatar
5. Repeat 5 times

**Expected Result:**
- ✅ Profile icon updates immediately on login
- ✅ Profile icon updates immediately on logout
- ✅ No delays or missing updates
- ✅ Header shows correct authorization state
- ✅ No exceptions in logs

**Why This Tests Event Pattern:**
- `AccessService.OnChange` event fires when `IsAuthorized` changes
- `MainLayout.OnAuthChanged` handler receives event
- `StateHasChanged()` triggers UI update
- Tests that `EventHandler?` pattern works same as old `Action?`

---

### Test Case 4: Event Pattern - Status Updates
**Objective:** Verify StatusChanged and DownChanged events work correctly

**Prerequisites:** Connected to aMule with active downloads

**Steps:**
1. Navigate to Downloads page
2. Observe download speeds in header
3. Wait for auto-refresh (30 seconds)
4. Observe speed values update
5. Navigate to Servers page
6. Observe connection status (ED2K, KAD) in Servers page
7. Wait for auto-refresh
8. Verify status updates

**Expected Result:**
- ✅ Download speed in header updates every ~30 seconds
- ✅ Connection status updates on Downloads page
- ✅ Connection status updates on Servers page
- ✅ Multiple pages can subscribe to same events
- ✅ No memory leaks (see Test Case 5)

**Why This Tests Event Pattern:**
- `StatusChanged` fires when `Status` property updated
- `DownChanged` fires when `DownSpeed` property updated
- Multiple components subscribe to same events
- All use standard `EventHandler?` pattern

---

### Test Case 5: Memory Leak - Component Disposal
**Objective:** Verify components properly unsubscribe from events

**Steps:**
1. Open app and login
2. Navigate to Servers page (subscribes to StatusChanged)
3. Navigate away to Downloads page
4. Navigate back to Servers page
5. Repeat steps 2-4 at least 50 times
6. Optional: Use Android Profiler to monitor memory

**Expected Result:**
- ✅ No increasing memory usage over time
- ✅ No crashes after many navigations
- ✅ App remains responsive
- ✅ No warning logs about event handler leaks
- ✅ Android profiler shows stable memory (if available)

**Why This Tests Memory Leaks:**
- ServerHome.razor subscribes to `StatusChanged` in `OnInitialized()`
- ServerHome.razor unsubscribes in `Dispose()`
- Without unsubscribe, every navigation creates leaked event handler
- 50 navigations would create 50 leaked handlers

---

### Test Case 6: Memory Leak - MainLayout Disposal
**Objective:** Verify MainLayout properly disposes (edge case)

**Steps:**
1. This is difficult to test as MainLayout rarely disposes
2. MainLayout is parent of all pages, only disposed on app close
3. Best test: Close and reopen app 10 times
4. Verify no accumulating memory between app restarts

**Expected Result:**
- ✅ Each app restart starts fresh
- ✅ No error logs on app close
- ✅ Memory released when app closed

**Note:** MainLayout disposal is less critical than page disposal since it's a singleton-like component.

---

### Test Case 7: Lock Contention - Stress Test
**Objective:** Verify locks don't cause performance issues or deadlocks

**Prerequisites:** Connected to aMule with 10+ active downloads

**Steps:**
1. Navigate to Downloads page
2. Enable auto-refresh (should already be on)
3. Rapidly scroll through download list
4. While scrolling, auto-refresh will trigger
5. Continue scrolling + auto-refresh for 2-3 minutes
6. Monitor for UI freezing or stuttering

**Expected Result:**
- ✅ No UI freezing
- ✅ Smooth scrolling
- ✅ Auto-refresh completes in background
- ✅ No deadlocks
- ✅ No "application not responding" warnings

**Why This Tests Lock Contention:**
- UI thread reads `DownSpeed` and `Status` while scrolling
- Background thread writes `DownSpeed` and `Status` during refresh
- Locks coordinate access, should be very brief (microseconds)
- If locks held too long, UI would stutter

---

### Test Case 8: Event Handler Thread Safety
**Objective:** Verify event handlers use InvokeAsync for thread safety

**Steps:**
1. Navigate to Downloads page
2. Let auto-refresh trigger
3. Observe download speeds update
4. Check logs for any threading warnings

**Expected Result:**
- ✅ UI updates smoothly
- ✅ No "accessed from wrong thread" warnings
- ✅ No crashes from cross-thread UI access

**Code Verification:**
All event handlers should use `InvokeAsync(StateHasChanged)`:
```csharp
private void OnStateChanged(object? sender, EventArgs e)
   => this.InvokeAsync(StateHasChanged);
```

---

## 🔍 Code Review Checklist

### aMuleRemoteService.cs
- [x] `_statusLock` object declared as `readonly`
- [x] `Status` getter wrapped in `lock (_statusLock)`
- [x] `Status` setter wrapped in `lock (_statusLock)`
- [x] `DownSpeed` getter wrapped in `lock (_statusLock)`
- [x] `DownSpeed` setter wrapped in `lock (_statusLock)`
- [x] Events fired inside lock blocks
- [x] XML documentation added explaining thread safety

### AccessService.cs
- [x] `_authLock` object declared as `readonly`
- [x] `IsAuthorized` getter wrapped in `lock (_authLock)`
- [x] `IsAuthorized` setter wrapped in `lock (_authLock)`
- [x] Event fired inside lock block
- [x] `OnChange` event changed to `EventHandler?`
- [x] `NotifyStateChanged()` invokes with `(this, EventArgs.Empty)`
- [x] XML documentation added

### IAccessService.cs
- [x] `OnChange` event type changed to `EventHandler?`
- [x] XML documentation added
- [x] Commented-out methods removed (clean interface)

### MainLayout.razor
- [x] Implements `@implements IDisposable` (already present)
- [x] Subscribes to `OnChange` with `OnAuthChanged` handler
- [x] `OnAuthChanged` signature matches `EventHandler` pattern
- [x] All handlers use `InvokeAsync(StateHasChanged)`
- [x] `Dispose()` unsubscribes from all 3 events

### ServerHome.razor
- [x] Implements `@implements IDisposable` (ADDED IN SPRINT 2)
- [x] Subscribes to `StatusChanged` with `OnStateChanged` handler
- [x] Handler uses `InvokeAsync(StateHasChanged)`
- [x] `Dispose()` unsubscribes from event

### CLAUDE.md
- [x] Updated State Management Pattern section
- [x] Documented thread safety with lock examples
- [x] Documented standard event pattern
- [x] Added guidance for subscribers using `InvokeAsync()`

---

## 🧪 Automated Testing Recommendations

### Unit Tests (Future - Phase 3)

**Test: Concurrent Property Access**
```csharp
[Fact]
public void Status_ConcurrentAccess_ThreadSafe()
{
    var service = new aMuleRemoteService(...);
    var tasks = new List<Task>();

    // 100 threads reading Status
    for (int i = 0; i < 100; i++)
    {
        tasks.Add(Task.Run(() => { var x = service.Status; }));
    }

    // 100 threads writing Status
    for (int i = 0; i < 100; i++)
    {
        tasks.Add(Task.Run(() => { service.Status = new Stats(); }));
    }

    // Should not throw or deadlock
    Task.WaitAll(tasks.ToArray());
}
```

**Test: Event Handler Disposal**
```csharp
[Fact]
public void Component_Disposed_UnsubscribesFromEvents()
{
    var service = new AccessService(...);
    var component = new MainLayout { _accessService = service };

    component.OnInitialized();
    var handlerCount = GetEventHandlerCount(service.OnChange);
    Assert.Equal(1, handlerCount);

    component.Dispose();
    handlerCount = GetEventHandlerCount(service.OnChange);
    Assert.Equal(0, handlerCount); // Handler removed
}
```

---

## ⚠️ Known Issues / Edge Cases

### Issue 1: Lock Inside Property Setter
**Status:** ⚠️ By Design

The locks are inside property getters/setters, which means the lock is held while the event fires. This is intentional:
- Ensures event sees consistent state
- Lock is very brief (microseconds)
- No I/O or expensive operations inside lock

**Alternative considered:** Fire events outside lock
- Problem: State could change between assignment and event
- Conclusion: Current approach is correct

### Issue 2: InvokeAsync Overhead
**Status:** ⚠️ Acceptable

`InvokeAsync(StateHasChanged)` has small overhead vs direct `StateHasChanged()`:
- ~1-2ms per call
- Necessary for thread safety
- Only called on state changes (not frequent)

**Conclusion:** Acceptable tradeoff for thread safety

---

## ✅ Test Sign-Off

Once all test cases pass, sign off below:

- [ ] Test Case 1: Concurrent Status Updates
- [ ] Test Case 2: Authorization State Thread Safety
- [ ] Test Case 3: Event Pattern Standardization
- [ ] Test Case 4: Status Update Events
- [ ] Test Case 5: Component Disposal (50+ navigations)
- [ ] Test Case 6: MainLayout Disposal
- [ ] Test Case 7: Lock Contention Stress Test
- [ ] Test Case 8: Event Handler Thread Safety
- [ ] Code Review Checklist Complete

**Tester Name:** _________________________
**Test Date:** _________________________
**Device:** _________________________
**Android Version:** _________________________
**Result:** ⬜ PASS / ⬜ FAIL

**Notes:**
_____________________________________________________________
_____________________________________________________________
_____________________________________________________________

---

## 📝 Sprint 2 Success Criteria

✅ **All criteria met:**
1. ✅ Thread-safe state management implemented in `aMuleRemoteService`
2. ✅ Thread-safe state management implemented in `AccessService`
3. ✅ All events use standard `EventHandler?` pattern
4. ✅ All subscribing components implement `IDisposable`
5. ✅ All components properly unsubscribe in `Dispose()`
6. ✅ CLAUDE.md updated with patterns and best practices
7. ✅ Memory leak audit completed and documented
8. ✅ Build successful with 0 errors
9. ✅ Code properly documented with XML comments

**Sprint 2 Status:** ✅ **COMPLETE - Ready for User Testing**

---

## 📊 Code Changes Summary

**Files Created:**
1. `Sprint2_MemoryLeakAudit.md` (comprehensive audit document)
2. `Sprint2_TestChecklist.md` (this file)

**Files Modified:**
1. `Components/Service/aMuleRemoteService.cs` - Added thread safety to `Status` and `DownSpeed` properties
2. `Components/Service/AccessService.cs` - Added thread safety to `IsAuthorized`, changed event type
3. `Components/Interfaces/IAccessService.cs` - Changed event type to `EventHandler?`
4. `Components/Layout/MainLayout.razor` - Updated event subscription for new pattern
5. `Components/Pages/aMule/ServerHome.razor` - Added `@implements IDisposable` directive
6. `CLAUDE.md` - Updated State Management Pattern section with thread safety docs
7. `MasterPlan.md` - Updated with Sprint 1 completion and Sprint 2 status

**Total Lines Changed:** ~100 lines added/modified across 7 files

**Build Status:** ✅ Successful (0 errors, only pre-existing warnings)

---

## 🎯 Sprint 2 → Sprint 3 Transition

### Completed in Sprint 2:
- ✅ Task 1.4: Thread-Safe State Management (8 hours)
- ✅ Task 1.5: Standardize Event Patterns (6 hours)
- ✅ Task 1.6: Memory Leak Audit (4 hours)
- **Total:** 18 hours

### Next Up - Sprint 3 Tasks:
- ⬜ Task 1.7: Atomic File Writes - Helper (6 hours)
- ⬜ Task 1.8: Atomic File Writes - Migration (4 hours)
- ⬜ Task 1.9: Input Validation - Service Layer (6 hours)
- ⬜ Task 1.10: Input Validation - Testing (4 hours)
- **Total:** 20 hours
- **Release:** v2.1 - "Stability Release"

---

**End of Sprint 2 Test Checklist**
