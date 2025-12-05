# Sprint 3: Input Validation Test Cases

**Created:** 2025-12-05
**Sprint:** 3 - Atomic Writes & Validation
**Status:** ✅ Complete

---

## Overview

This document contains comprehensive test cases for input validation implemented in Sprint 3, covering both atomic file writes and service layer input validation.

---

## 1. Atomic File Write Test Cases

### Test Group 1.1: WriteJsonAtomically<T>() - Success Scenarios

| Test ID | Scenario | Input | Expected Result | Priority |
|---------|----------|-------|-----------------|----------|
| AFW-001 | Write simple object | `NetworkSetting` object | File created successfully, returns true | 🔴 Critical |
| AFW-002 | Write list object | `List<GlobalSetting>` | File created successfully, returns true | 🔴 Critical |
| AFW-003 | Overwrite existing file | Valid object, existing file | File overwritten atomically, returns true | 🔴 Critical |
| AFW-004 | Write complex nested object | `LoginData` with nested properties | File created successfully, JSON properly formatted | 🟠 High |
| AFW-005 | Write large object | 10MB+ JSON data | File created successfully, no corruption | 🟡 Medium |

### Test Group 1.2: WriteJsonAtomically<T>() - Error Scenarios

| Test ID | Scenario | Input | Expected Result | Priority |
|---------|----------|-------|-----------------|----------|
| AFW-006 | Null target path | `null` path | Returns false, logs error | 🔴 Critical |
| AFW-007 | Empty target path | Empty string path | Returns false, logs error | 🔴 Critical |
| AFW-008 | Null data object | Valid path, `null` data | Returns false, logs error | 🔴 Critical |
| AFW-009 | Invalid directory | Path to non-existent directory | Returns false, logs IOException | 🟠 High |
| AFW-010 | Read-only directory | Path to read-only location | Returns false, logs UnauthorizedAccessException | 🟠 High |
| AFW-011 | Disk full scenario | Valid data, no disk space | Returns false, logs IOException, temp file cleaned up | 🟡 Medium |

### Test Group 1.3: Atomic Write Crash Recovery

| Test ID | Scenario | Test Method | Expected Result | Priority |
|---------|----------|-------------|-----------------|----------|
| AFW-012 | Crash during temp file write | Kill app during write | Original file unchanged, temp file exists | 🔴 Critical |
| AFW-013 | Crash during File.Move | Kill app during move | Either old or new file exists (atomic), no corruption | 🔴 Critical |
| AFW-014 | Crash after File.Move | Kill app after move | New file exists, old file gone, data intact | 🟠 High |
| AFW-015 | Multiple concurrent writes | 5 threads writing different files | All succeed, no race conditions | 🟠 High |

---

## 2. SearchFiles() Validation Test Cases

### Test Group 2.1: Valid Search Input

| Test ID | Scenario | Search Text | Expected Result | Priority |
|---------|----------|-------------|-----------------|----------|
| VAL-001 | Simple search | "ubuntu" | Search executes, returns results | 🔴 Critical |
| VAL-002 | Multi-word search | "ubuntu server iso" | Search executes, text sanitized | 🔴 Critical |
| VAL-003 | Special characters | "file-name_v1.0" | Search executes, characters preserved | 🟠 High |
| VAL-004 | Unicode characters | "übung" | Search executes, unicode preserved | 🟠 High |
| VAL-005 | Numbers only | "123456" | Search executes, returns results | 🟡 Medium |
| VAL-006 | Exactly 100 chars | 100-character string | Search executes, no truncation | 🟠 High |

### Test Group 2.2: Invalid Search Input - Empty/Null

| Test ID | Scenario | Search Text | Expected Result | Priority |
|---------|----------|-------------|-----------------|----------|
| VAL-007 | Null search text | `null` | Returns empty list, logs warning | 🔴 Critical |
| VAL-008 | Empty string | `""` | Returns empty list, logs warning | 🔴 Critical |
| VAL-009 | Whitespace only | `"   "` | Returns empty list, logs warning | 🔴 Critical |
| VAL-010 | Tab characters only | `"\t\t"` | Returns empty list, logs warning | 🟠 High |
| VAL-011 | Newline characters | `"\n\n"` | Returns empty list, logs warning | 🟠 High |

### Test Group 2.3: Invalid Search Input - Length Validation

| Test ID | Scenario | Search Text | Expected Result | Priority |
|---------|----------|-------------|-----------------|----------|
| VAL-012 | 101 characters | 101-char string | Returns empty list, logs warning "exceeds maximum length" | 🔴 Critical |
| VAL-013 | 200 characters | 200-char string | Returns empty list, logs warning | 🟠 High |
| VAL-014 | 1000 characters | 1000-char string | Returns empty list, logs warning | 🟡 Medium |
| VAL-015 | Maximum allowed (100) | 100-char string | Search executes successfully | 🟠 High |

### Test Group 2.4: XSS and Injection Prevention

| Test ID | Scenario | Search Text | Expected Result | Priority |
|---------|----------|-------------|-----------------|----------|
| VAL-016 | XSS script tag | `<script>alert('xss')</script>` | Text HTML-encoded, search executes safely | 🔴 Critical |
| VAL-017 | HTML img tag | `<img src=x onerror=alert(1)>` | Text HTML-encoded, no script execution | 🔴 Critical |
| VAL-018 | SQL injection attempt | `' OR '1'='1` | Text encoded, no SQL executed (note: aMule doesn't use SQL) | 🟠 High |
| VAL-019 | JavaScript injection | `javascript:alert(1)` | Text encoded, no execution | 🟠 High |
| VAL-020 | Path traversal | `../../etc/passwd` | Text encoded, treated as search string | 🟠 High |
| VAL-021 | Null byte injection | `file\0.exe` | Text encoded or rejected | 🟡 Medium |

---

## 3. PostDownloadCommand() Validation Test Cases

### Test Group 3.1: Valid Commands - Single File

| Test ID | Scenario | File ID | Command | Expected Result | Priority |
|---------|----------|---------|---------|-----------------|----------|
| CMD-001 | Pause download | "12345" | "pause" | Command executes, returns download list | 🔴 Critical |
| CMD-002 | Resume download | "67890" | "resume" | Command executes, returns download list | 🔴 Critical |
| CMD-003 | Delete download | "11111" | "delete" | Command executes, returns download list | 🔴 Critical |
| CMD-004 | Cancel download | "22222" | "cancel" | Command executes, returns download list | 🟠 High |
| CMD-005 | Set priority | "33333" | "priority" | Command executes, returns download list | 🟡 Medium |
| CMD-006 | Large file ID | "999999999999" | "pause" | Command executes, large number handled | 🟡 Medium |

### Test Group 3.2: Valid Commands - Multiple Files

| Test ID | Scenario | File IDs | Command | Expected Result | Priority |
|---------|----------|----------|---------|-----------------|----------|
| CMD-007 | Pause 2 files | `["123", "456"]` | "pause" | Command executes for both | 🔴 Critical |
| CMD-008 | Resume 5 files | List of 5 IDs | "resume" | Command executes for all | 🟠 High |
| CMD-009 | Delete 10 files | List of 10 IDs | "delete" | Command executes for all | 🟠 High |

### Test Group 3.3: Invalid File ID - Single File

| Test ID | Scenario | File ID | Command | Expected Result | Priority |
|---------|----------|---------|---------|-----------------|----------|
| CMD-010 | Null file ID | `null` | "pause" | Returns null, logs "fileId is null or empty" | 🔴 Critical |
| CMD-011 | Empty string ID | `""` | "pause" | Returns null, logs "fileId is null or empty" | 🔴 Critical |
| CMD-012 | Whitespace ID | `"   "` | "pause" | Returns null, logs "fileId is null or empty" | 🔴 Critical |
| CMD-013 | Non-numeric ID | "abc123" | "pause" | Returns null, logs "Invalid fileId format" | 🔴 Critical |
| CMD-014 | Special chars ID | "12@#$34" | "pause" | Returns null, logs "Invalid fileId format" | 🟠 High |
| CMD-015 | Negative number | "-12345" | "pause" | Returns null, logs "Invalid fileId format" | 🟠 High |
| CMD-016 | Decimal number | "123.45" | "pause" | Returns null, logs "Invalid fileId format" | 🟡 Medium |
| CMD-017 | SQL injection ID | "1' OR '1'='1" | "pause" | Returns null, logs "Invalid fileId format" | 🟠 High |

### Test Group 3.4: Invalid File ID - Multiple Files

| Test ID | Scenario | File IDs | Command | Expected Result | Priority |
|---------|----------|----------|---------|-----------------|----------|
| CMD-018 | Null list | `null` | "pause" | Returns null, logs "filesId list is null or empty" | 🔴 Critical |
| CMD-019 | Empty list | `[]` | "pause" | Returns null, logs "filesId list is null or empty" | 🔴 Critical |
| CMD-020 | One invalid ID in list | `["123", "abc", "456"]` | "pause" | Returns null, logs "Invalid fileId format 'abc'" | 🔴 Critical |
| CMD-021 | All invalid IDs | `["abc", "def", "ghi"]` | "pause" | Returns null, logs first invalid ID | 🟠 High |

### Test Group 3.5: Invalid Commands

| Test ID | Scenario | File ID | Command | Expected Result | Priority |
|---------|----------|---------|---------|-----------------|----------|
| CMD-022 | Null command | "12345" | `null` | Returns null, logs "command is null or empty" | 🔴 Critical |
| CMD-023 | Empty command | "12345" | `""` | Returns null, logs "command is null or empty" | 🔴 Critical |
| CMD-024 | Whitespace command | "12345" | `"   "` | Returns null, logs "command is null or empty" | 🔴 Critical |
| CMD-025 | Invalid command | "12345" | "hack" | Returns null, logs "Invalid command 'hack'" with allowed list | 🔴 Critical |
| CMD-026 | SQL injection command | "12345" | "DELETE FROM" | Returns null, logs "Invalid command" | 🟠 High |
| CMD-027 | Case variation (uppercase) | "12345" | "PAUSE" | Command executes (ToLower() handles this) | 🟠 High |
| CMD-028 | Case variation (mixed) | "12345" | "PaUsE" | Command executes (ToLower() handles this) | 🟡 Medium |
| CMD-029 | Command with spaces | "12345" | "pause " | Returns null (whitespace not trimmed) | 🟡 Medium |

---

## 4. Integration Test Scenarios

### Test Group 4.1: End-to-End Validation Flow

| Test ID | Scenario | Steps | Expected Result | Priority |
|---------|----------|-------|-----------------|----------|
| INT-001 | Search and download | 1. SearchFiles("ubuntu")<br>2. DownloadSearch(fileId) | Both execute successfully | 🔴 Critical |
| INT-002 | Download and pause | 1. Add download<br>2. PostDownloadCommand(id, "pause") | Download pauses successfully | 🔴 Critical |
| INT-003 | Invalid search then valid | 1. SearchFiles(101 chars)<br>2. SearchFiles("ubuntu") | First fails, second succeeds | 🟠 High |
| INT-004 | Settings save/load | 1. Modify settings<br>2. Save with atomic write<br>3. Restart app<br>4. Load settings | Settings persisted correctly | 🔴 Critical |

### Test Group 4.2: Concurrent Operation Tests

| Test ID | Scenario | Concurrent Operations | Expected Result | Priority |
|---------|----------|----------------------|-----------------|----------|
| INT-005 | Multiple searches | 3 SearchFiles() calls simultaneously | All execute independently | 🟠 High |
| INT-006 | Multiple commands | 5 PostDownloadCommand() calls | All validate and execute | 🟠 High |
| INT-007 | Save settings while reading | Write + Read same file | Atomic write prevents corruption | 🔴 Critical |

---

## 5. Logging Verification Test Cases

### Test Group 5.1: Validation Logging

| Test ID | Scenario | Expected Log Entry | Log Level | Priority |
|---------|----------|-------------------|-----------|----------|
| LOG-001 | Null search text | "SearchFiles: searchText is null or empty" | Warning | 🔴 Critical |
| LOG-002 | Exceeded search length | "SearchFiles: searchText exceeds maximum length of 100 characters" | Warning | 🔴 Critical |
| LOG-003 | Invalid file ID | "PostDownloadCommand: Invalid fileId format '{id}' - must be numeric" | Warning | 🔴 Critical |
| LOG-004 | Invalid command | "PostDownloadCommand: Invalid command '{cmd}' - allowed: pause, resume, delete, cancel, priority" | Warning | 🔴 Critical |
| LOG-005 | Successful search | "SearchFiles: Searching for '{text}' (original length: {len})" | Information | 🟡 Medium |
| LOG-006 | Atomic write success | "WriteJsonAtomically: Successfully wrote file atomically to {filename}" | Information | 🟡 Medium |
| LOG-007 | Atomic write failure | "WriteJsonAtomically: IO error writing to {path}: {message}" | Error | 🔴 Critical |

---

## 6. Performance Test Cases

### Test Group 6.1: Performance Under Load

| Test ID | Scenario | Load | Expected Performance | Priority |
|---------|----------|------|---------------------|----------|
| PERF-001 | Search with max length | 100-char search | < 2 seconds response time | 🟡 Medium |
| PERF-002 | Atomic write 1MB file | 1MB JSON | < 500ms write time | 🟡 Medium |
| PERF-003 | Atomic write 10MB file | 10MB JSON | < 3 seconds write time | 🟢 Low |
| PERF-004 | 100 validation checks | 100 SearchFiles() validations | < 100ms total (1ms each) | 🟡 Medium |

---

## 7. Test Execution Plan

### Phase 1: Unit Testing (Task 1.7-1.8)
- [ ] AFW-001 to AFW-015 (Atomic writes)
- [ ] Manual crash simulation tests
- [ ] Verify temp file cleanup

### Phase 2: Validation Testing (Task 1.9)
- [ ] VAL-001 to VAL-021 (SearchFiles)
- [ ] CMD-001 to CMD-029 (PostDownloadCommand)
- [ ] All XSS and injection tests

### Phase 3: Integration Testing (Task 1.10)
- [ ] INT-001 to INT-007
- [ ] End-to-end workflows
- [ ] Concurrent operation tests

### Phase 4: Logging Verification
- [ ] LOG-001 to LOG-007
- [ ] Check Serilog output files
- [ ] Verify structured logging

### Phase 5: Performance Testing
- [ ] PERF-001 to PERF-004
- [ ] Measure with Stopwatch
- [ ] Document results

---

## 8. Test Results Summary

| Test Group | Total Tests | Passed | Failed | Skipped | Pass Rate |
|------------|-------------|--------|--------|---------|-----------|
| Atomic File Writes | 15 | - | - | - | - |
| SearchFiles Validation | 21 | - | - | - | - |
| PostDownloadCommand | 29 | - | - | - | - |
| Integration Tests | 7 | - | - | - | - |
| Logging Verification | 7 | - | - | - | - |
| Performance Tests | 4 | - | - | - | - |
| **TOTAL** | **83** | **-** | **-** | **-** | **-%** |

*Note: Fill in results after executing tests*

---

## 9. Known Issues and Limitations

### Identified During Testing
*To be filled during test execution*

### Future Improvements
1. Add unit tests using xUnit (Phase 3 Sprint 7-10)
2. Implement automated integration tests
3. Add performance benchmarking suite
4. Create mock aMule HTML fixtures for testing parsers

---

## 10. Test Environment

**Operating System:** macOS / Android 24+
**Framework:** .NET 10.0 MAUI Blazor
**Build Target:** net10.0-android
**Test Date:** 2025-12-05
**Tester:** Sprint 3 Implementation
**aMule Version:** Not specified (LAN testing required)

---

## 11. Regression Testing Checklist

After completing Sprint 3, verify these existing features still work:

- [ ] Login authentication
- [ ] Download list display
- [ ] Upload list display
- [ ] Server connection
- [ ] Search functionality (basic)
- [ ] Settings persistence
- [ ] Culture selection
- [ ] Theme switching (after Sprint 13)
- [ ] Deep linking (after Sprint 4-6)

---

## 12. Success Criteria

Sprint 3 is considered complete when:

- ✅ All File.WriteAllText() calls replaced with WriteJsonAtomically()
- ✅ SearchFiles() validates input (null, length, XSS)
- ✅ PostDownloadCommand() validates fileId (numeric) and command (allowed list)
- ✅ Build succeeds with 0 errors
- ✅ This test document created with 20+ validation scenarios
- ⬜ Manual testing performed for critical scenarios (VAL-001, VAL-007, VAL-012, VAL-016, CMD-001, CMD-010, CMD-013, CMD-022, CMD-025)
- ⬜ No regressions in existing functionality

---

**Document Status:** ✅ Complete
**Next Step:** Execute manual testing and fill in test results
**Sprint 3 Target:** v2.1 - "Stability Release"
