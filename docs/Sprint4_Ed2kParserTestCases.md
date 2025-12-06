# Sprint 4: Ed2k URL Parser Test Cases

**Created:** 2025-12-05
**Sprint:** 4 - Ed2k URL Parser
**Phase:** 2 - Deep Linking Improvements
**Status:** ✅ Complete

---

## Overview

This document contains comprehensive test cases for the Ed2k URL parser implemented in Sprint 4. The parser validates and extracts components from ed2k:// URLs used by eDonkey2000/aMule networks.

---

## Ed2k Link Format Reference

**Standard Format:**
```
ed2k://|file|filename|filesize|filehash|/
```

**Extended Format (with optional fields):**
```
ed2k://|file|filename|filesize|filehash|h=hashset|s=server:port|/
```

**Components:**
- **filename**: UTF-8 encoded file name (may be URL encoded)
- **filesize**: Positive integer (1 to 16 TB)
- **filehash**: Exactly 32 hexadecimal characters (MD4 hash, case insensitive)
- **h=hashset**: Optional AICH hashset identifier
- **s=server:port**: Optional source server(s)

---

## 1. Valid Ed2k Link Test Cases

### Test Group 1.1: Basic Valid Links

| Test ID | Scenario | Input | Expected Result | Priority |
|---------|----------|-------|-----------------|----------|
| ED2K-001 | Standard valid link | `ed2k://\|file\|Ubuntu-20.04.iso\|2877227008\|5E0A6F1D2C3B4A5D6E7F8A9B0C1D2E3F\|/` | Success: FileName="Ubuntu-20.04.iso", FileSize=2877227008, Hash="5E0A6F1D2C3B4A5D6E7F8A9B0C1D2E3F" | 🔴 Critical |
| ED2K-002 | Lowercase hash | `ed2k://\|file\|test.avi\|104857600\|abcd1234abcd1234abcd1234abcd1234\|/` | Success: Hash normalized to "ABCD1234ABCD1234ABCD1234ABCD1234" | 🔴 Critical |
| ED2K-003 | Mixed case hash | `ed2k://\|file\|test.zip\|1024\|AbCd1234AbCd1234AbCd1234AbCd1234\|/` | Success: Hash normalized to uppercase | 🟠 High |
| ED2K-004 | Small file (1 byte) | `ed2k://\|file\|tiny.txt\|1\|00000000000000000000000000000000\|/` | Success: FileSize=1 (minimum valid) | 🟡 Medium |
| ED2K-005 | Large file (1 TB) | `ed2k://\|file\|huge.iso\|1099511627776\|FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF\|/` | Success: FileSize=1099511627776 | 🟡 Medium |

### Test Group 1.2: Valid Links with Optional Fields

| Test ID | Scenario | Input | Expected Result | Priority |
|---------|----------|-------|-----------------|----------|
| ED2K-006 | With hashset | `ed2k://\|file\|Movie.avi\|734003200\|ABCD1234ABCD1234ABCD1234ABCD1234\|h=ZYXW9876ZYXW9876ZYXW9876ZYXW9876\|/` | Success: HashSet="ZYXW9876ZYXW9876ZYXW9876ZYXW9876" | 🟠 High |
| ED2K-007 | With single source | `ed2k://\|file\|test.zip\|1048576\|12345678123456781234567812345678\|s=192.168.1.100:4661\|/` | Success: Sources=["192.168.1.100:4661"] | 🟠 High |
| ED2K-008 | With multiple sources | `ed2k://\|file\|file.tar\|2097152\|AABBCCDDAABBCCDDAABBCCDDAABBCCDD\|s=192.168.1.100:4661\|s=server.com:4662\|/` | Success: Sources contains 2 entries | 🟠 High |
| ED2K-009 | With hashset and sources | `ed2k://\|file\|complete.rar\|5242880\|11111111111111111111111111111111\|h=22222222\|s=10.0.0.1:4661\|/` | Success: Both HashSet and Sources populated | 🟡 Medium |

### Test Group 1.3: URL Encoding Scenarios

| Test ID | Scenario | Input | Expected Result | Priority |
|---------|----------|-------|-----------------|----------|
| ED2K-010 | URL-encoded filename (spaces) | `ed2k://\|file\|My%20Video%20File.avi\|1048576\|ABCDABCDABCDABCDABCDABCDABCDABCD\|/` | Success: FileName="My Video File.avi" (decoded) | 🔴 Critical |
| ED2K-011 | URL-encoded special chars | `ed2k://\|file\|File%20%28%202023%20%29.zip\|2048\|12121212121212121212121212121212\|/` | Success: FileName="File ( 2023 ).zip" | 🔴 Critical |
| ED2K-012 | Unicode characters (ü, é, ñ) | `ed2k://\|file\|Übung_café_niño.txt\|512\|FEDCBA98FEDCBA98FEDCBA98FEDCBA98\|/` | Success: Unicode preserved correctly | 🟠 High |
| ED2K-013 | Percent-encoded entire URL | `ed2k%3A%2F%2F%7Cfile%7Ctest.txt%7C1024%7C...` (full encoding) | Success: URL decoded before parsing | 🟡 Medium |

---

## 2. Invalid Ed2k Link Test Cases

### Test Group 2.1: Null and Empty Inputs

| Test ID | Scenario | Input | Expected Error | Priority |
|---------|----------|-------|----------------|----------|
| ED2K-014 | Null URL | `null` | Failure: "URL is null or empty" (Ed2kParseError.NullOrEmpty) | 🔴 Critical |
| ED2K-015 | Empty string | `""` | Failure: "URL is null or empty" | 🔴 Critical |
| ED2K-016 | Whitespace only | `"   "` | Failure: "URL is null or empty" | 🔴 Critical |
| ED2K-017 | Tab and newline | `"\t\n"` | Failure: "URL is null or empty" | 🟠 High |

### Test Group 2.2: Invalid Format

| Test ID | Scenario | Input | Expected Error | Priority |
|---------|----------|-------|----------------|----------|
| ED2K-018 | Missing ed2k:// prefix | `\|file\|test.txt\|1024\|12345678123456781234567812345678\|/` | Failure: "Invalid ed2k link format" (Ed2kParseError.InvalidFormat) | 🔴 Critical |
| ED2K-019 | Wrong protocol (http) | `http://\|file\|test.txt\|1024\|12345678123456781234567812345678\|/` | Failure: "Invalid ed2k link format" | 🔴 Critical |
| ED2K-020 | Missing pipes | `ed2k://file test.txt 1024 12345678123456781234567812345678` | Failure: "Invalid ed2k link format" | 🔴 Critical |
| ED2K-021 | Missing trailing slash | `ed2k://\|file\|test.txt\|1024\|12345678123456781234567812345678\|` | Failure: "Invalid ed2k link format" | 🟠 High |
| ED2K-022 | Extra pipes | `ed2k://\|file\|\|test.txt\|1024\|12345678123456781234567812345678\|/` | Failure: "Invalid ed2k link format" or "Missing required field" | 🟠 High |
| ED2K-023 | Wrong order | `ed2k://\|file\|1024\|test.txt\|12345678123456781234567812345678\|/` | Failure: "Invalid file size" or "Invalid format" | 🟡 Medium |

### Test Group 2.3: Invalid File Names

| Test ID | Scenario | Input | Expected Error | Priority |
|---------|----------|-------|----------------|----------|
| ED2K-024 | Empty filename | `ed2k://\|file\|\|1024\|12345678123456781234567812345678\|/` | Failure: "Missing required field" (Ed2kParseError.MissingRequired) | 🔴 Critical |
| ED2K-025 | Whitespace filename | `ed2k://\|file\|   \|1024\|12345678123456781234567812345678\|/` | Failure: "Filename is empty after decoding" | 🟠 High |
| ED2K-026 | URL-encoded to empty | `ed2k://\|file\|%20%20%20\|1024\|12345678123456781234567812345678\|/` | Failure: "Filename is empty after decoding" | 🟡 Medium |

### Test Group 2.4: Invalid File Sizes

| Test ID | Scenario | Input | Expected Error | Priority |
|---------|----------|-------|----------------|----------|
| ED2K-027 | Zero size | `ed2k://\|file\|test.txt\|0\|12345678123456781234567812345678\|/` | Failure: "File size must be at least 1 byte" (Ed2kParseError.InvalidSize) | 🔴 Critical |
| ED2K-028 | Negative size | `ed2k://\|file\|test.txt\|-1024\|12345678123456781234567812345678\|/` | Failure: "Invalid file size format" | 🔴 Critical |
| ED2K-029 | Non-numeric size | `ed2k://\|file\|test.txt\|abc\|12345678123456781234567812345678\|/` | Failure: "Invalid file size format" | 🔴 Critical |
| ED2K-030 | Decimal size | `ed2k://\|file\|test.txt\|123.45\|12345678123456781234567812345678\|/` | Failure: "Invalid file size format" | 🟠 High |
| ED2K-031 | Size too large (>16 TB) | `ed2k://\|file\|huge.iso\|99999999999999999\|12345678123456781234567812345678\|/` | Failure: "File size exceeds maximum of 16 TB" | 🟡 Medium |
| ED2K-032 | Size with spaces | `ed2k://\|file\|test.txt\|1 024\|12345678123456781234567812345678\|/` | Failure: "Invalid file size format" | 🟡 Medium |

### Test Group 2.5: Invalid File Hashes

| Test ID | Scenario | Input | Expected Error | Priority |
|---------|----------|-------|-----------------|----------|
| ED2K-033 | Missing hash | `ed2k://\|file\|test.txt\|1024\|\|/` | Failure: "Invalid ed2k link format" (Ed2kParseError.InvalidHash) | 🔴 Critical |
| ED2K-034 | Hash too short (31 chars) | `ed2k://\|file\|test.txt\|1024\|1234567812345678123456781234567\|/` | Failure: "Invalid ed2k link format" | 🔴 Critical |
| ED2K-035 | Hash too long (33 chars) | `ed2k://\|file\|test.txt\|1024\|123456781234567812345678123456781\|/` | Failure: "Invalid ed2k link format" | 🔴 Critical |
| ED2K-036 | Non-hex characters (G, Z) | `ed2k://\|file\|test.txt\|1024\|GGZZ5678123456781234567812345678\|/` | Failure: "Invalid ed2k link format" | 🔴 Critical |
| ED2K-037 | Special characters in hash | `ed2k://\|file\|test.txt\|1024\|12345678-1234-5678-1234-567812345678\|/` | Failure: "Invalid ed2k link format" | 🟠 High |
| ED2K-038 | Hash with spaces | `ed2k://\|file\|test.txt\|1024\|12345678 12345678 12345678 12345678\|/` | Failure: "Invalid ed2k link format" | 🟠 High |

---

## 3. Security Test Cases

### Test Group 3.1: Injection Attacks

| Test ID | Scenario | Input | Expected Result | Priority |
|---------|----------|-------|-----------------|----------|
| ED2K-039 | SQL injection in filename | `ed2k://\|file\|test' OR '1'='1.txt\|1024\|12345678123456781234567812345678\|/` | Success: Filename treated as literal string | 🔴 Critical |
| ED2K-040 | XSS in filename | `ed2k://\|file\|<script>alert('xss')</script>.txt\|1024\|12345678123456781234567812345678\|/` | Success: Filename HTML-encoded when displayed | 🔴 Critical |
| ED2K-041 | Path traversal in filename | `ed2k://\|file\|../../etc/passwd\|1024\|12345678123456781234567812345678\|/` | Success: Filename treated as literal | 🟠 High |
| ED2K-042 | Null byte injection | `ed2k://\|file\|file.txt%00.exe\|1024\|12345678123456781234567812345678\|/` | Success or Failure: Handled safely | 🟠 High |
| ED2K-043 | Command injection | `ed2k://\|file\|file.txt; rm -rf /\|1024\|12345678123456781234567812345678\|/` | Success: Treated as literal filename | 🟠 High |

### Test Group 3.2: Malformed and Edge Cases

| Test ID | Scenario | Input | Expected Result | Priority |
|---------|----------|-------|-----------------|----------|
| ED2K-044 | Very long filename (>1000 chars) | `ed2k://\|file\|{1000 'A' characters}.txt\|1024\|12345678123456781234567812345678\|/` | Success or Failure: Handled safely | 🟡 Medium |
| ED2K-045 | Filename with only dots | `ed2k://\|file\|...\|1024\|12345678123456781234567812345678\|/` | Success: FileName="..." | 🟢 Low |
| ED2K-046 | Binary characters in URL | `ed2k://\|file\|test\x00\x01\x02.txt\|1024\|...` | Failure: Encoding error | 🟡 Medium |
| ED2K-047 | Multiple forward slashes | `ed2k://////\|file\|test.txt\|1024\|12345678123456781234567812345678\|/` | Failure: Invalid format | 🟡 Medium |
| ED2K-048 | Case variations in protocol | `ED2K://\|file\|test.txt\|1024\|12345678123456781234567812345678\|/` | Success: Case insensitive protocol | 🟡 Medium |

---

## 4. IsValid() Quick Validation Test Cases

### Test Group 4.1: IsValid() - Should Return True

| Test ID | Scenario | Input | Expected | Priority |
|---------|----------|-------|----------|----------|
| VALID-001 | Standard valid link | `ed2k://\|file\|Ubuntu-20.04.iso\|2877227008\|5E0A6F1D2C3B4A5D6E7F8A9B0C1D2E3F\|/` | true | 🔴 Critical |
| VALID-002 | With optional fields | `ed2k://\|file\|test.zip\|1024\|12345678123456781234567812345678\|h=ABC\|s=1.2.3.4:4661\|/` | true | 🟠 High |
| VALID-003 | Case insensitive | `Ed2K://\|file\|test.txt\|1024\|ABCDABCDABCDABCDABCDABCDABCDABCD\|/` | true | 🟡 Medium |

### Test Group 4.2: IsValid() - Should Return False

| Test ID | Scenario | Input | Expected | Priority |
|---------|----------|-------|----------|----------|
| VALID-004 | Null | `null` | false | 🔴 Critical |
| VALID-005 | Empty string | `""` | false | 🔴 Critical |
| VALID-006 | Wrong protocol | `http://\|file\|test.txt\|1024\|12345678123456781234567812345678\|/` | false | 🔴 Critical |
| VALID-007 | No pipes | `ed2k://file test.txt 1024 hash` | false | 🔴 Critical |
| VALID-008 | Too few pipes (<4) | `ed2k://\|file\|test.txt\|/` | false | 🟠 High |

---

## 5. Error Message Test Cases

### Test Group 5.1: GetErrorMessage() Verification

| Test ID | Ed2kParseError | Expected Message | Contains | Priority |
|---------|----------------|------------------|----------|----------|
| ERR-001 | InvalidFormat | Full message | "Invalid ed2k link format", "ed2k://\|file\|filename\|size\|hash\|/" | 🔴 Critical |
| ERR-002 | InvalidHash | Full message | "32 hexadecimal characters", "MD4" | 🔴 Critical |
| ERR-003 | InvalidSize | Full message | "positive number" | 🔴 Critical |
| ERR-004 | MissingRequired | Full message | "filename", "size", "hash" | 🔴 Critical |
| ERR-005 | NullOrEmpty | Full message | "null or empty" | 🔴 Critical |
| ERR-006 | EncodingError | Full message | "decode", "corrupted", "invalid characters" | 🟠 High |
| ERR-007 | UnexpectedError | Full message | "unexpected error" | 🟡 Medium |

---

## 6. Integration Test Scenarios

### Test Group 6.1: Real-World Ed2k Links

| Test ID | Scenario | Link Type | Expected | Priority |
|---------|----------|-----------|----------|----------|
| INT-001 | Ubuntu ISO (real example) | Large file | Success | 🔴 Critical |
| INT-002 | Movie file with hashset | Video | Success with HashSet | 🟠 High |
| INT-003 | Archive with sources | RAR/ZIP | Success with Sources | 🟠 High |
| INT-004 | Small text file | Document | Success | 🟡 Medium |

### Test Group 6.2: Chained Operations

| Test ID | Scenario | Operation | Expected | Priority |
|---------|----------|-----------|----------|----------|
| INT-005 | IsValid() then Parse() | Valid link | Both succeed | 🔴 Critical |
| INT-006 | Parse() invalid then GetErrorMessage() | Invalid link | Specific error message | 🟠 High |
| INT-007 | Parse 100 valid links | Batch operation | All succeed, no memory leaks | 🟡 Medium |

---

## 7. Performance Test Cases

### Test Group 7.1: Performance Benchmarks

| Test ID | Scenario | Input Count | Expected Time | Priority |
|---------|----------|-------------|---------------|----------|
| PERF-001 | Parse single valid link | 1 | < 10ms | 🟡 Medium |
| PERF-002 | IsValid() single link | 1 | < 1ms (very fast) | 🟡 Medium |
| PERF-003 | Parse 1000 links | 1000 | < 1 second total | 🟢 Low |
| PERF-004 | Parse very long filename (10KB) | 1 | < 50ms | 🟢 Low |

---

## 8. Logging Verification Test Cases

### Test Group 8.1: Log Output Verification

| Test ID | Scenario | Expected Log | Log Level | Priority |
|---------|----------|--------------|-----------|----------|
| LOG-001 | Successful parse | "Ed2k parse successful: {filename} ({size})" | Information | 🟡 Medium |
| LOG-002 | Null URL | "Ed2k parse failed: URL is null or empty" | Warning | 🔴 Critical |
| LOG-003 | Invalid format | "Ed2k parse failed: Invalid format for URL: ..." | Warning | 🔴 Critical |
| LOG-004 | Invalid hash | "Ed2k parse failed: Invalid hash length: {length}" | Warning | 🟠 High |
| LOG-005 | File too large | "Ed2k parse failed: File size too large: {size} bytes" | Warning | 🟡 Medium |
| LOG-006 | URL decoding | "Ed2k URL decoded: ..." | Debug | 🟢 Low |
| LOG-007 | Optional fields | "Ed2k link has hashset: ...", "Ed2k link has source: ..." | Debug | 🟢 Low |

---

## 9. Test Execution Plan

### Phase 1: Basic Validation (Ed2k-001 to ED2K-017)
- [ ] Test all valid link formats
- [ ] Test null/empty inputs
- [ ] Verify Result<T> success/failure states

### Phase 2: Invalid Input Handling (ED2K-018 to ED2K-038)
- [ ] Test all invalid format variations
- [ ] Test boundary conditions
- [ ] Verify error messages match Ed2kParseError enum

### Phase 3: Security Testing (ED2K-039 to ED2K-048)
- [ ] Test injection attempts
- [ ] Test malformed inputs
- [ ] Verify safe handling of malicious inputs

### Phase 4: IsValid() Testing (VALID-001 to VALID-008)
- [ ] Test quick validation method
- [ ] Verify performance (< 1ms)
- [ ] Test edge cases

### Phase 5: Integration & Performance (INT-001 to PERF-004)
- [ ] Test real-world ed2k links
- [ ] Test batch operations
- [ ] Measure performance metrics

### Phase 6: Logging Verification (LOG-001 to LOG-007)
- [ ] Check Serilog output
- [ ] Verify structured logging
- [ ] Test debug/info/warning levels

---

## 10. Test Results Summary

| Test Group | Total Tests | Passed | Failed | Skipped | Pass Rate |
|------------|-------------|--------|--------|---------|-----------|
| Valid Links | 13 | - | - | - | - |
| Invalid Links | 31 | - | - | - | - |
| Security Tests | 10 | - | - | - | - |
| IsValid() Tests | 8 | - | - | - | - |
| Error Messages | 7 | - | - | - | - |
| Integration Tests | 7 | - | - | - | - |
| Performance Tests | 4 | - | - | - | - |
| Logging Tests | 7 | - | - | - | - |
| **TOTAL** | **87** | **-** | **-** | **-** | **-%** |

*Note: Fill in results after executing tests*

---

## 11. Known Edge Cases

### Documented Behavior
1. **Hash Normalization**: All hashes are normalized to uppercase regardless of input case
2. **URL Decoding**: URLs are decoded automatically, including filenames
3. **File Size Limits**: 1 byte minimum, 16 TB maximum (17592186044416 bytes)
4. **Optional Fields**: HashSet and Sources are optional, parser handles their absence gracefully

### Future Enhancements
1. Support for partner hash (p= field) - currently ignored
2. Validation of server IP addresses in sources
3. Support for magnet links (different protocol)

---

## 12. Success Criteria

Sprint 4 is considered complete when:

- ✅ All ed2k parser interfaces and classes implemented
- ✅ Result<T> pattern created and working
- ✅ Regex validation handles all standard ed2k formats
- ✅ URL decoding works correctly
- ✅ Hash validation (32 hex chars) enforced
- ✅ File size validation (1 byte to 16 TB) enforced
- ✅ Optional fields (hashset, sources) parsed correctly
- ✅ Build succeeds with 0 errors
- ✅ This test document created with 87 test cases
- ⬜ Manual testing performed for critical scenarios
- ⬜ Security tests verify injection attacks are handled
- ⬜ Performance tests show < 10ms per parse operation

---

**Document Status:** ✅ Complete
**Next Step:** Execute manual testing and implement unit tests (Sprint 7+)
**Sprint 4 Target:** Prepare for Sprint 5 - Deep Link Service integration
