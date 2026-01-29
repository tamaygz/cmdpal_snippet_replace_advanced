# Contributing to cmdpal_snippet_replace_advanced

Thank you for your interest in contributing! This guide will help you get started.

## Quick Links

- [Code of Conduct](#code-of-conduct)
- [Development Setup](#development-setup)
- [Architecture](#architecture)
- [Making Changes](#making-changes)
- [Testing](#testing)
- [Pull Request Process](#pull-request-process)

## Code of Conduct

This project follows the Microsoft Open Source Code of Conduct. Please be respectful and constructive.

## Development Setup

### Prerequisites

- **Windows 10/11** (build 19041 or later)
- **Visual Studio 2022** (17.8 or later) with:
  - .NET desktop development workload
  - Windows App SDK
- **.NET 9.0 SDK** or later
- **PowerToys** (version with Command Palette support)

### Clone and Build

```bash
git clone https://github.com/tamaygz/cmdpal_snippet_replace_advanced.git
cd cmdpal_snippet_replace_advanced
```

```powershell
# Restore dependencies
dotnet restore

# Build
dotnet build

# Build release
dotnet build -c Release

# Create MSIX package (requires Windows SDK)
# Use Visual Studio: Project → Publish → Create App Packages
```

### Project Structure

```
cmdpal_snippet_replace_advanced/
├── Models/                          # Data models
│   ├── Snippet.cs                   # Snippet entity
│   ├── SnippetCollection.cs        # Collection entity
│   └── SnippetData.cs              # Root data structure
├── Services/                        # Business logic
│   ├── SnippetStorageService.cs    # JSON persistence
│   ├── VariableExpansionService.cs # Variable parsing
│   ├── AutoExpansionService.cs     # Auto-expansion engine
│   ├── ConfigurationService.cs     # Settings management
│   ├── WindowsIntegrationService.cs # Windows helpers
│   └── Windows/                     # Windows API layer
│       ├── NativeMethods.cs        # P/Invoke declarations
│       ├── KeyboardHookManager.cs  # Keyboard hook
│       ├── InputSimulator.cs       # SendInput wrapper
│       └── ClipboardHelper.cs      # Clipboard operations
├── Pages/                           # UI pages
│   └── Snippets/                   # Command Palette pages
│       ├── SnippetsMainPage.cs     # Main navigation
│       ├── SnippetSearchPage.cs    # Search UI
│       ├── AutoExpansionSettingsPage.cs # Settings UI
│       └── ...
├── Assets/                          # Icons and resources
├── Program.cs                       # Entry point
├── cmdpal_snippet_replace_advanced.cs # Extension class
└── cmdpal_snippet_replace_advancedCommandsProvider.cs # Provider

Documentation:
├── README.md                        # User documentation
├── ARCHITECTURE.md                  # System design
├── TROUBLESHOOTING.md              # Common issues
└── CONTRIBUTING.md                 # This file
```

## Architecture

Please read [ARCHITECTURE.md](ARCHITECTURE.md) for a detailed overview of the system design.

### Key Concepts

1. **Extension Model**: Out-of-process COM server activated by PowerToys
2. **Storage**: JSON files in `%LOCALAPPDATA%\PowerToys\cmdpal_snippets\`
3. **Keyboard Hook**: Low-level Windows hook (WH_KEYBOARD_LL)
4. **Input Simulation**: Windows SendInput API for text insertion
5. **Threading**: Hook on background thread, UI on UI thread

## Making Changes

### Before You Start

1. **Check existing issues**: Someone might already be working on it
2. **Discuss large changes**: Open an issue first for big features
3. **Read the architecture**: Understand the design before coding

### Coding Guidelines

#### C# Style

- **Follow Microsoft conventions**: Use PascalCase, camelCase appropriately
- **Use nullable reference types**: Enable `<Nullable>enable</Nullable>`
- **Async/await**: Use async for I/O operations
- **XML docs**: Document public APIs
- **Copyright headers**: Include Microsoft copyright notice

Example:
```csharp
// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;

namespace cmdpal_snippet_replace_advanced.Services;

/// <summary>
/// Manages snippet operations
/// </summary>
public sealed class SnippetService
{
    /// <summary>
    /// Load all snippets asynchronously
    /// </summary>
    /// <returns>Array of snippets</returns>
    public async Task<Snippet[]> LoadSnippetsAsync()
    {
        // Implementation
    }
}
```

#### Performance Guidelines

- **Keyboard hook**: Keep callback < 1ms (critical for responsiveness)
- **Async I/O**: Use async for file operations
- **Caching**: Cache frequently accessed data
- **Lazy loading**: Don't load everything at startup
- **Dispose pattern**: Implement IDisposable for unmanaged resources

#### Security Guidelines

- **No network**: Don't send data over network without explicit user consent
- **Secure storage**: Don't store sensitive data in plain text
- **Input validation**: Validate all user input
- **Permissions**: Only request necessary permissions
- **Secure fields**: Respect secure field detection

### Adding a New Feature

#### 1. Create an Issue

Describe:
- **What**: What feature are you adding?
- **Why**: Why is it useful?
- **How**: How will it work?

#### 2. Branch Naming

```bash
git checkout -b feature/short-description
git checkout -b fix/issue-number-short-description
```

#### 3. Implementation Checklist

- [ ] Core logic implemented
- [ ] XML documentation added
- [ ] Error handling added
- [ ] Performance considered
- [ ] Security reviewed
- [ ] Manual testing completed
- [ ] README updated (if user-facing)
- [ ] ARCHITECTURE.md updated (if architectural change)

#### 4. Example: Adding a New Variable

```csharp
// 1. Add to VariableExpansionService.cs
private async Task<string> ExpandVariableAsync(string variable)
{
    return variable.ToLowerInvariant() switch
    {
        // ... existing variables ...
        "myfeature" => await GetMyFeatureValueAsync(),
        _ => $"{{{variable}}}"
    };
}

// 2. Add helper method
private async Task<string> GetMyFeatureValueAsync()
{
    // Implementation
}

// 3. Add to preview
private static string GetVariablePreview(string variable)
{
    return variable.ToLowerInvariant() switch
    {
        // ... existing variables ...
        "myfeature" => "<feature value>",
        _ => $"{{{variable}}}"
    };
}

// 4. Update README.md
- `{myfeature}` → Description of what it does
```

### Common Tasks

#### Adding a New Page

1. Create class inheriting `ListPage` or `FormPage`
2. Implement `GetItems()` or form fields
3. Add to `CommandsProvider` in `TopLevelCommands()`
4. Test navigation and functionality

#### Adding a New Service

1. Create service class in `Services/`
2. Document public methods with XML comments
3. Consider thread safety (use locks if needed)
4. Add disposal if needed (implement IDisposable)
5. Register in `CommandsProvider` constructor

#### Modifying Windows Integration

**⚠️ Warning**: Windows APIs are fragile. Test thoroughly!

1. Declare P/Invoke in `NativeMethods.cs`
2. Add wrapper in appropriate service
3. Handle errors gracefully
4. Test on multiple Windows versions
5. Document any limitations

## Testing

### Manual Testing

Required for all changes:

1. **Build and install**: Create MSIX and install
2. **PowerToys integration**: Verify extension loads
3. **Basic functionality**: Test all affected features
4. **Edge cases**: Try invalid input, empty data, etc.
5. **Performance**: Check for lag or slowness

### Testing Checklist

For auto-expansion changes:
- [ ] Test in Notepad
- [ ] Test in VS Code
- [ ] Test in Chrome
- [ ] Test in Word/Excel
- [ ] Test with modifiers (Ctrl, Shift, Alt)
- [ ] Test in password field
- [ ] Test special characters
- [ ] Test Unicode (emoji, symbols)
- [ ] Test performance (no typing lag)

For UI changes:
- [ ] Test keyboard navigation
- [ ] Test search/filter
- [ ] Test with empty data
- [ ] Test with large data (100+ snippets)
- [ ] Test error states

### Test Environment

- **Windows 10**: Build 19041 or later
- **Windows 11**: Latest build
- **PowerToys**: Latest stable version

### Debugging Tips

```csharp
// Use Debug.WriteLine for logging
System.Diagnostics.Debug.WriteLine($"Expanded: {keyword} → {text}");

// Attach debugger to PowerToys process
// In Visual Studio: Debug → Attach to Process → PowerToys.exe

// Use breakpoints in hook (but be careful - can freeze system!)
// Consider logging instead of breakpoints in keyboard hook
```

## Pull Request Process

### Before Submitting

1. **Test thoroughly**: Follow testing checklist
2. **Update docs**: README, ARCHITECTURE, etc.
3. **Clean commits**: Squash WIP commits, write good messages
4. **No secrets**: Remove any API keys, passwords, personal data
5. **Copyright headers**: Ensure all new files have proper headers

### PR Description Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
- [ ] Tested on Windows 10
- [ ] Tested on Windows 11
- [ ] Tested auto-expansion
- [ ] Tested UI changes
- [ ] No performance regression

## Checklist
- [ ] Code follows style guidelines
- [ ] XML documentation added
- [ ] README updated (if needed)
- [ ] No compiler warnings
- [ ] Tested thoroughly

## Screenshots (if UI change)
Before/after screenshots

## Related Issues
Fixes #123
```

### Review Process

1. **Automated checks**: Build must pass
2. **Code review**: Maintainer will review code
3. **Testing**: Maintainer may test on their machine
4. **Feedback**: Address review comments
5. **Merge**: Once approved, will be merged

### After Merge

- Your changes will be included in next release
- You'll be credited in release notes
- Thank you for contributing! 🎉

## Areas for Contribution

Good places to start:

### Easy Issues
- Documentation improvements
- UI polish
- Default snippet additions
- Bug fixes in existing features

### Medium Issues
- New variable types
- New UI pages
- Performance improvements
- Test coverage

### Hard Issues
- Hotkey support
- Cloud sync
- Rich text support
- Plugin system

### PRD Features Not Yet Implemented
- Global hotkeys per snippet/collection
- Advanced regex with capture groups
- PowerShell script snippets
- AI-powered snippet suggestions
- Voice command integration
- Team collaboration features

## Getting Help

- **Discord/Chat**: Not set up yet
- **Issues**: Use GitHub issues for questions
- **Email**: Check package manifest for contact

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

## Recognition

All contributors will be:
- Listed in release notes
- Credited in documentation
- Added to contributors list

Thank you for making this project better!
