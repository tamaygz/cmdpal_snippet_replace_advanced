# Troubleshooting Guide

## Common Issues and Solutions

### Auto-Expansion Not Working

#### Problem: Auto-expansion doesn't trigger
**Symptoms**: Typing snippet keywords doesn't expand them

**Solutions**:
1. **Check if enabled**: Open Command Palette → "Auto-Expansion Settings" → Verify status is "ENABLED"
2. **Check collection settings**: Ensure the snippet's collection has "AutoExpandEnabled = true"
3. **Restart the service**: Disable → Enable auto-expansion
4. **Check permissions**: Windows may require elevation for keyboard hooks

#### Problem: Expansion is too slow or too fast
**Symptoms**: Snippets expand before you finish typing, or take too long to expand

**Solutions**:
1. **Adjust trigger delay**: Default is 300ms
   - Too fast? Increase to 500-1000ms
   - Too slow? Decrease to 200-250ms
2. **Edit config**: `%LOCALAPPDATA%\PowerToys\cmdpal_snippets\config.json`
   ```json
   {
     "autoExpansion": {
       "delayMs": 500
     }
   }
   ```

#### Problem: Keyboard input is laggy when auto-expansion is enabled
**Symptoms**: Noticeable delay when typing

**Solutions**:
1. **Check system resources**: High CPU usage can affect hook performance
2. **Reduce active snippets**: Disable collections you don't need
3. **Simplify regex patterns**: Complex regex can slow matching
4. **File an issue**: This shouldn't happen - might be a bug

### Snippets Not Saving

#### Problem: Changes to snippets don't persist
**Symptoms**: Edit snippet, close app, changes are gone

**Solutions**:
1. **Check file permissions**: Verify write access to `%LOCALAPPDATA%\PowerToys\cmdpal_snippets\`
2. **Check disk space**: Ensure sufficient disk space
3. **Corrupted file**: Delete `snippets.json` (will reset to defaults)
4. **Export first**: Use Import/Export to backup before troubleshooting

#### Problem: Import fails
**Symptoms**: Error when importing JSON file

**Solutions**:
1. **Validate JSON**: Use a JSON validator (jsonlint.com)
2. **Check format**: Ensure file matches expected structure:
   ```json
   {
     "version": 1,
     "collections": [],
     "snippets": []
   }
   ```
3. **Try merge instead of replace**: Less destructive if there's a format issue

### Variable Expansion Issues

#### Problem: {clipboard} doesn't work
**Symptoms**: Variable shows "{clipboard}" instead of clipboard content

**Solutions**:
1. **Check clipboard**: Ensure something is actually copied
2. **Clipboard access**: Windows may block clipboard access
3. **Restart app**: Clipboard API sometimes needs reset

#### Problem: {date+7d} shows wrong date
**Symptoms**: Date math calculation is incorrect

**Solutions**:
1. **Check format**: Must be exactly `{date+7d}` or `{date-1m}`
2. **Valid units**: d (days), w (weeks), m (months), y (years)
3. **Case insensitive**: `{DATE+7D}` works too

#### Problem: {input:prompt} doesn't prompt
**Symptoms**: Shows "[prompt]" instead of asking for input

**Solutions**:
1. **Manual expansion only**: Input prompts only work in Command Palette expansion
2. **Auto-expansion limitation**: Can't show dialogs during auto-expansion
3. **Use static text**: For auto-expansion, avoid input variables

### Performance Issues

#### Problem: High CPU usage
**Symptoms**: Extension uses significant CPU

**Solutions**:
1. **Check snippet count**: 1000+ snippets can slow matching
2. **Regex patterns**: Complex regex is CPU-intensive
3. **Disable when not needed**: Turn off auto-expansion when not actively using
4. **Reduce buffer size**: Edit config (not recommended unless necessary)

#### Problem: Memory usage growing
**Symptoms**: Extension memory increases over time

**Solutions**:
1. **Restart PowerToys**: Clears all caches
2. **Check for leaks**: File an issue with reproduction steps
3. **Limit collection count**: Each collection adds overhead

### Windows Integration Issues

#### Problem: Can't install keyboard hook
**Symptoms**: Error message "Failed to install keyboard hook"

**Solutions**:
1. **Run as Administrator**: Right-click PowerToys → Run as Administrator
2. **Antivirus blocking**: Add PowerToys to antivirus whitelist
3. **Another hook active**: Other apps with keyboard hooks may conflict
   - Gaming software (Razer, Corsair)
   - Macro tools (AutoHotkey, AHK)
   - Security software
4. **Windows version**: Requires Windows 10/11

#### Problem: SendInput not working
**Symptoms**: Snippets don't insert text

**Solutions**:
1. **Application blocking**: Some apps block SendInput (games, secure apps)
2. **Admin rights**: App running as admin may block normal app's input
3. **Try clipboard method**: Temporary workaround (not implemented yet)

#### Problem: Secure field detection not working
**Symptoms**: Snippets expand in password fields

**Solutions**:
1. **Disable secure field expansion**: Settings → "Secure Fields: Disabled"
2. **App-specific issue**: Some apps use custom password fields
3. **Add to exclusions**: Future feature will allow app-specific exclusions

## Debugging

### Enable Debug Logging

1. **Check Debug Output**:
   - Attach Visual Studio debugger
   - View Debug output window
   - Look for messages from extension

2. **Common Debug Messages**:
   ```
   "Auto-expansion service started with keyboard hook"
   "Expanded snippet: keyword → 123 chars"
   "Skipping expansion in secure field"
   "Failed to install keyboard hook: [error]"
   ```

### Diagnostic Commands

#### Check Data Files
```powershell
# List all data files
dir $env:LOCALAPPDATA\PowerToys\cmdpal_snippets

# View snippets.json
notepad $env:LOCALAPPDATA\PowerToys\cmdpal_snippets\snippets.json

# View config.json
notepad $env:LOCALAPPDATA\PowerToys\cmdpal_snippets\config.json
```

#### Reset to Defaults
```powershell
# Backup first!
Copy-Item $env:LOCALAPPDATA\PowerToys\cmdpal_snippets\snippets.json $env:USERPROFILE\Desktop\snippets-backup.json

# Delete to reset (will recreate with defaults on next launch)
Remove-Item $env:LOCALAPPDATA\PowerToys\cmdpal_snippets\snippets.json
Remove-Item $env:LOCALAPPDATA\PowerToys\cmdpal_snippets\config.json
```

### Known Limitations

1. **Input Prompts in Auto-Expansion**: Not supported (would block hook thread)
2. **Admin App Integration**: Cannot send input to apps running as administrator
3. **Game Compatibility**: Some games block keyboard hooks
4. **Remote Desktop**: Keyboard hooks don't work over RDP
5. **UWP Apps**: Some UWP apps have security restrictions

## Getting Help

### Before Filing an Issue

1. **Check this guide**: Most issues are covered here
2. **Update PowerToys**: Ensure you have the latest version
3. **Restart PowerToys**: Many issues resolve with restart
4. **Export snippets**: Backup your data before troubleshooting

### Filing a Bug Report

Include:
1. **Windows version**: `winver` output
2. **PowerToys version**: Help → About
3. **Extension version**: From package manifest
4. **Steps to reproduce**: Detailed steps
5. **Expected vs actual**: What should happen vs what does happen
6. **Debug logs**: If available
7. **Config files**: (remove sensitive data first!)

### Feature Requests

Check roadmap in README.md first. If your feature isn't listed:
1. **Describe use case**: Why do you need this?
2. **Provide examples**: Show how it would work
3. **Similar tools**: How do other tools handle this?

## Performance Benchmarks

Expected performance (on modern hardware):

- **Snippet expansion**: < 50ms (imperceptible)
- **Keyboard hook latency**: < 1ms (no typing lag)
- **Snippet matching**: < 10ms (for 100 snippets)
- **File load**: < 100ms (1000 snippets)
- **Memory usage**: < 50MB baseline

If you're seeing worse performance, something is wrong.

## Tips for Power Users

### Optimize Performance

1. **Organize collections**: Keep related snippets together
2. **Disable unused collections**: Reduce active snippet count
3. **Use specific keywords**: Shorter keywords = faster matching
4. **Avoid complex regex**: Use simple string matches when possible

### Best Practices

1. **Backup regularly**: Use Import/Export weekly
2. **Test new snippets**: Try manually before enabling auto-expansion
3. **Use descriptive titles**: Makes searching easier
4. **Tag consistently**: Use consistent tag names
5. **Monitor statistics**: Review unused snippets periodically

### Advanced Usage

1. **Keyboard shortcuts**: Learn Command Palette shortcuts
2. **Variable combinations**: Combine multiple variables
3. **Template snippets**: Use for complex document structures
4. **Collection prefixes**: Organize by context (work/, personal/)

## Emergency Recovery

### Lost All Snippets

1. **Check backup**: `%USERPROFILE%\Documents\cmdpal_snippets_export_*.json`
2. **Check recycle bin**: May have accidentally deleted
3. **System restore**: Windows may have restore point
4. **OneDrive/backup**: If you use cloud storage

### Extension Won't Load

1. **Reinstall extension**: Uninstall → reinstall MSIX
2. **Check PowerToys**: Update to latest version
3. **Check Windows**: Ensure .NET 9 runtime installed
4. **Event Viewer**: Check Windows Logs → Application

### Can't Stop Auto-Expansion

If auto-expansion is stuck and won't disable:

```powershell
# Force stop PowerToys
Stop-Process -Name PowerToys -Force

# Disable in config
$config = Get-Content "$env:LOCALAPPDATA\PowerToys\cmdpal_snippets\config.json" | ConvertFrom-Json
$config.autoExpansion.enabled = $false
$config | ConvertTo-Json | Set-Content "$env:LOCALAPPDATA\PowerToys\cmdpal_snippets\config.json"

# Restart PowerToys
Start-Process "C:\Program Files\PowerToys\PowerToys.exe"
```
