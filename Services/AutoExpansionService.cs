// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using cmdpal_snippet_replace_advanced.Models;
using cmdpal_snippet_replace_advanced.Services.Windows;

namespace cmdpal_snippet_replace_advanced.Services;

/// <summary>
/// Manages auto-expansion of snippets based on typed keywords using Windows keyboard hooks
/// </summary>
public sealed class AutoExpansionService : IDisposable
{
    private readonly SnippetStorageService _storageService;
    private readonly VariableExpansionService _variableService;
    private readonly KeyboardHookManager _keyboardHook;
    private readonly object _lock = new();
    private bool _isEnabled = false;
    private List<Snippet> _activeSnippets = new();
    private readonly StringBuilder _typedBuffer = new(256);
    private Timer? _expansionTimer;
    private Snippet? _pendingSnippet;
    private int _pendingKeywordLength;

    // Configuration
    public int TriggerDelayMs { get; set; } = 300; // Delay before expanding
    public bool EnableInSecureFields { get; set; } = false;
    public int MaxBufferSize { get; set; } = 100; // Max characters to track

    public AutoExpansionService(SnippetStorageService storageService, VariableExpansionService variableService)
    {
        _storageService = storageService;
        _variableService = variableService;
        _keyboardHook = new KeyboardHookManager();
        _keyboardHook.KeyPressed += OnKeyPressed;
    }

    /// <summary>
    /// Start the auto-expansion service
    /// </summary>
    public async Task StartAsync()
    {
        lock (_lock)
        {
            if (_isEnabled) return;
            _isEnabled = true;
        }

        // Load all snippets
        await ReloadSnippetsAsync();

        // Install keyboard hook
        if (!_keyboardHook.Install())
        {
            lock (_lock)
            {
                _isEnabled = false;
            }
            throw new InvalidOperationException("Failed to install keyboard hook. Make sure the application has the required permissions.");
        }
        
        System.Diagnostics.Debug.WriteLine("Auto-expansion service started with keyboard hook");
    }

    /// <summary>
    /// Stop the auto-expansion service
    /// </summary>
    public void Stop()
    {
        lock (_lock)
        {
            if (!_isEnabled) return;
            _isEnabled = false;
        }

        // Cancel any pending expansion
        _expansionTimer?.Dispose();
        _expansionTimer = null;

        // Unhook keyboard
        _keyboardHook.Uninstall();
        
        System.Diagnostics.Debug.WriteLine("Auto-expansion service stopped");
    }

    /// <summary>
    /// Handle keyboard events
    /// </summary>
    private void OnKeyPressed(object? sender, KeyPressedEventArgs e)
    {
        if (!_isEnabled)
            return;

        try
        {
            // Cancel pending expansion on any new keystroke
            _expansionTimer?.Dispose();
            _expansionTimer = null;
            _pendingSnippet = null;

            // Ignore if modifier keys are pressed (Ctrl, Alt)
            if (InputSimulator.IsModifierPressed())
            {
                ClearBuffer();
                return;
            }

            // Handle special keys
            if (e.VirtualKeyCode == NativeMethods.VK_BACK)
            {
                // Backspace - remove last character from buffer
                if (_typedBuffer.Length > 0)
                    _typedBuffer.Length--;
                return;
            }

            if (e.VirtualKeyCode == NativeMethods.VK_ESCAPE || 
                e.VirtualKeyCode == NativeMethods.VK_RETURN ||
                e.VirtualKeyCode == NativeMethods.VK_TAB)
            {
                ClearBuffer();
                return;
            }

            // Add character to buffer
            if (e.Character.HasValue && !char.IsControl(e.Character.Value))
            {
                _typedBuffer.Append(e.Character.Value);

                // Limit buffer size
                if (_typedBuffer.Length > MaxBufferSize)
                {
                    _typedBuffer.Remove(0, _typedBuffer.Length - MaxBufferSize);
                }

                // Check for snippet match
                var bufferText = _typedBuffer.ToString();
                var snippet = FindMatchingSnippet(bufferText);
                
                if (snippet != null)
                {
                    // Found a match - schedule expansion after delay
                    var keywordLength = snippet.IsRegexTrigger ? bufferText.Length : snippet.Keyword.Length;
                    ScheduleExpansion(snippet, keywordLength);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in key press handler: {ex.Message}");
        }
    }

    /// <summary>
    /// Schedule snippet expansion after configured delay
    /// </summary>
    private void ScheduleExpansion(Snippet snippet, int keywordLength)
    {
        _pendingSnippet = snippet;
        _pendingKeywordLength = keywordLength;

        _expansionTimer = new Timer(async _ =>
        {
            try
            {
                await PerformExpansionAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during expansion: {ex.Message}");
            }
        }, null, TriggerDelayMs, Timeout.Infinite);
    }

    /// <summary>
    /// Perform the actual snippet expansion
    /// </summary>
    private async Task PerformExpansionAsync()
    {
        var snippet = _pendingSnippet;
        var keywordLength = _pendingKeywordLength;

        if (snippet == null)
            return;

        try
        {
            // Check if we're in a secure field
            if (!EnableInSecureFields && WindowsIntegrationService.IsSecureField())
            {
                System.Diagnostics.Debug.WriteLine("Skipping expansion in secure field");
                ClearBuffer();
                return;
            }

            // Expand variables in snippet text
            var expandedText = await _variableService.ExpandVariablesAsync(snippet.ExpansionText);

            // Perform the replacement: delete keyword, insert expansion
            if (InputSimulator.ReplaceText(keywordLength, expandedText))
            {
                // Update statistics
                snippet.IncrementUsage();
                await _storageService.SaveSnippetAsync(snippet);

                System.Diagnostics.Debug.WriteLine($"Expanded snippet: {snippet.Keyword} → {expandedText.Length} chars");
            }

            ClearBuffer();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to expand snippet: {ex.Message}");
            ClearBuffer();
        }
    }

    /// <summary>
    /// Clear the typed buffer
    /// </summary>
    private void ClearBuffer()
    {
        _typedBuffer.Clear();
        _pendingSnippet = null;
        _pendingKeywordLength = 0;
    }

    /// <summary>
    /// Reload snippets from storage
    /// </summary>
    public async Task ReloadSnippetsAsync()
    {
        var allSnippets = await _storageService.GetSnippetsAsync();
        var collections = await _storageService.GetCollectionsAsync();
        
        // Filter to only snippets in collections that have auto-expand enabled
        var enabledCollectionIds = collections
            .Where(c => c.AutoExpandEnabled)
            .Select(c => c.Id)
            .ToHashSet();

        lock (_lock)
        {
            _activeSnippets = allSnippets
                .Where(s => enabledCollectionIds.Contains(s.CollectionId))
                .OrderByDescending(s => s.Keyword.Length) // Match longest keywords first
                .ToList();
        }
    }

    /// <summary>
    /// Check if text matches a snippet keyword
    /// This would be called from the keyboard hook
    /// </summary>
    public Snippet? FindMatchingSnippet(string typedText)
    {
        if (string.IsNullOrEmpty(typedText))
            return null;

        lock (_lock)
        {
            // First try exact matches
            var exactMatch = _activeSnippets.FirstOrDefault(s =>
                typedText.EndsWith(s.Keyword, StringComparison.OrdinalIgnoreCase));
            
            if (exactMatch != null)
                return exactMatch;

            // Then try regex patterns
            foreach (var snippet in _activeSnippets.Where(s => s.IsRegexTrigger))
            {
                try
                {
                    if (Regex.IsMatch(typedText, snippet.Keyword))
                        return snippet;
                }
                catch
                {
                    // Invalid regex, skip
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Expand a snippet in the current context
    /// This would be called when a match is found
    /// </summary>
    public async Task<bool> ExpandSnippetAsync(Snippet snippet)
    {
        try
        {
            // Check if we're in a secure field
            if (!EnableInSecureFields && WindowsIntegrationService.IsSecureField())
            {
                System.Diagnostics.Debug.WriteLine("Skipping expansion in secure field");
                return false;
            }

            // Expand variables
            var expandedText = await _variableService.ExpandVariablesAsync(snippet.ExpansionText);

            // Delete the typed keyword (backspace n times)
            // Then insert the expanded text
            // This would use Windows SendInput API
            await WindowsIntegrationService.InsertTextIntoActiveWindowAsync(expandedText);

            // Update statistics
            snippet.IncrementUsage();
            await _storageService.SaveSnippetAsync(snippet);

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Expansion error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get list of active snippets for debugging
    /// </summary>
    public IReadOnlyList<Snippet> GetActiveSnippets()
    {
        lock (_lock)
        {
            return _activeSnippets.ToArray();
        }
    }

    /// <summary>
    /// Check if service is running
    /// </summary>
    public bool IsEnabled
    {
        get
        {
            lock (_lock)
            {
                return _isEnabled;
            }
        }
    }

    public void Dispose()
    {
        Stop();
        _expansionTimer?.Dispose();
        _keyboardHook?.Dispose();
    }
}

/// <summary>
/// Configuration for auto-expansion behavior
/// </summary>
public sealed class AutoExpansionConfig
{
    /// <summary>
    /// Whether auto-expansion is enabled globally
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Delay in milliseconds before expanding (to avoid conflicts)
    /// </summary>
    public int DelayMs { get; set; } = 300;

    /// <summary>
    /// Whether to expand in password fields and secure inputs
    /// </summary>
    public bool EnableInSecureFields { get; set; } = false;

    /// <summary>
    /// List of application process names to exclude
    /// </summary>
    public string[] ExcludedApplications { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Whether to require a trigger character after keyword (e.g., space or semicolon)
    /// </summary>
    public bool RequireTriggerChar { get; set; } = false;

    /// <summary>
    /// Trigger character if RequireTriggerChar is true
    /// </summary>
    public char TriggerChar { get; set; } = ' ';
}
