# Snippet Replace Advanced for PowerToys Command Palette

A powerful snippet management extension for PowerToys Command Palette that brings Alfred-style snippet functionality to Windows. Create, organize, and expand text snippets with dynamic variables and keyboard triggers.

## Features

### Core Snippet Management
- **Create & Organize**: Create text snippets with keywords/aliases for quick expansion
- **Collections**: Group snippets into collections (dictionaries) with custom prefixes and suffixes
- **Search & Browse**: Find snippets across all collections with powerful search
- **Usage Tracking**: Monitor snippet usage statistics and most-used snippets

### Auto-Expansion ⚡
- **System-Wide**: Automatically expands snippets as you type in ANY Windows application
- **Keyboard Hook**: Low-level Windows keyboard hook captures keystrokes
- **Smart Triggers**: Configurable delay (default 300ms) to prevent accidental expansions
- **Secure Fields**: Optional (disabled by default) - skip password fields
- **Per-Collection**: Enable/disable auto-expansion for each collection independently
- **Real-Time**: Instant replacement using Windows SendInput API

### Dynamic Variables
Support for powerful variable expansion in snippets:
- **Date/Time**: `{date}`, `{time}`, `{datetime}`, `{year}`, `{month}`, `{day}`
- **Date Math**: `{date+7d}` (7 days from now), `{date-1m}` (1 month ago)
- **System**: `{username}`, `{computername}`, `{userdomain}`
- **Clipboard**: `{clipboard}` - paste clipboard content
- **Selection**: `{selection}` - insert selected text
- **Environment**: `{env:VAR}` - expand environment variables
- **Input Prompts**: `{input:prompt}` - prompt user for input when expanding
- **Timestamp**: `{timestamp}` - Unix timestamp

### Smart Organization
- **Tags**: Categorize snippets with tags for better organization
- **Collections**: Organize snippets into themed collections (Personal, Work, etc.)
- **Prefixes/Suffixes**: Auto-apply formatting to all snippets in a collection
- **Statistics**: Track usage count and last used date for each snippet

### Import/Export
- **Backup**: Export all snippets to JSON for safekeeping
- **Share**: Share snippet collections with others
- **Restore**: Import snippets from JSON files
- **Merge**: Combine imported snippets with existing ones

## Installation

1. Install [PowerToys](https://github.com/microsoft/PowerToys) (version with Command Palette support)
2. Build this extension or download the release package
3. Install the MSIX package
4. Open PowerToys Command Palette (Ctrl+K by default)
5. Type "Snippets" to access the extension
6. **Enable Auto-Expansion**: Go to "Auto-Expansion Settings" and enable the feature

**Note:** Auto-expansion requires Windows accessibility permissions to monitor keystrokes system-wide.

## Usage

### Quick Start - Manual Expansion

1. **Access Snippets**: Open Command Palette (Ctrl+K) and type "Snippets"
2. **Browse Collections**: Select a collection to see its snippets
3. **Expand Snippet**: Click on a snippet to expand it and copy to clipboard
4. **Search**: Use "Search Snippets" to find snippets across all collections

### Auto-Expansion (Recommended)

1. **Enable**: Open Command Palette → "Auto-Expansion Settings" → "Enable Auto-Expansion"
2. **Type Naturally**: Just type your snippet keywords in any application
3. **Automatic Replacement**: After 300ms, the keyword is replaced with the expanded text
4. **Works Everywhere**: VS Code, browsers, Notepad, Word, etc.

**Example:**
- Type `myemail` → Automatically replaced with `user@example.com`
- Type `today` → Automatically replaced with `2026-01-29`
- Type `greet` → Prompts for name, then expands to greeting

### Configuration

Access "Auto-Expansion Settings" to configure:
- **Enable/Disable**: Turn auto-expansion on/off
- **Trigger Delay**: Adjust timing (200-1000ms recommended)
- **Secure Fields**: Enable/disable in password fields (disabled by default)
- **Active Snippets**: View which snippets are available for auto-expansion

### Creating Snippets

Default snippets are created on first launch:

#### Example: Email Address
- **Keyword**: `myemail`
- **Expansion**: `user@example.com`
- **Usage**: Type "Snippets" in Command Palette, find snippet, expand

#### Example: Current Date
- **Keyword**: `today`
- **Expansion**: `{date}`
- **Result**: Expands to current date like `2026-01-29`

#### Example: Greeting with Prompt
- **Keyword**: `greet`
- **Expansion**: `Hello {input:Enter name}! How are you?`
- **Usage**: Prompts for name when expanded

#### Example: Meeting Notes
- **Keyword**: `meeting`
- **Expansion**: 
  ```
  # Meeting Notes - {date}
  
  ## Attendees
  - 
  
  ## Agenda
  - 
  
  ## Action Items
  - 
  ```
- **Result**: Creates structured meeting notes with today's date

### Variable Examples

- `{date}` → `2026-01-29`
- `{date+7d}` → `2026-02-05` (7 days from now)
- `{date-1m}` → `2025-12-29` (1 month ago)
- `{time}` → `14:30:45`
- `{datetime}` → `2026-01-29 14:30:45`
- `{timestamp}` → `1738126785`
- `{username}` → Your Windows username
- `{env:PATH}` → Your PATH environment variable
- `{clipboard}` → Content from clipboard
- `{input:Enter your name}` → Prompts for input

## Data Storage

Snippets are stored in JSON format at:
```
%LOCALAPPDATA%\PowerToys\cmdpal_snippets\snippets.json
```

You can directly edit this file or use the Import/Export features for backup.

## Architecture

### Components

1. **Models**: Data structures for snippets and collections
   - `Snippet.cs` - Individual snippet with metadata
   - `SnippetCollection.cs` - Collection/dictionary of snippets
   - `SnippetData.cs` - Root data structure

2. **Services**:
   - `SnippetStorageService.cs` - JSON storage and retrieval
   - `VariableExpansionService.cs` - Variable parsing and expansion

3. **Pages**: Command Palette UI pages
   - `SnippetsMainPage.cs` - Browse collections
   - `SnippetSearchPage.cs` - Search all snippets
   - `CollectionManagementPage.cs` - Manage collections
   - `ImportExportPage.cs` - Backup and restore

4. **Commands**:
   - `ExpandSnippetCommand.cs` - Expand and copy snippet

### Technology Stack

- **.NET 9.0** with Windows SDK
- **PowerToys Command Palette SDK** for extension integration
- **Windows P/Invoke APIs** for keyboard hooks and input simulation
  - SetWindowsHookEx (WH_KEYBOARD_LL) for low-level keyboard monitoring
  - SendInput for reliable text insertion
  - User32.dll for clipboard and window management
- **JSON** for data storage
- **Regex** for variable parsing and pattern matching

## Future Enhancements

Planned features for future versions:

### v1.1 - Enhanced Triggers ✅ COMPLETE
- [x] Auto-expansion engine (background service)
- [x] System-wide keyboard hook
- [x] SendInput API integration for direct insertion
- [ ] Global hotkey support per snippet/collection
- [ ] Advanced regex trigger patterns with capture groups

### v1.2 - Advanced Features
- [ ] Cloud sync (OneDrive, GitHub)
- [ ] AI-powered snippet suggestions
- [ ] Multi-language support (IME-aware)
- [ ] Voice command integration
- [ ] Rich text (RTF/HTML) support

### v2.0 - Team Features
- [ ] Shared snippet libraries
- [ ] Team collaboration
- [ ] Snippet templates marketplace
- [ ] Advanced analytics dashboard

## Development

### Building

This project requires Windows 10/11 with .NET 9.0 SDK:

```bash
dotnet restore
dotnet build
```

### Project Structure

```
cmdpal_snippet_replace_advanced/
├── Models/                  # Data models
│   ├── Snippet.cs
│   ├── SnippetCollection.cs
│   └── SnippetData.cs
├── Services/               # Business logic
│   ├── SnippetStorageService.cs
│   └── VariableExpansionService.cs
├── Pages/                  # UI pages
│   └── Snippets/
│       ├── SnippetsMainPage.cs
│       ├── SnippetSearchPage.cs
│       ├── CollectionManagementPage.cs
│       ├── ImportExportPage.cs
│       └── ExpandSnippetCommand.cs
├── Assets/                 # Icons and resources
├── Program.cs             # Entry point
└── Package.appxmanifest  # App manifest
```

## Comparison with Alfred

| Feature | Alfred (macOS) | cmdpal_snippet_replace_advanced |
|---------|----------------|-------------------------------|
| Collections | ✅ Yes | ✅ Yes + prefixes/suffixes |
| Basic Variables | ✅ Yes | ✅ Yes |
| Date/Time | ✅ Yes | ✅ Yes + date math |
| Clipboard | ✅ Yes | ✅ Yes |
| Input Prompts | ✅ Yes | ✅ Yes |
| Environment Vars | ❌ Limited | ✅ Full support |
| Auto-Expand | ✅ Yes | ✅ Yes (Windows hooks) |
| Hotkeys | ✅ Yes | 🚧 Planned |
| Regex Triggers | ❌ No | ✅ Yes |
| Import/Export | ✅ Yes | ✅ Yes |
| Statistics | ✅ Yes | ✅ Yes |
| Cloud Sync | ✅ Yes | 🚧 Planned |

## License

Copyright (c) Microsoft Corporation. Licensed under the MIT license.

## Contributing

Contributions are welcome! This project aims to provide Alfred-quality snippet management for Windows users.

### Ideas for Contributors
- Implement auto-expansion background service
- Add form-based snippet editing UI
- Create snippet template library
- Improve variable expansion with more types
- Add PowerShell script snippet support
- Implement regex trigger patterns

## Support

For issues and feature requests, please use the GitHub issue tracker.

