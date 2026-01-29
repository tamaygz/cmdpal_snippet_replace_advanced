// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace cmdpal_snippet_replace_advanced.Services.Windows;

/// <summary>
/// Manages low-level keyboard hook for capturing keystrokes system-wide
/// </summary>
public sealed class KeyboardHookManager : IDisposable
{
    private IntPtr _hookId = IntPtr.Zero;
    private NativeMethods.LowLevelKeyboardProc? _hookCallback;
    private readonly object _lock = new();
    private bool _isHooked = false;

    /// <summary>
    /// Event fired when a key is pressed
    /// </summary>
    public event EventHandler<KeyPressedEventArgs>? KeyPressed;

    /// <summary>
    /// Install the keyboard hook
    /// </summary>
    public bool Install()
    {
        lock (_lock)
        {
            if (_isHooked)
                return true;

            try
            {
                _hookCallback = HookCallback;
                using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
                using var curModule = curProcess.MainModule;
                
                if (curModule?.ModuleName == null)
                    return false;

                _hookId = NativeMethods.SetWindowsHookEx(
                    NativeMethods.WH_KEYBOARD_LL,
                    _hookCallback,
                    NativeMethods.GetModuleHandle(curModule.ModuleName),
                    0);

                _isHooked = _hookId != IntPtr.Zero;
                return _isHooked;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to install keyboard hook: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Uninstall the keyboard hook
    /// </summary>
    public void Uninstall()
    {
        lock (_lock)
        {
            if (!_isHooked)
                return;

            try
            {
                if (_hookId != IntPtr.Zero)
                {
                    NativeMethods.UnhookWindowsHookEx(_hookId);
                    _hookId = IntPtr.Zero;
                }
                _isHooked = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to uninstall keyboard hook: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Low-level keyboard hook callback
    /// </summary>
    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        try
        {
            if (nCode >= 0 && wParam == (IntPtr)NativeMethods.WM_KEYDOWN)
            {
                var hookStruct = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
                var vkCode = (int)hookStruct.vkCode;

                // Get the character representation
                var character = GetCharFromKey(vkCode);
                
                // Fire the event
                KeyPressed?.Invoke(this, new KeyPressedEventArgs
                {
                    VirtualKeyCode = vkCode,
                    Character = character,
                    IsModifier = IsModifierKey(vkCode)
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in keyboard hook: {ex.Message}");
        }

        return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    /// <summary>
    /// Convert virtual key code to character
    /// </summary>
    private static char? GetCharFromKey(int vkCode)
    {
        try
        {
            var keyboardState = new byte[256];
            NativeMethods.GetKeyboardState(keyboardState);

            var threadId = (uint)Thread.CurrentThread.ManagedThreadId;
            var hkl = NativeMethods.GetKeyboardLayout(threadId);

            var result = new StringBuilder(2);
            var ret = NativeMethods.ToUnicodeEx(
                (uint)vkCode,
                0,
                keyboardState,
                result,
                result.Capacity,
                0,
                hkl);

            if (ret > 0 && result.Length > 0)
            {
                return result[0];
            }
        }
        catch
        {
            // Ignore errors
        }

        return null;
    }

    /// <summary>
    /// Check if key is a modifier (Shift, Ctrl, Alt)
    /// </summary>
    private static bool IsModifierKey(int vkCode)
    {
        return vkCode == NativeMethods.VK_SHIFT ||
               vkCode == NativeMethods.VK_CONTROL ||
               vkCode == NativeMethods.VK_MENU;
    }

    public void Dispose()
    {
        Uninstall();
    }
}

/// <summary>
/// Event arguments for key press events
/// </summary>
public sealed class KeyPressedEventArgs : EventArgs
{
    /// <summary>
    /// Virtual key code
    /// </summary>
    public int VirtualKeyCode { get; init; }

    /// <summary>
    /// Character representation (if available)
    /// </summary>
    public char? Character { get; init; }

    /// <summary>
    /// Whether this is a modifier key
    /// </summary>
    public bool IsModifier { get; init; }
}
