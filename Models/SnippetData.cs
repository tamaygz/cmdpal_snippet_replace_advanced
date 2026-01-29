// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text.Json.Serialization;

namespace cmdpal_snippet_replace_advanced.Models;

/// <summary>
/// Root data structure for storing all snippets and collections
/// </summary>
public sealed class SnippetData
{
    /// <summary>
    /// Version of the data format
    /// </summary>
    [JsonPropertyName("version")]
    public int Version { get; set; } = 1;

    /// <summary>
    /// All snippet collections
    /// </summary>
    [JsonPropertyName("collections")]
    public SnippetCollection[] Collections { get; set; } = Array.Empty<SnippetCollection>();

    /// <summary>
    /// All snippets across all collections
    /// </summary>
    [JsonPropertyName("snippets")]
    public Snippet[] Snippets { get; set; } = Array.Empty<Snippet>();
}
