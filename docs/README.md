# aMule HTML Test Fixtures

This directory contains captured HTML responses from aMule web interface for testing parser implementations.

## Purpose

These HTML fixtures enable:
- **Unit testing** parsers without a running aMule instance
- **Regression testing** to catch breaking changes when aMule updates
- **Reproducible tests** across different development environments
- **Edge case testing** (empty lists, malformed data, special characters)

## Capturing HTML from aMule Web Interface

### Prerequisites
1. Running aMule instance with web interface enabled
2. Web interface accessible (typically http://localhost:4711)
3. Valid authentication credentials

### Capture Instructions

**Method 1: Browser Developer Tools (Recommended)**

1. Open aMule web interface in Chrome/Firefox
2. Open Developer Tools (F12)
3. Navigate to Network tab
4. Navigate to the page you want to capture (e.g., Downloads)
5. Find the request in Network tab (e.g., `amuleweb-main-dload.php`)
6. Right-click → Copy → Copy Response
7. Save to file with appropriate name

**Method 2: cURL Command Line**

```bash
# Replace PASSWORD with your aMule web password
# Downloads page
curl "http://localhost:4711/amuleweb-main-dload.php?pass=PASSWORD" -o downloads.html

# Servers page
curl "http://localhost:4711/amuleweb-main-servers.php?pass=PASSWORD" -o servers.html

# Stats page
curl "http://localhost:4711/stats.php?pass=PASSWORD" -o stats.html

# Search page (after performing a search)
curl "http://localhost:4711/amuleweb-main-search.php?pass=PASSWORD" -o search.html

# Preferences page
curl "http://localhost:4711/amuleweb-main-prefs.php?pass=PASSWORD" -o preferences.html

# Footer (contains version info)
curl "http://localhost:4711/footer.php?pass=PASSWORD" -o footer.html
```

**Method 3: Programmatic Capture (from app)**

```csharp
// In aMuleRemoteService, temporarily add:
var html = await _networkHelperServices.SendRequest("amuleweb-main-dload.php");
File.WriteAllText("/path/to/TestFixtures/AmuleHtml/downloads.html", html);
```

## Required Fixtures

### Sprint 7 (Task 3.3) - Baseline Fixtures

Create these files by capturing from your running aMule instance:

#### Downloads Page Variations
- [ ] `downloads-empty.html` - No active downloads
- [ ] `downloads-single.html` - Exactly one download
- [ ] `downloads-multiple.html` - 5-10 typical downloads
- [ ] `downloads-100items.html` - Large list (performance testing)
- [ ] `downloads-paused.html` - Mix of paused and active downloads
- [ ] `downloads-completed.html` - Completed downloads (100% progress)
- [ ] `downloads-malformed.html` - Edge case: missing columns or corrupt data (manually edit)

#### Uploads Page Variations
- [ ] `uploads-empty.html` - No active uploads
- [ ] `uploads-single.html` - One upload
- [ ] `uploads-multiple.html` - Multiple uploads

#### Servers Page Variations
- [ ] `servers-empty.html` - No servers in list
- [ ] `servers-single.html` - One server
- [ ] `servers-multiple.html` - Multiple servers
- [ ] `servers-connected.html` - With active connection

#### Other Pages
- [ ] `stats.html` - Statistics page (Ed2k/Kad status)
- [ ] `search-empty.html` - Search with no results
- [ ] `search-results.html` - Search with 10+ results
- [ ] `preferences.html` - Full preferences page
- [ ] `footer.html` - Footer containing version info (for version detection)
- [ ] `log.html` - aMule log page

### Sprint 8-9 - Parser-Specific Fixtures

Additional edge cases discovered during parser implementation.

## Fixture Naming Convention

Format: `{page}-{variation}.html`

**Examples:**
- `downloads-empty.html` - Downloads page with zero downloads
- `downloads-paused.html` - Downloads page with paused items
- `servers-connected.html` - Servers page with active connection

## Security Note

⚠️ **Before committing fixtures:**
1. **Remove passwords** from any URL parameters
2. **Sanitize file names** - remove personal information
3. **Review content** for sensitive data (IP addresses, server names)
4. Use placeholder values like `192.168.1.100` for IP addresses

## HTML Structure Documentation

After capturing fixtures, document the HTML structure in `HTML_STRUCTURE.md`:
- Table structure (which table index contains what data)
- Column ordering within tables
- Special cases and edge conditions
- Version differences (if testing multiple aMule versions)

## Embedded Resources (Future)

In future sprints, these fixtures may be added as embedded resources in the .csproj file:

```xml
<ItemGroup>
  <EmbeddedResource Include="TestFixtures\AmuleHtml\*.html" />
</ItemGroup>
```

This allows test projects to access fixtures without file system dependencies.

## Validation Checklist

Before considering fixtures complete:
- [ ] All required baseline fixtures captured
- [ ] Files saved with correct encoding (UTF-8)
- [ ] Sensitive data removed/sanitized
- [ ] HTML structure documented in HTML_STRUCTURE.md
- [ ] At least one fixture per page type
- [ ] At least one edge case fixture per parser (empty, malformed)

## Usage in Tests (Sprint 8+)

```csharp
// Example from DownloadParserTests.cs
[Fact]
public void DownloadParser_WithEmptyList_ReturnsEmptyCollection()
{
    // Arrange
    var html = File.ReadAllText("TestFixtures/AmuleHtml/downloads-empty.html");
    var parser = new DownloadParser(new XPathConfiguration());

    // Act
    var result = parser.Parse(html);

    // Assert
    result.Should().NotBeNull();
    result.Should().BeEmpty();
}
```

## Maintenance

- **Update fixtures** when aMule version changes
- **Add new variations** when bugs are discovered
- **Document differences** between aMule versions
- **Keep fixtures minimal** - only include necessary HTML structure

---

**Last Updated:** Sprint 7 - December 2025
**aMule Version:** 2.3.2 (update when fixtures are captured)
