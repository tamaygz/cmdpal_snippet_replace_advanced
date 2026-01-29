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
/// Page for managing snippet collections
/// </summary>
internal sealed class CollectionManagementPage : ListPage
{
    private readonly SnippetStorageService _storageService;

    public CollectionManagementPage(SnippetStorageService storageService)
    {
        _storageService = storageService;
        
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "Collection Management";
        Name = "Manage";
    }

    public override IListItem[] GetItems()
    {
        var items = new List<IListItem>();
        
        var collections = _storageService.GetCollectionsAsync().GetAwaiter().GetResult();

        foreach (var collection in collections.OrderBy(c => c.Name))
        {
            var snippetCount = _storageService.GetSnippetsByCollectionAsync(collection.Id)
                .GetAwaiter().GetResult().Count;

            var details = new List<string>
            {
                $"**Snippets:** {snippetCount}",
                $"**Created:** {collection.CreatedAt:g}"
            };

            if (!string.IsNullOrEmpty(collection.Prefix))
                details.Add($"**Prefix:** {collection.Prefix}");
            
            if (!string.IsNullOrEmpty(collection.Suffix))
                details.Add($"**Suffix:** {collection.Suffix}");
            
            if (!string.IsNullOrEmpty(collection.Hotkey))
                details.Add($"**Hotkey:** {collection.Hotkey}");

            details.Add($"**Auto-expand:** {(collection.AutoExpandEnabled ? "Enabled" : "Disabled")}");

            items.Add(new ListItem(new NoOpCommand())
            {
                Title = collection.Name,
                Subtitle = $"{snippetCount} snippets - {collection.Description ?? "No description"}",
                Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png"),
                MoreInfoMarkdown = string.Join("\n\n", details)
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
