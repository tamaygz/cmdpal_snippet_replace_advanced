// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using cmdpal_snippet_replace_advanced.Services.Windows;

namespace cmdpal_snippet_replace_advanced.Services;

/// <summary>
/// Windows-specific utilities for clipboard and text insertion
/// </summary>
public static class WindowsIntegrationService
{
    /// <summary>
    /// Copy text to Windows clipboard
    /// </summary>
    public static async Task<bool> CopyToClipboardAsync(string text)
    {
        try
        {
            await Task.CompletedTask;
            return ClipboardHelper.SetText(text);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Clipboard error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get text from Windows clipboard
    /// </summary>
    public static async Task<string?> GetClipboardTextAsync()
    {
        try
        {
            await Task.CompletedTask;
            return ClipboardHelper.GetText();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Clipboard error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Send keystrokes to insert text into active window
    /// Uses Windows SendInput API for reliable text insertion
    /// </summary>
    public static async Task<bool> InsertTextIntoActiveWindowAsync(string text)
    {
        try
        {
            await Task.CompletedTask;
            return InputSimulator.SendText(text);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Insert error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get currently selected text from active window
    /// Uses clipboard trick: save clipboard, send Ctrl+C, get text, restore clipboard
    /// </summary>
    public static async Task<string?> GetSelectedTextAsync()
    {
        try
        {
            // Save current clipboard
            var savedClipboard = ClipboardHelper.SaveClipboard();

            // Clear clipboard
            ClipboardHelper.SetText(string.Empty);

            // Wait a bit for clipboard to clear
            await Task.Delay(50);

            // Send Ctrl+C to copy selection
            if (!InputSimulator.SimulatePaste())
            {
                ClipboardHelper.RestoreClipboard(savedClipboard);
                return null;
            }

            // Wait for clipboard to receive data
            await Task.Delay(100);

            // Get clipboard text
            var selectedText = ClipboardHelper.GetText();

            // Restore original clipboard
            ClipboardHelper.RestoreClipboard(savedClipboard);

            return selectedText;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Get selection error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Check if current window is a secure field (password input)
    /// </summary>
    public static bool IsSecureField()
    {
        try
        {
            var hwnd = NativeMethods.GetForegroundWindow();
            if (hwnd == IntPtr.Zero)
                return false;

            var className = new StringBuilder(256);
            NativeMethods.GetClassName(hwnd, className, className.Capacity);
            var classNameStr = className.ToString().ToLowerInvariant();

            // Check for known password field class names
            return classNameStr.Contains("password") ||
                   classNameStr.Contains("secure") ||
                   classNameStr.Contains("credential");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Secure field check error: {ex.Message}");
            return false; // Assume not secure on error
        }
    }
}
