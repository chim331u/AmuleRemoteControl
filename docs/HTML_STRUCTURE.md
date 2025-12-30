# aMule HTML Structure Documentation

This document describes the HTML structure of aMule web interface pages to aid parser development.

**aMule Version:** 2.3.2 (update when capturing fixtures)
**Last Updated:** Sprint 7 - December 2025

---

## 1. Downloads Page (amuleweb-main-dload.php)

### HTML Structure Overview

```
<html>
  <body>
    <table> [0] - Header/Navigation
    <table> [1] - ???
    <table> [2] - ???
    <table> [3] - ???
    <table> [4] - ???
    <table> [5] - ???
    <table> [6] - *** DOWNLOAD TABLE *** ← Skip(6).Take(1)
      <tr> [0] - Header row (File Name, Size, Completed, etc.)
      <tr> [1] - First download ← Skip(1) to skip header
      <tr> [2] - Second download
      ...
    <table> [7] - ???
    <table> [8] - *** UPLOAD TABLE *** ← Skip(8).Take(1)
      <tr> [0] - ???
      <tr> [1] - ???
      <tr> [2] - First upload ← Skip(2) to skip headers
      ...
  </body>
</html>
```

### Download Table Structure (Table Index 6)

**Header Row:**
| Column | Content | Parser Field |
|--------|---------|--------------|
| 0 | Checkbox `<input>` | FileId (extracted from name attribute) |
| 1 | File Name | FileName |
| 2 | Size (e.g., "699.40 MB") | Size |
| 3 | Completed (e.g., "350.2 MB (50.1%)") | Completed, Progress |
| 4 | Download Speed (e.g., "125.3 kb/s") | DownloadSpeed |
| 5 | Progress Bar (HTML) | (skipped) |
| 6 | Sources (e.g., "5 / 20") | Sources |
| 7 | Status (e.g., "Downloading", "Paused") | Status |
| 8 | Priority (e.g., "Normal", "High") | Priority |

**Example Row HTML:**
```html
<tr>
  <td><input type="checkbox" name="file_12345" /></td>
  <td>Ubuntu-20.04-desktop-amd64.iso</td>
  <td>2.7 GB</td>
  <td>1.35 GB (50.0%)</td>
  <td>512.5 kb/s</td>
  <td><!-- progress bar HTML --></td>
  <td>12 / 45</td>
  <td>Downloading</td>
  <td>Normal</td>
</tr>
```

**Parsing Logic (Current):**
- Skip first 6 tables to reach download table
- Skip header row (row 0)
- Iterate through cells, using switch statement based on count
- Extract FileId from checkbox input name attribute

**Known Issues:**
- Magic number `Skip(6)` - fragile to HTML changes
- Column count hardcoded in switch statement
- No null safety on `FirstOrDefault()` calls

---

## 2. Upload Table (amuleweb-main-dload.php)

### HTML Structure

**Location:** Same page as downloads, Table Index 8

**Header Rows:** Skip first 2 rows

**Columns:**
| Column | Content | Parser Field |
|--------|---------|--------------|
| 1 | File Name | FileName |
| 2 | User Name | UserName |
| 3 | Uploaded | Up |
| 4 | Downloaded | Down |
| 5 | (skipped) | - |
| 6 | (skipped) | - |
| 7 | Speed | Speed |

**Parsing Logic (Current):**
- Skip first 8 tables to reach upload table
- Skip first 2 rows (headers)
- Switch statement with cases 1-7
- Empty cells (`&nbsp;`) increment counter but don't store data

---

## 3. Servers Page (amuleweb-main-servers.php)

### HTML Structure

**Location:** Table Index 1 ← Skip(1)

**Header Rows:** Skip first 3 rows

**Columns:**
| Column | Content | Parser Field |
|--------|---------|--------------|
| 0 | Connection link `<a href>` | ServerId, Port (extracted from URL) |
| 1 | Server Name | ServerName |
| 2 | Description | Description |
| 3 | Address (IP:Port) | Address |
| 4 | Users | Users |
| 5 | Files | Files |

**Example Link HTML:**
```html
<a href="amuleweb-main-servers.php?cmd=connect&ip=516650843&port=4321">Connect</a>
```

**Parsing Logic (Current):**
- Extract `ip=` and `port=` parameters from href attribute
- Parse remaining columns sequentially
- Reset on empty cell

---

## 4. Stats Page (stats.php)

### HTML Structure

**Location:** All `<table>` elements (no Skip)

**Content:** Not structured as data table - uses `<td>` cells with text content

**Example:**
```html
<table>
  <tr>
    <td>Ed2k : Connected</td>
  </tr>
  <tr>
    <td>Kad : Firewalled</td>
  </tr>
</table>
```

**Parsing Logic (Current):**
- Search all cells for text containing "Ed2k" or "Kad"
- Extract status text after colon
- Simple string replacement and trim

---

## 5. Search Results Page (amuleweb-main-search.php)

### HTML Structure

**Location:** All `<tr>` elements (direct XPath: `//tr`)

**No table index** - searches all table rows across entire page

**Columns:**
| Column | Content | Parser Field |
|--------|---------|--------------|
| 0 | Checkbox `<input>` | SearchId (from name attribute) |
| 1 | File Name | FileName |
| 2 | File Size | FileSize |
| 3 | Sources | Sources |

**Parsing Logic (Current):**
- Iterate through all `<tr>` elements
- State machine with count variable (0-3)
- Checkbox detection increments count
- Resets on empty cell

**Known Issues:**
- No table filtering - may pick up wrong tables
- Fragile state machine logic

---

## 6. Preferences Page (amuleweb-main-prefs.php)

### HTML Structure

**Location:** `<script>` tags containing JavaScript

**XPath:** `//script`

**Content Format:**
```javascript
initvals["max_line_up_cap"] = "100";
initvals["max_line_down_cap"] = "300";
initvals["tcp_port"] = "4662";
...
```

**Parsing Logic (Current):**
- Find script tag containing "initvals"
- Extract substring between `initvals[` and `<!--`
- Split by semicolon
- 200+ line switch statement matching preference keys
- String manipulation to extract key and value

**Known Issues:**
- **Massive switch statement** (200+ lines, unmaintainable)
- Hardcoded string indices
- No error handling for malformed JavaScript

**Sprint 9 Refactor Plan:**
- Create `PreferenceMapping.json` with key mappings
- Dictionary lookup instead of switch statement
- Reduce from 200 lines to ~50 lines

---

## 7. Log Page (log.php)

### HTML Structure

**Location:** `<pre>` tag

**XPath:** `//pre`

**Content:** Preformatted text (plain log output)

**Parsing Logic (Current):**
- Select first `<pre>` element
- Extract `InnerText`
- No additional parsing

---

## 8. Footer (footer.php)

### HTML Structure

**Expected Content:** Version string like "aMule 2.3.2"

**XPath:** TBD (Sprint 10)

**Parsing Logic (Planned - Sprint 10):**
- Regex pattern: `aMule\s+([\d\.]+)`
- Extract version number
- Use for XPath configuration selection

---

## Version Differences

### aMule 2.3.2 vs 2.3.3

**TODO:** Document differences when fixtures from both versions are captured

| Feature | 2.3.2 | 2.3.3 | Notes |
|---------|-------|-------|-------|
| Download Table Index | 6 | ? | Verify if same |
| Upload Table Index | 8 | ? | Verify if same |
| ... | ... | ... | ... |

---

## Parser Refactoring Notes

### Current Problems

1. **Magic Numbers:** Skip(6), Skip(8), Skip(1) - no explanation
2. **Hardcoded Indices:** Switch statements with numbered cases
3. **No Null Safety:** `FirstOrDefault()` without null checks
4. **State Machine Fragility:** Count variables, complex reset logic
5. **Massive Switch Statements:** PreferencesParser is 200+ lines

### Refactoring Strategy (Sprint 7-10)

1. **Sprint 7:** Extract to XPathConfiguration
   - Replace `Skip(6)` with `xpathConfig.DownloadTableIndex`
   - Document meaning of each index

2. **Sprint 8-9:** Implement dedicated parsers
   - One parser per page type
   - Inject XPathConfiguration
   - Named constants for column indices
   - Null-safe operators

3. **Sprint 9:** Refactor PreferencesParser
   - JSON-driven mapping
   - Dictionary lookup
   - Reduce complexity

4. **Sprint 10:** Version detection
   - Parse footer for aMule version
   - Load version-specific XPath configuration
   - Graceful fallback to default

---

## Testing Strategy

### Fixture Requirements

For each page type, need fixtures for:
- **Empty state** (no data)
- **Single item** (one download/upload/server)
- **Multiple items** (typical use case)
- **Malformed data** (missing columns, corrupt HTML)
- **Edge cases** (special characters, very large numbers)

### Test Coverage Goals

- 80%+ code coverage for all parsers
- All edge cases covered
- Regression tests for known bugs

---

## Maintenance

**When to update this document:**
- After capturing HTML fixtures (fill in actual structure)
- When aMule version changes (document differences)
- When discovering new edge cases
- After parser refactoring (update "Current" sections to "Old")

**Template sections marked with TODO:**
- [ ] Fill in actual table indices after capturing fixtures
- [ ] Document exact HTML structure with real examples
- [ ] Add screenshots of browser DevTools showing table structure
- [ ] Document version differences if testing multiple aMule versions

---

**Notes for Sprint 7 Task 3.3:**

To complete this document:
1. Capture downloads.html fixture
2. Open in browser and inspect with DevTools
3. Count tables from top to confirm index 6 is download table
4. Document exact column structure
5. Repeat for all other page types
6. Add real HTML examples to each section
