// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using cmdpal_snippet_replace_advanced.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cmdpal_snippet_replace_advanced.Pages.Snippets;

/// <summary>
/// Page showing snippet usage statistics and analytics
/// </summary>
internal sealed class SnippetStatisticsPage : ListPage
{
    private readonly SnippetStorageService _storageService;

    public SnippetStatisticsPage(SnippetStorageService storageService)
    {
        _storageService = storageService;
        
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "Snippet Statistics";
        Name = "Analytics";
    }

    public override IListItem[] GetItems()
    {
        var items = new List<IListItem>();
        
        var snippets = _storageService.GetSnippetsAsync().GetAwaiter().GetResult();
        var collections = _storageService.GetCollectionsAsync().GetAwaiter().GetResult();
        var collectionMap = collections.ToDictionary(c => c.Id, c => c.Name);

        // Overall statistics
        var totalSnippets = snippets.Count;
        var totalUsage = snippets.Sum(s => s.UsageCount);
        var avgUsage = totalSnippets > 0 ? totalUsage / (double)totalSnippets : 0;
        
        items.Add(new ListItem(new NoOpCommand())
        {
            Title = "📊 Overall Statistics",
            Subtitle = $"{totalSnippets} snippets • {totalUsage} total uses • {avgUsage:F1} avg uses per snippet",
            Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png"),
            MoreInfoMarkdown = $"# Overall Statistics\n\n" +
                $"**Total Snippets:** {totalSnippets}\n\n" +
                $"**Total Uses:** {totalUsage}\n\n" +
                $"**Average Uses per Snippet:** {avgUsage:F2}\n\n" +
                $"**Collections:** {collections.Count}"
        });

        // Most used snippets
        items.Add(new ListItem(new NoOpCommand())
        {
            Title = "🔥 Most Used Snippets",
            Subtitle = "Top snippets by usage count",
            Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png")
        });

        var topSnippets = snippets
            .Where(s => s.UsageCount > 0)
            .OrderByDescending(s => s.UsageCount)
            .Take(10);

        foreach (var snippet in topSnippets)
        {
            var collectionName = collectionMap.TryGetValue(snippet.CollectionId, out var name) ? name : "Unknown";
            var lastUsed = snippet.LastUsed?.ToString("g") ?? "Never";
            
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = $"  {snippet.UsageCount}× {snippet.Title}",
                Subtitle = $"{snippet.Keyword} • [{collectionName}] • Last: {lastUsed}",
                Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png"),
                MoreInfoMarkdown = $"**Keyword:** {snippet.Keyword}\n\n" +
                    $"**Collection:** {collectionName}\n\n" +
                    $"**Usage Count:** {snippet.UsageCount}\n\n" +
                    $"**Last Used:** {lastUsed}\n\n" +
                    $"**Created:** {snippet.CreatedAt:g}"
            });
        }

        // Recently used snippets
        items.Add(new ListItem(new NoOpCommand())
        {
            Title = "⏱️ Recently Used",
            Subtitle = "Snippets used recently",
            Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png")
        });

        var recentSnippets = snippets
            .Where(s => s.LastUsed.HasValue)
            .OrderByDescending(s => s.LastUsed)
            .Take(10);

        foreach (var snippet in recentSnippets)
        {
            var collectionName = collectionMap.TryGetValue(snippet.CollectionId, out var name) ? name : "Unknown";
            var lastUsed = snippet.LastUsed?.ToString("g") ?? "Never";
            var timeAgo = snippet.LastUsed.HasValue 
                ? GetTimeAgo(snippet.LastUsed.Value) 
                : "never";
            
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = $"  {snippet.Title}",
                Subtitle = $"{snippet.Keyword} • [{collectionName}] • {timeAgo}",
                Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png"),
                MoreInfoMarkdown = $"**Keyword:** {snippet.Keyword}\n\n" +
                    $"**Collection:** {collectionName}\n\n" +
                    $"**Last Used:** {lastUsed}\n\n" +
                    $"**Usage Count:** {snippet.UsageCount}"
            });
        }

        // Unused snippets
        var unusedSnippets = snippets.Where(s => s.UsageCount == 0).ToArray();
        if (unusedSnippets.Length > 0)
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "💤 Unused Snippets",
                Subtitle = $"{unusedSnippets.Length} snippets never used",
                Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png"),
                MoreInfoMarkdown = $"You have {unusedSnippets.Length} snippets that have never been used. " +
                    "Consider reviewing them to see if they're still needed."
            });
        }

        // Collection statistics
        items.Add(new ListItem(new NoOpCommand())
        {
            Title = "📚 Collection Statistics",
            Subtitle = "Usage by collection",
            Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png")
        });

        var collectionStats = snippets
            .GroupBy(s => s.CollectionId)
            .Select(g => new
            {
                CollectionId = g.Key,
                Count = g.Count(),
                TotalUsage = g.Sum(s => s.UsageCount),
                AvgUsage = g.Average(s => s.UsageCount)
            })
            .OrderByDescending(c => c.TotalUsage);

        foreach (var stat in collectionStats)
        {
            var collectionName = collectionMap.TryGetValue(stat.CollectionId, out var name) ? name : "Unknown";
            
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = $"  {collectionName}",
                Subtitle = $"{stat.Count} snippets • {stat.TotalUsage} uses • {stat.AvgUsage:F1} avg",
                Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png")
            });
        }

        if (items.Count == 1)
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "No statistics available",
                Subtitle = "Create and use snippets to see statistics"
            });
        }

        return items.ToArray();
    }

    /// <summary>
    /// Get human-readable time ago string
    /// </summary>
    private static string GetTimeAgo(DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime.ToUniversalTime();

        if (timeSpan.TotalSeconds < 60)
            return "just now";
        
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes >= 2 ? "s" : "")} ago";
        
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} hour{(timeSpan.TotalHours >= 2 ? "s" : "")} ago";
        
        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays} day{(timeSpan.TotalDays >= 2 ? "s" : "")} ago";
        
        if (timeSpan.TotalDays < 30)
            return $"{(int)(timeSpan.TotalDays / 7)} week{(timeSpan.TotalDays >= 14 ? "s" : "")} ago";
        
        if (timeSpan.TotalDays < 365)
            return $"{(int)(timeSpan.TotalDays / 30)} month{(timeSpan.TotalDays >= 60 ? "s" : "")} ago";
        
        return $"{(int)(timeSpan.TotalDays / 365)} year{(timeSpan.TotalDays >= 730 ? "s" : "")} ago";
    }
}
