// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using cmdpal_snippet_replace_advanced.Models;
using cmdpal_snippet_replace_advanced.Services;
using System.Collections.Generic;
using System.Linq;

namespace cmdpal_snippet_replace_advanced.Pages.Snippets;

/// <summary>
/// Main snippets page showing collections
/// </summary>
internal sealed class SnippetsMainPage : ListPage
{
    private readonly SnippetStorageService _storageService;
    private readonly VariableExpansionService _variableService;

    public SnippetsMainPage(SnippetStorageService storageService, VariableExpansionService variableService)
    {
        _storageService = storageService;
        _variableService = variableService;
        
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "Snippets";
        Name = "Browse Collections";
    }

    public override IListItem[] GetItems()
    {
        var items = new List<IListItem>();

        // Load collections
        var collections = _storageService.GetCollectionsAsync().GetAwaiter().GetResult();
        
        foreach (var collection in collections)
        {
            var snippetCount = _storageService.GetSnippetsByCollectionAsync(collection.Id)
                .GetAwaiter().GetResult().Count;
            
            items.Add(new ListItem(new CollectionSnippetsPage(_storageService, _variableService, collection.Id))
            {
                Title = collection.Name,
                Subtitle = $"{snippetCount} snippets - {collection.Description ?? "No description"}",
                Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png")
            });
        }

        // Add option to create new collection
        items.Add(new ListItem(new NoOpCommand())
        {
            Title = "+ Create New Collection",
            Subtitle = "Add a new snippet collection",
            Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png")
        });

        return items.ToArray();
    }
}

/// <summary>
/// Page showing snippets within a collection
/// </summary>
internal sealed class CollectionSnippetsPage : ListPage
{
    private readonly SnippetStorageService _storageService;
    private readonly VariableExpansionService _variableService;
    private readonly string _collectionId;

    public CollectionSnippetsPage(SnippetStorageService storageService, VariableExpansionService variableService, string collectionId)
    {
        _storageService = storageService;
        _variableService = variableService;
        _collectionId = collectionId;
        
        var collection = _storageService.GetCollectionAsync(collectionId).GetAwaiter().GetResult();
        
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = collection?.Name ?? "Collection";
        Name = "Snippets";
    }

    public override IListItem[] GetItems()
    {
        var items = new List<IListItem>();
        
        var snippets = _storageService.GetSnippetsByCollectionAsync(_collectionId)
            .GetAwaiter().GetResult();

        foreach (var snippet in snippets.OrderBy(s => s.Title))
        {
            var preview = _variableService.PreviewExpansion(snippet.ExpansionText);
            if (preview.Length > 100)
            {
                preview = preview.Substring(0, 100) + "...";
            }

            items.Add(new ListItem(new ExpandSnippetCommand(_storageService, _variableService, snippet.Id))
            {
                Title = snippet.Title,
                Subtitle = $"{snippet.Keyword} → {preview}",
                Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png"),
                MoreInfoMarkdown = $"**Keyword:** {snippet.Keyword}\n\n**Expansion:**\n```\n{snippet.ExpansionText}\n```\n\n**Preview:**\n```\n{preview}\n```\n\n**Tags:** {string.Join(", ", snippet.Tags)}\n\n**Usage:** {snippet.UsageCount} times"
            });
        }

        // Add option to create new snippet
        items.Add(new ListItem(new NoOpCommand())
        {
            Title = "+ Add New Snippet",
            Subtitle = "Create a new snippet in this collection",
            Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png")
        });

        if (items.Count == 1)
        {
            items.Insert(0, new ListItem(new NoOpCommand())
            {
                Title = "No snippets yet",
                Subtitle = "Create your first snippet to get started"
            });
        }

        return items.ToArray();
    }
}
