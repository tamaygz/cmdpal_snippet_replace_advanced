// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace cmdpal_snippet_replace_advanced.Services.Windows;

/// <summary>
/// Helper for Windows clipboard operations
/// </summary>
public static class ClipboardHelper
{
    /// <summary>
    /// Get text from Windows clipboard
    /// </summary>
    public static string? GetText()
    {
        if (!NativeMethods.OpenClipboard(IntPtr.Zero))
            return null;

        try
        {
            var handle = NativeMethods.GetClipboardData(NativeMethods.CF_UNICODETEXT);
            if (handle == IntPtr.Zero)
                return null;

            var ptr = NativeMethods.GlobalLock(handle);
            if (ptr == IntPtr.Zero)
                return null;

            try
            {
                return Marshal.PtrToStringUni(ptr);
            }
            finally
            {
                NativeMethods.GlobalUnlock(handle);
            }
        }
        finally
        {
            NativeMethods.CloseClipboard();
        }
    }

    /// <summary>
    /// Set text to Windows clipboard
    /// </summary>
    public static bool SetText(string text)
    {
        if (!NativeMethods.OpenClipboard(IntPtr.Zero))
            return false;

        try
        {
            NativeMethods.EmptyClipboard();

            var bytes = Encoding.Unicode.GetBytes(text + "\0");
            var hGlobal = NativeMethods.GlobalAlloc(
                NativeMethods.GMEM_MOVEABLE,
                (UIntPtr)bytes.Length);

            if (hGlobal == IntPtr.Zero)
                return false;

            var ptr = NativeMethods.GlobalLock(hGlobal);
            if (ptr == IntPtr.Zero)
            {
                NativeMethods.GlobalFree(hGlobal);
                return false;
            }

            try
            {
                Marshal.Copy(bytes, 0, ptr, bytes.Length);
            }
            finally
            {
                NativeMethods.GlobalUnlock(hGlobal);
            }

            var result = NativeMethods.SetClipboardData(NativeMethods.CF_UNICODETEXT, hGlobal);
            if (result == IntPtr.Zero)
            {
                NativeMethods.GlobalFree(hGlobal);
                return false;
            }

            return true;
        }
        finally
        {
            NativeMethods.CloseClipboard();
        }
    }

    /// <summary>
    /// Save current clipboard content
    /// </summary>
    public static string? SaveClipboard()
    {
        return GetText();
    }

    /// <summary>
    /// Restore clipboard content
    /// </summary>
    public static bool RestoreClipboard(string? text)
    {
        if (text == null)
            return true;

        return SetText(text);
    }
}
