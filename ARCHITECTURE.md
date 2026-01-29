# Architecture Overview

## System Design

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                   PowerToys Command Palette                  │
│                        (Host Process)                        │
└────────────────────────┬────────────────────────────────────┘
                         │ COM Activation
                         ▼
┌─────────────────────────────────────────────────────────────┐
│          cmdpal_snippet_replace_advanced Extension          │
│                    (Out-of-Process COM)                      │
├─────────────────────────────────────────────────────────────┤
│  CommandsProvider                                            │
│  ├─ SnippetsMainPage                                        │
│  ├─ SnippetSearchPage                                       │
│  ├─ AutoExpansionSettingsPage                              │
│  ├─ CollectionManagementPage                               │
│  ├─ SnippetStatisticsPage                                  │
│  └─ ImportExportPage                                        │
└─────────────────────────┬────────────────────────────────────┘
                         │
         ┌───────────────┼───────────────┐
         ▼               ▼               ▼
    ┌─────────┐   ┌──────────┐   ┌─────────────┐
    │ Storage │   │ Variable │   │Auto-Expand  │
    │ Service │   │ Expansion│   │  Service    │
    └────┬────┘   └─────┬────┘   └──────┬──────┘
         │              │                │
         ▼              ▼                ▼
    ┌─────────────────────────────────────────┐
    │         Windows Integration             │
    │  ├─ Keyboard Hook (SetWindowsHookEx)   │
    │  ├─ Input Simulation (SendInput)        │
    │  ├─ Clipboard Operations                │
    │  └─ Window Class Detection              │
    └─────────────────────────────────────────┘
```

## Core Components

### 1. Data Models (`Models/`)

#### Snippet
- **Purpose**: Represents a text snippet with expansion rules
- **Key Properties**:
  - `Id`: Unique identifier (GUID)
  - `Title`: Display name
  - `Keyword`: Trigger text (e.g., "myemail")
  - `ExpansionText`: Text to expand (with variables)
  - `Tags`: Categorization
  - `UsageCount`: Statistics tracking
  - `IsRegexTrigger`: Whether keyword is a regex pattern

#### SnippetCollection
- **Purpose**: Groups related snippets with shared settings
- **Key Properties**:
  - `Name`: Collection name (e.g., "Work", "Personal")
  - `Prefix/Suffix`: Auto-applied to snippet titles
  - `AutoExpandEnabled`: Enable/disable auto-expansion for collection
  - `Hotkey`: Optional global hotkey

#### SnippetData
- **Purpose**: Root storage structure
- **Format**: JSON serialization
- **Location**: `%LOCALAPPDATA%\PowerToys\cmdpal_snippets\snippets.json`

### 2. Services (`Services/`)

#### SnippetStorageService
**Responsibilities**:
- Load/save snippets from/to JSON
- CRUD operations for snippets and collections
- Search and filtering
- Import/export functionality

**Key Methods**:
- `LoadDataAsync()`: Load from disk with caching
- `SaveSnippetAsync()`: Persist snippet changes
- `SearchSnippetsAsync()`: Full-text search
- `ExportSnippetsAsync()`: JSON export
- `ImportSnippetsAsync()`: JSON import with merge

**Caching Strategy**: In-memory cache, cleared on write operations

#### VariableExpansionService
**Responsibilities**:
- Parse and expand variables in snippet text
- Support for dynamic variables (date, time, clipboard)
- Date math expressions
- Input prompts

**Supported Variables**:
- Basic: `{date}`, `{time}`, `{datetime}`, `{year}`, `{month}`, `{day}`
- Date Math: `{date+7d}`, `{date-1m}`, `{date+2w}`
- System: `{username}`, `{computername}`, `{env:VAR}`
- Dynamic: `{clipboard}`, `{selection}`, `{input:prompt}`
- Utility: `{timestamp}`

**Implementation**:
- Regex-based parsing: `\{([^}]+)\}`
- Async expansion for I/O operations (clipboard, user input)
- Preview mode for UI display

#### AutoExpansionService
**Responsibilities**:
- System-wide keyboard monitoring
- Real-time snippet detection
- Automatic text replacement
- Buffer management

**Key Components**:
1. **Keyboard Hook**:
   - Uses `SetWindowsHookEx(WH_KEYBOARD_LL)`
   - Captures all keystrokes system-wide
   - Converts VK codes to characters

2. **Text Buffer**:
   - Maintains last 100 typed characters
   - Clears on Escape, Enter, or modifier keys
   - Efficiently tracks typing

3. **Matching Engine**:
   - Checks buffer against snippet keywords
   - Longest match first (ordered by keyword length)
   - Regex pattern support

4. **Expansion Process**:
   ```
   1. User types "myemail"
   2. Buffer contains "myemail"
   3. Match found → schedule expansion (300ms delay)
   4. Timer fires → expand variables
   5. Send backspaces (7 times to delete "myemail")
   6. Send Unicode characters (expanded text)
   7. Update usage statistics
   ```

**Configuration**:
- `TriggerDelayMs`: Default 300ms (prevents accidental triggers)
- `EnableInSecureFields`: Default false (skip password fields)
- `MaxBufferSize`: Default 100 characters

#### ConfigurationService
**Responsibilities**:
- Persist user settings
- Auto-expansion configuration
- UI preferences

**Storage**: `%LOCALAPPDATA%\PowerToys\cmdpal_snippets\config.json`

### 3. Windows Integration (`Services/Windows/`)

#### NativeMethods
**Purpose**: P/Invoke declarations for Windows APIs

**Key APIs**:
- `SetWindowsHookEx`: Install low-level keyboard hook
- `SendInput`: Simulate keyboard input
- `GetForegroundWindow`: Get active window
- `GetClassName`: Detect secure fields
- Clipboard functions: `OpenClipboard`, `GetClipboardData`, `SetClipboardData`

#### KeyboardHookManager
**Responsibilities**:
- Install/uninstall keyboard hook
- Process keyboard events
- Convert VK codes to characters

**Event Flow**:
```
Windows → Hook Callback → Parse Keystroke → Fire Event → AutoExpansionService
```

**Thread Safety**: Hook must be on UI thread or message pump

#### InputSimulator
**Responsibilities**:
- Send keystrokes using SendInput API
- Unicode character support
- Backspace simulation

**Key Methods**:
- `SendText()`: Send Unicode text
- `SendBackspaces()`: Delete characters
- `ReplaceText()`: Delete + insert (atomic operation)
- `SimulatePaste()`: Ctrl+V simulation

#### ClipboardHelper
**Responsibilities**:
- Get/set Windows clipboard
- Unicode text support
- Save/restore clipboard state

**Use Cases**:
- Variable expansion: `{clipboard}`
- Get selected text: Save → Ctrl+C → Get → Restore

### 4. Command Palette Pages (`Pages/Snippets/`)

#### SnippetsMainPage
- Lists all collections
- Shows snippet count per collection
- Navigation to collection details

#### SnippetSearchPage
- Search across all snippets
- Sort by usage count
- Quick expansion from search results

#### AutoExpansionSettingsPage
- Enable/disable auto-expansion
- Configure trigger delay
- View active snippets
- Toggle secure field expansion

#### CollectionManagementPage
- View all collections
- Shows snippet counts
- Configure prefixes/suffixes

#### SnippetStatisticsPage
- Overall usage statistics
- Most used snippets
- Recently used snippets
- Unused snippets
- Per-collection analytics

#### ImportExportPage
- Export to JSON file
- Import with merge or replace
- Backup functionality

## Data Flow

### Snippet Expansion (Manual)

```
User → Command Palette → Select Snippet
  ↓
ExpandSnippetCommand
  ↓
VariableExpansionService.ExpandVariablesAsync()
  ↓
WindowsIntegrationService.CopyToClipboardAsync()
  ↓
Update Statistics → Save to Storage
```

### Auto-Expansion Flow

```
User types in any app
  ↓
Windows Keyboard Event
  ↓
KeyboardHookManager.HookCallback()
  ↓
AutoExpansionService.OnKeyPressed()
  ↓
Add char to buffer → Check for match
  ↓
Match found? → Schedule Timer (300ms)
  ↓
Timer fires → PerformExpansionAsync()
  ↓
Check secure field → Expand variables
  ↓
InputSimulator.ReplaceText()
  - Send backspaces (delete keyword)
  - Send Unicode text (expanded text)
  ↓
Update statistics → Clear buffer
```

## Threading Model

### UI Thread
- Command Palette pages
- User interactions
- Page rendering

### Hook Thread
- Keyboard hook callback (must return quickly)
- Events fire on hook thread

### Background Thread
- Timer for expansion delay
- File I/O operations
- Variable expansion

**Synchronization**: `lock` objects for thread-safe collections and state

## Performance Considerations

### Keyboard Hook
- **Critical**: Must return < 1ms to avoid input lag
- **Strategy**: Minimal processing in hook, delegate to background
- **Buffering**: Limit to 100 characters to prevent memory growth

### Text Insertion
- **Method**: SendInput with Unicode (KEYEVENTF_UNICODE)
- **Speed**: ~1ms per character (imperceptible to user)
- **Alternative**: Clipboard + Ctrl+V for large texts (faster but disrupts clipboard)

### Storage
- **Caching**: In-memory cache after first load
- **Writes**: Async file I/O with JSON serialization
- **Format**: Indented JSON for human readability

### Startup
- **Lazy Loading**: Services created on first use
- **Auto-Expansion**: Only starts if enabled in config
- **Snippets**: Loaded on first access (not at startup)

## Security Considerations

### Keyboard Hook
- **Privilege**: Requires accessibility permissions
- **Risk**: Can capture all keystrokes (including passwords)
- **Mitigation**: 
  - Secure field detection (skip password fields)
  - No network transmission
  - Local storage only
  - Open source (auditable)

### Clipboard Access
- **Risk**: Can read/modify clipboard
- **Mitigation**:
  - Only accessed for `{clipboard}` variable
  - Clipboard restored after operations
  - No persistence of clipboard data

### Data Storage
- **Location**: User-specific %LOCALAPPDATA%
- **Format**: Plain JSON (optional encryption planned)
- **Permissions**: User read/write only

## Extension Lifecycle

### Initialization
```
1. PowerToys activates COM server
2. Create extension instance
3. Register CommandsProvider
4. Load configuration
5. Auto-start auto-expansion if enabled
6. Extension ready
```

### Disposal
```
1. User closes PowerToys or disables extension
2. Dispose() called on extension
3. Stop auto-expansion service
4. Unhook keyboard
5. Clean up resources
6. COM server shutdown
```

## Future Architecture Enhancements

### Hotkey Support
- Global hotkey registration using `RegisterHotKey`
- Per-snippet and per-collection hotkeys
- Configurable key combinations

### Cloud Sync
- Abstract storage interface (local/cloud)
- OneDrive integration via Microsoft Graph API
- GitHub Gist integration via REST API
- Conflict resolution strategies

### Plugin System
- Custom variable providers
- Third-party snippet sources
- API integrations (GitHub, Jira, etc.)

## Testing Strategy

### Unit Tests
- Storage service (mock file system)
- Variable expansion (all variable types)
- Date math calculations
- Buffer management

### Integration Tests
- Keyboard hook (requires Windows)
- Input simulation (requires Windows)
- Clipboard operations (requires Windows)

### Manual Tests
- Auto-expansion in: VS Code, Chrome, Notepad, Word
- Secure field detection
- Unicode characters (emoji, special chars)
- Performance (typing lag test)
