// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace cmdpal_snippet_replace_advanced.Services.Windows;

/// <summary>
/// Simulates keyboard input using Windows SendInput API
/// </summary>
public static class InputSimulator
{
    /// <summary>
    /// Send text by simulating keystrokes
    /// </summary>
    public static bool SendText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return true;

        try
        {
            var inputs = new NativeMethods.INPUT[text.Length * 2]; // key down + key up for each char
            var inputIndex = 0;

            foreach (var c in text)
            {
                // Key down
                inputs[inputIndex++] = CreateKeyboardInput(c, false);
                // Key up
                inputs[inputIndex++] = CreateKeyboardInput(c, true);
            }

            var sent = NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
            return sent == inputs.Length;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to send text: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Send backspace keystrokes to delete characters
    /// </summary>
    public static bool SendBackspaces(int count)
    {
        if (count <= 0)
            return true;

        try
        {
            var inputs = new NativeMethods.INPUT[count * 2]; // key down + key up for each backspace
            var inputIndex = 0;

            for (int i = 0; i < count; i++)
            {
                // Key down
                inputs[inputIndex++] = CreateKeyInput(NativeMethods.VK_BACK, false);
                // Key up
                inputs[inputIndex++] = CreateKeyInput(NativeMethods.VK_BACK, true);
            }

            var sent = NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
            return sent == inputs.Length;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to send backspaces: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Delete text and insert replacement (for snippet expansion)
    /// </summary>
    public static bool ReplaceText(int deleteCount, string replacementText)
    {
        try
        {
            // Small delay to ensure previous keystrokes are processed
            Thread.Sleep(10);

            // Delete the typed keyword
            if (!SendBackspaces(deleteCount))
                return false;

            // Small delay between delete and insert
            Thread.Sleep(10);

            // Insert the replacement text
            return SendText(replacementText);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to replace text: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Create keyboard input for a Unicode character
    /// </summary>
    private static NativeMethods.INPUT CreateKeyboardInput(char c, bool isKeyUp)
    {
        return new NativeMethods.INPUT
        {
            type = NativeMethods.INPUT_KEYBOARD,
            u = new NativeMethods.InputUnion
            {
                ki = new NativeMethods.KEYBDINPUT
                {
                    wVk = 0,
                    wScan = c,
                    dwFlags = NativeMethods.KEYEVENTF_UNICODE | (isKeyUp ? NativeMethods.KEYEVENTF_KEYUP : 0),
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };
    }

    /// <summary>
    /// Create keyboard input for a virtual key code
    /// </summary>
    private static NativeMethods.INPUT CreateKeyInput(int vkCode, bool isKeyUp)
    {
        return new NativeMethods.INPUT
        {
            type = NativeMethods.INPUT_KEYBOARD,
            u = new NativeMethods.InputUnion
            {
                ki = new NativeMethods.KEYBDINPUT
                {
                    wVk = (ushort)vkCode,
                    wScan = 0,
                    dwFlags = isKeyUp ? NativeMethods.KEYEVENTF_KEYUP : 0,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };
    }

    /// <summary>
    /// Simulate Ctrl+V to paste from clipboard
    /// </summary>
    public static bool SimulatePaste()
    {
        try
        {
            var inputs = new NativeMethods.INPUT[4];
            
            // Ctrl down
            inputs[0] = CreateKeyInput(NativeMethods.VK_CONTROL, false);
            // V down
            inputs[1] = CreateKeyInput(0x56, false); // V key
            // V up
            inputs[2] = CreateKeyInput(0x56, true);
            // Ctrl up
            inputs[3] = CreateKeyInput(NativeMethods.VK_CONTROL, true);

            var sent = NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
            return sent == inputs.Length;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to simulate paste: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Check if Ctrl, Shift, or Alt is currently pressed
    /// </summary>
    public static bool IsModifierPressed()
    {
        return (NativeMethods.GetKeyState(NativeMethods.VK_CONTROL) & 0x8000) != 0 ||
               (NativeMethods.GetKeyState(NativeMethods.VK_SHIFT) & 0x8000) != 0 ||
               (NativeMethods.GetKeyState(NativeMethods.VK_MENU) & 0x8000) != 0;
    }
}
