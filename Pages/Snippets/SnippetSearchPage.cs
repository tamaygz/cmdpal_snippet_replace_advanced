// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using cmdpal_snippet_replace_advanced.Services;
using System.Collections.Generic;
using System.Linq;

namespace cmdpal_snippet_replace_advanced.Pages.Snippets;

/// <summary>
/// Page for searching all snippets
/// </summary>
internal sealed class SnippetSearchPage : ListPage
{
    private readonly SnippetStorageService _storageService;
    private readonly VariableExpansionService _variableService;

    public SnippetSearchPage(SnippetStorageService storageService, VariableExpansionService variableService)
    {
        _storageService = storageService;
        _variableService = variableService;
        
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "Search Snippets";
        Name = "Search";
    }

    public override IListItem[] GetItems()
    {
        var items = new List<IListItem>();
        
        // Get all snippets
        var snippets = _storageService.GetSnippetsAsync().GetAwaiter().GetResult();
        
        // Group by collection
        var collections = _storageService.GetCollectionsAsync().GetAwaiter().GetResult();
        var collectionMap = collections.ToDictionary(c => c.Id, c => c.Name);

        foreach (var snippet in snippets.OrderByDescending(s => s.UsageCount).ThenBy(s => s.Title))
        {
            var collectionName = collectionMap.TryGetValue(snippet.CollectionId, out var name) ? name : "Unknown";
            var preview = _variableService.PreviewExpansion(snippet.ExpansionText);
            if (preview.Length > 80)
            {
                preview = preview.Substring(0, 80) + "...";
            }

            items.Add(new ListItem(new ExpandSnippetCommand(_storageService, _variableService, snippet.Id))
            {
                Title = $"{snippet.Keyword} - {snippet.Title}",
                Subtitle = $"[{collectionName}] {preview}",
                Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png"),
                MoreInfoMarkdown = $"**Collection:** {collectionName}\n\n**Keyword:** {snippet.Keyword}\n\n**Expansion:**\n```\n{snippet.ExpansionText}\n```\n\n**Preview:**\n```\n{preview}\n```\n\n**Tags:** {string.Join(", ", snippet.Tags)}\n\n**Usage:** {snippet.UsageCount} times\n\n**Last used:** {snippet.LastUsed?.ToString("g") ?? "Never"}"
            });
        }

        if (items.Count == 0)
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "No snippets found",
                Subtitle = "Create your first snippet to get started"
            });
        }

        return items.ToArray();
    }
}
