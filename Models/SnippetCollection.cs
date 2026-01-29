// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text.Json.Serialization;

namespace cmdpal_snippet_replace_advanced.Models;

/// <summary>
/// Represents a collection (dictionary) of snippets with shared settings
/// </summary>
public sealed class SnippetCollection
{
    /// <summary>
    /// Unique identifier for the collection
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Display name for the collection (e.g., "Personal", "Work")
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of this collection
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Optional prefix to add to all snippet titles in this collection
    /// </summary>
    [JsonPropertyName("prefix")]
    public string? Prefix { get; set; }

    /// <summary>
    /// Optional suffix to add to all snippet titles in this collection
    /// </summary>
    [JsonPropertyName("suffix")]
    public string? Suffix { get; set; }

    /// <summary>
    /// Icon path for this collection
    /// </summary>
    [JsonPropertyName("iconPath")]
    public string? IconPath { get; set; }

    /// <summary>
    /// Whether this collection is enabled for auto-expansion
    /// </summary>
    [JsonPropertyName("autoExpandEnabled")]
    public bool AutoExpandEnabled { get; set; } = true;

    /// <summary>
    /// When this collection was created
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this collection was last modified
    /// </summary>
    [JsonPropertyName("modifiedAt")]
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional hotkey for expanding snippets in this collection
    /// </summary>
    [JsonPropertyName("hotkey")]
    public string? Hotkey { get; set; }

    /// <summary>
    /// Get formatted title with prefix/suffix if configured
    /// </summary>
    public string GetFormattedTitle(string title)
    {
        var result = title;
        if (!string.IsNullOrEmpty(Prefix))
        {
            result = Prefix + result;
        }
        if (!string.IsNullOrEmpty(Suffix))
        {
            result = result + Suffix;
        }
        return result;
    }
}
