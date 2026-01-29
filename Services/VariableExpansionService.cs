// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace cmdpal_snippet_replace_advanced.Services;

/// <summary>
/// Handles expansion of variables in snippet text
/// Supports: {date}, {time}, {clipboard}, {input:prompt}, {env:VAR}, date math
/// </summary>
public sealed partial class VariableExpansionService
{
    [GeneratedRegex(@"\{([^}]+)\}", RegexOptions.Compiled)]
    private static partial Regex VariablePattern();

    /// <summary>
    /// Expand all variables in the given text
    /// </summary>
    public async Task<string> ExpandVariablesAsync(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var result = new StringBuilder(text);
        var matches = VariablePattern().Matches(text);

        // Process matches in reverse order to maintain string positions
        foreach (Match match in matches.Reverse())
        {
            var variable = match.Groups[1].Value;
            var expanded = await ExpandVariableAsync(variable);
            result.Remove(match.Index, match.Length);
            result.Insert(match.Index, expanded);
        }

        return result.ToString();
    }

    /// <summary>
    /// Expand a single variable
    /// </summary>
    private async Task<string> ExpandVariableAsync(string variable)
    {
        // Check for input prompts: {input:prompt text}
        if (variable.StartsWith("input:", StringComparison.OrdinalIgnoreCase))
        {
            var prompt = variable.Substring(6);
            return await GetInputFromUserAsync(prompt);
        }

        // Check for environment variables: {env:VAR}
        if (variable.StartsWith("env:", StringComparison.OrdinalIgnoreCase))
        {
            var varName = variable.Substring(4);
            return Environment.GetEnvironmentVariable(varName) ?? string.Empty;
        }

        // Check for date math: {date+7d}, {date-1m}, etc.
        if (variable.StartsWith("date", StringComparison.OrdinalIgnoreCase) && variable.Length > 4)
        {
            return ExpandDateMath(variable);
        }

        // Basic variables
        return variable.ToLowerInvariant() switch
        {
            "date" => DateTime.Now.ToString("yyyy-MM-dd"),
            "time" => DateTime.Now.ToString("HH:mm:ss"),
            "datetime" => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            "year" => DateTime.Now.Year.ToString(),
            "month" => DateTime.Now.Month.ToString("D2"),
            "day" => DateTime.Now.Day.ToString("D2"),
            "hour" => DateTime.Now.Hour.ToString("D2"),
            "minute" => DateTime.Now.Minute.ToString("D2"),
            "second" => DateTime.Now.Second.ToString("D2"),
            "timestamp" => DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
            "clipboard" => await GetClipboardTextAsync(),
            "selection" => await GetSelectedTextAsync(),
            "username" => Environment.UserName,
            "computername" => Environment.MachineName,
            "userdomain" => Environment.UserDomainName,
            _ => $"{{{variable}}}" // Return unchanged if not recognized
        };
    }

    /// <summary>
    /// Expand date math expressions like {date+7d}, {date-1m}
    /// </summary>
    private static string ExpandDateMath(string expression)
    {
        try
        {
            // Parse expressions like "date+7d", "date-1m", "date+2w"
            var match = Regex.Match(expression, @"date([+-])(\d+)([dwmy])", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return DateTime.Now.ToString("yyyy-MM-dd");
            }

            var operation = match.Groups[1].Value;
            var amount = int.Parse(match.Groups[2].Value);
            var unit = match.Groups[3].Value.ToLower();

            if (operation == "-")
            {
                amount = -amount;
            }

            var result = unit switch
            {
                "d" => DateTime.Now.AddDays(amount),
                "w" => DateTime.Now.AddDays(amount * 7),
                "m" => DateTime.Now.AddMonths(amount),
                "y" => DateTime.Now.AddYears(amount),
                _ => DateTime.Now
            };

            return result.ToString("yyyy-MM-dd");
        }
        catch
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }
    }

    /// <summary>
    /// Get text from clipboard
    /// </summary>
    private static async Task<string> GetClipboardTextAsync()
    {
        try
        {
            var text = await WindowsIntegrationService.GetClipboardTextAsync();
            return text ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Get currently selected text
    /// </summary>
    private static async Task<string> GetSelectedTextAsync()
    {
        try
        {
            var text = await WindowsIntegrationService.GetSelectedTextAsync();
            return text ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Prompt user for input
    /// </summary>
    private static async Task<string> GetInputFromUserAsync(string prompt)
    {
        // In a real implementation, this would show a dialog to get user input
        // For Command Palette, this could trigger a form page
        await Task.CompletedTask;
        return $"[{prompt}]";
    }

    /// <summary>
    /// Preview expansion without prompting for input
    /// </summary>
    public string PreviewExpansion(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var result = new StringBuilder(text);
        var matches = VariablePattern().Matches(text);

        // Process matches in reverse order
        foreach (Match match in matches.Reverse())
        {
            var variable = match.Groups[1].Value;
            var preview = GetVariablePreview(variable);
            result.Remove(match.Index, match.Length);
            result.Insert(match.Index, preview);
        }

        return result.ToString();
    }

    /// <summary>
    /// Get preview text for a variable without actually expanding it
    /// </summary>
    private static string GetVariablePreview(string variable)
    {
        if (variable.StartsWith("input:", StringComparison.OrdinalIgnoreCase))
        {
            var prompt = variable.Substring(6);
            return $"<{prompt}>";
        }

        if (variable.StartsWith("env:", StringComparison.OrdinalIgnoreCase))
        {
            var varName = variable.Substring(4);
            return Environment.GetEnvironmentVariable(varName) ?? $"<{varName}>";
        }

        if (variable.StartsWith("date", StringComparison.OrdinalIgnoreCase) && variable.Length > 4)
        {
            return ExpandDateMath(variable);
        }

        return variable.ToLowerInvariant() switch
        {
            "date" => DateTime.Now.ToString("yyyy-MM-dd"),
            "time" => DateTime.Now.ToString("HH:mm:ss"),
            "datetime" => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            "year" => DateTime.Now.Year.ToString(),
            "month" => DateTime.Now.Month.ToString("D2"),
            "day" => DateTime.Now.Day.ToString("D2"),
            "hour" => DateTime.Now.Hour.ToString("D2"),
            "minute" => DateTime.Now.Minute.ToString("D2"),
            "second" => DateTime.Now.Second.ToString("D2"),
            "timestamp" => DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
            "clipboard" => "<clipboard>",
            "selection" => "<selection>",
            "username" => Environment.UserName,
            "computername" => Environment.MachineName,
            "userdomain" => Environment.UserDomainName,
            _ => $"{{{variable}}}"
        };
    }
}
