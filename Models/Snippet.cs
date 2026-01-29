// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text.Json.Serialization;

namespace cmdpal_snippet_replace_advanced.Models;

/// <summary>
/// Represents a text snippet with keyword trigger and expansion text
/// </summary>
public sealed class Snippet
{
    /// <summary>
    /// Unique identifier for the snippet
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Display title for the snippet
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Keyword/alias that triggers this snippet (e.g., "addr")
    /// </summary>
    [JsonPropertyName("keyword")]
    public string Keyword { get; set; } = string.Empty;

    /// <summary>
    /// The text that will be expanded when the keyword is triggered
    /// Supports variables like {date}, {time}, {clipboard}, {input:prompt}
    /// </summary>
    [JsonPropertyName("expansionText")]
    public string ExpansionText { get; set; } = string.Empty;

    /// <summary>
    /// Optional tags for categorizing snippets
    /// </summary>
    [JsonPropertyName("tags")]
    public string[] Tags { get; set; } = Array.Empty<string>();

    /// <summary>
    /// ID of the collection this snippet belongs to
    /// </summary>
    [JsonPropertyName("collectionId")]
    public string CollectionId { get; set; } = string.Empty;

    /// <summary>
    /// Number of times this snippet has been used
    /// </summary>
    [JsonPropertyName("usageCount")]
    public int UsageCount { get; set; } = 0;

    /// <summary>
    /// Last time this snippet was used
    /// </summary>
    [JsonPropertyName("lastUsed")]
    public DateTime? LastUsed { get; set; }

    /// <summary>
    /// When this snippet was created
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this snippet was last modified
    /// </summary>
    [JsonPropertyName("modifiedAt")]
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional hotkey for this snippet (e.g., "Win+Shift+1")
    /// </summary>
    [JsonPropertyName("hotkey")]
    public string? Hotkey { get; set; }

    /// <summary>
    /// Whether this is a regex trigger pattern
    /// </summary>
    [JsonPropertyName("isRegexTrigger")]
    public bool IsRegexTrigger { get; set; } = false;

    /// <summary>
    /// Increment usage statistics
    /// </summary>
    public void IncrementUsage()
    {
        UsageCount++;
        LastUsed = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;
    }
}
