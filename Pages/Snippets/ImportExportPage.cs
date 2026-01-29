// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using cmdpal_snippet_replace_advanced.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace cmdpal_snippet_replace_advanced.Pages.Snippets;

/// <summary>
/// Page for importing and exporting snippets
/// </summary>
internal sealed class ImportExportPage : ListPage
{
    private readonly SnippetStorageService _storageService;

    public ImportExportPage(SnippetStorageService storageService)
    {
        _storageService = storageService;
        
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "Import/Export";
        Name = "Backup & Restore";
    }

    public override IListItem[] GetItems()
    {
        var items = new List<IListItem>
        {
            new ListItem(new ExportSnippetsCommand(_storageService))
            {
                Title = "Export All Snippets",
                Subtitle = "Export all snippets and collections to JSON file",
                Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png"),
                MoreInfoMarkdown = "Exports all snippets and collections to a JSON file in your Documents folder. This file can be used for backup or sharing snippets across devices."
            },
            new ListItem(new NoOpCommand())
            {
                Title = "Import Snippets (Merge)",
                Subtitle = "Import snippets from JSON file without replacing existing ones",
                Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png"),
                MoreInfoMarkdown = "Imports snippets from a JSON file and merges them with your existing snippets. Duplicate snippets (same ID) will be skipped."
            },
            new ListItem(new NoOpCommand())
            {
                Title = "Import Snippets (Replace)",
                Subtitle = "Import snippets from JSON file and replace all existing ones",
                Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png"),
                MoreInfoMarkdown = "**WARNING:** This will delete all your existing snippets and replace them with the imported ones. Make sure to export your current snippets first!"
            }
        };

        return items.ToArray();
    }
}

/// <summary>
/// Command to export snippets to JSON
/// </summary>
internal sealed class ExportSnippetsCommand : ICommand
{
    private readonly SnippetStorageService _storageService;

    public ExportSnippetsCommand(SnippetStorageService storageService)
    {
        _storageService = storageService;
    }

    public Task<ICommandResult> ExecuteAsync(ICommandContext context)
    {
        try
        {
            // Export to Documents folder
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var exportPath = Path.Combine(documentsPath, $"cmdpal_snippets_export_{DateTime.Now:yyyyMMdd_HHmmss}.json");

            var json = _storageService.ExportSnippetsAsync().GetAwaiter().GetResult();
            File.WriteAllText(exportPath, json);

            return Task.FromResult<ICommandResult>(new CommandResult
            {
                State = CommandResultState.Success,
                Message = $"Snippets exported to: {exportPath}"
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult<ICommandResult>(new CommandResult
            {
                State = CommandResultState.Error,
                Message = $"Failed to export snippets: {ex.Message}"
            });
        }
    }
}
