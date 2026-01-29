// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using cmdpal_snippet_replace_advanced.Services;
using System;
using System.Threading.Tasks;

namespace cmdpal_snippet_replace_advanced.Pages.Snippets;

/// <summary>
/// Command to expand a snippet and copy to clipboard
/// </summary>
internal sealed class ExpandSnippetCommand : Command
{
    private readonly SnippetStorageService _storageService;
    private readonly VariableExpansionService _variableService;
    private readonly string _snippetId;

    public ExpandSnippetCommand(SnippetStorageService storageService, VariableExpansionService variableService, string snippetId)
    {
        _storageService = storageService;
        _variableService = variableService;
        _snippetId = snippetId;
    }

    public override Task<ICommandResult> ExecuteAsync()
    {
        try
        {
            // Load the snippet
            var snippet = _storageService.GetSnippetAsync(_snippetId).GetAwaiter().GetResult();
            if (snippet == null)
            {
                return Task.FromResult<ICommandResult>(new CommandResult
                {
                    State = CommandResultState.Error,
                    Message = "Snippet not found"
                });
            }

            // Expand variables
            var expandedText = _variableService.ExpandVariablesAsync(snippet.ExpansionText)
                .GetAwaiter().GetResult();

            // Copy to clipboard
            CopyToClipboard(expandedText);

            // Update usage statistics
            snippet.IncrementUsage();
            _storageService.SaveSnippetAsync(snippet).GetAwaiter().GetResult();

            return Task.FromResult<ICommandResult>(new CommandResult
            {
                State = CommandResultState.Success,
                Message = $"Expanded '{snippet.Keyword}' and copied to clipboard"
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult<ICommandResult>(new CommandResult
            {
                State = CommandResultState.Error,
                Message = $"Failed to expand snippet: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Copy text to clipboard
    /// </summary>
    private static void CopyToClipboard(string text)
    {
        try
        {
            WindowsIntegrationService.CopyToClipboardAsync(text).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to copy to clipboard: {ex.Message}");
        }
    }
}
