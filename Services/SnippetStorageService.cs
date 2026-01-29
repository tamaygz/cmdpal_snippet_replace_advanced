// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using cmdpal_snippet_replace_advanced.Models;

namespace cmdpal_snippet_replace_advanced.Services;

/// <summary>
/// Manages storage and retrieval of snippets and collections
/// </summary>
public sealed class SnippetStorageService
{
    private readonly string _dataFilePath;
    private SnippetData? _cachedData;
    private readonly object _lock = new();

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public SnippetStorageService()
    {
        // Store in %LOCALAPPDATA%\PowerToys\cmdpal_snippets
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dataDirectory = Path.Combine(localAppData, "PowerToys", "cmdpal_snippets");
        Directory.CreateDirectory(dataDirectory);
        _dataFilePath = Path.Combine(dataDirectory, "snippets.json");
    }

    /// <summary>
    /// Load all snippet data from disk
    /// </summary>
    public async Task<SnippetData> LoadDataAsync()
    {
        lock (_lock)
        {
            if (_cachedData != null)
            {
                return _cachedData;
            }
        }

        if (!File.Exists(_dataFilePath))
        {
            // Initialize with default data
            var defaultData = CreateDefaultData();
            await SaveDataAsync(defaultData);
            return defaultData;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_dataFilePath);
            var data = JsonSerializer.Deserialize<SnippetData>(json, _jsonOptions) ?? CreateDefaultData();
            
            lock (_lock)
            {
                _cachedData = data;
            }
            
            return data;
        }
        catch (Exception)
        {
            // If file is corrupted, return default data
            return CreateDefaultData();
        }
    }

    /// <summary>
    /// Save all snippet data to disk
    /// </summary>
    public async Task SaveDataAsync(SnippetData data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            await File.WriteAllTextAsync(_dataFilePath, json);
            
            lock (_lock)
            {
                _cachedData = data;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save snippets: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Get all collections
    /// </summary>
    public async Task<IReadOnlyList<SnippetCollection>> GetCollectionsAsync()
    {
        var data = await LoadDataAsync();
        return data.Collections;
    }

    /// <summary>
    /// Get a collection by ID
    /// </summary>
    public async Task<SnippetCollection?> GetCollectionAsync(string collectionId)
    {
        var data = await LoadDataAsync();
        return data.Collections.FirstOrDefault(c => c.Id == collectionId);
    }

    /// <summary>
    /// Add or update a collection
    /// </summary>
    public async Task SaveCollectionAsync(SnippetCollection collection)
    {
        var data = await LoadDataAsync();
        var existingIndex = Array.FindIndex(data.Collections, c => c.Id == collection.Id);
        
        if (existingIndex >= 0)
        {
            data.Collections[existingIndex] = collection;
        }
        else
        {
            var collections = data.Collections.ToList();
            collections.Add(collection);
            data.Collections = collections.ToArray();
        }

        await SaveDataAsync(data);
    }

    /// <summary>
    /// Delete a collection and all its snippets
    /// </summary>
    public async Task DeleteCollectionAsync(string collectionId)
    {
        var data = await LoadDataAsync();
        data.Collections = data.Collections.Where(c => c.Id != collectionId).ToArray();
        data.Snippets = data.Snippets.Where(s => s.CollectionId != collectionId).ToArray();
        await SaveDataAsync(data);
    }

    /// <summary>
    /// Get all snippets
    /// </summary>
    public async Task<IReadOnlyList<Snippet>> GetSnippetsAsync()
    {
        var data = await LoadDataAsync();
        return data.Snippets;
    }

    /// <summary>
    /// Get snippets in a specific collection
    /// </summary>
    public async Task<IReadOnlyList<Snippet>> GetSnippetsByCollectionAsync(string collectionId)
    {
        var data = await LoadDataAsync();
        return data.Snippets.Where(s => s.CollectionId == collectionId).ToArray();
    }

    /// <summary>
    /// Get a snippet by ID
    /// </summary>
    public async Task<Snippet?> GetSnippetAsync(string snippetId)
    {
        var data = await LoadDataAsync();
        return data.Snippets.FirstOrDefault(s => s.Id == snippetId);
    }

    /// <summary>
    /// Get a snippet by keyword
    /// </summary>
    public async Task<Snippet?> GetSnippetByKeywordAsync(string keyword)
    {
        var data = await LoadDataAsync();
        return data.Snippets.FirstOrDefault(s => 
            s.Keyword.Equals(keyword, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Search snippets by keyword, title, or tags
    /// </summary>
    public async Task<IReadOnlyList<Snippet>> SearchSnippetsAsync(string query)
    {
        var data = await LoadDataAsync();
        var lowerQuery = query.ToLowerInvariant();
        
        return data.Snippets.Where(s =>
            s.Title.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase) ||
            s.Keyword.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase) ||
            s.Tags.Any(t => t.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase)) ||
            s.ExpansionText.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase)
        ).ToArray();
    }

    /// <summary>
    /// Add or update a snippet
    /// </summary>
    public async Task SaveSnippetAsync(Snippet snippet)
    {
        var data = await LoadDataAsync();
        var existingIndex = Array.FindIndex(data.Snippets, s => s.Id == snippet.Id);
        
        snippet.ModifiedAt = DateTime.UtcNow;
        
        if (existingIndex >= 0)
        {
            data.Snippets[existingIndex] = snippet;
        }
        else
        {
            var snippets = data.Snippets.ToList();
            snippets.Add(snippet);
            data.Snippets = snippets.ToArray();
        }

        await SaveDataAsync(data);
    }

    /// <summary>
    /// Delete a snippet
    /// </summary>
    public async Task DeleteSnippetAsync(string snippetId)
    {
        var data = await LoadDataAsync();
        data.Snippets = data.Snippets.Where(s => s.Id != snippetId).ToArray();
        await SaveDataAsync(data);
    }

    /// <summary>
    /// Export snippets to JSON string
    /// </summary>
    public async Task<string> ExportSnippetsAsync()
    {
        var data = await LoadDataAsync();
        return JsonSerializer.Serialize(data, _jsonOptions);
    }

    /// <summary>
    /// Import snippets from JSON string
    /// </summary>
    public async Task ImportSnippetsAsync(string json, bool merge = true)
    {
        var importedData = JsonSerializer.Deserialize<SnippetData>(json, _jsonOptions);
        if (importedData == null)
        {
            throw new InvalidOperationException("Invalid JSON data");
        }

        if (merge)
        {
            var existingData = await LoadDataAsync();
            
            // Merge collections
            var collections = existingData.Collections.ToList();
            foreach (var collection in importedData.Collections)
            {
                if (!collections.Any(c => c.Id == collection.Id))
                {
                    collections.Add(collection);
                }
            }
            
            // Merge snippets
            var snippets = existingData.Snippets.ToList();
            foreach (var snippet in importedData.Snippets)
            {
                if (!snippets.Any(s => s.Id == snippet.Id))
                {
                    snippets.Add(snippet);
                }
            }
            
            existingData.Collections = collections.ToArray();
            existingData.Snippets = snippets.ToArray();
            await SaveDataAsync(existingData);
        }
        else
        {
            await SaveDataAsync(importedData);
        }
    }

    /// <summary>
    /// Clear cache to force reload from disk
    /// </summary>
    public void ClearCache()
    {
        lock (_lock)
        {
            _cachedData = null;
        }
    }

    /// <summary>
    /// Create default data with sample snippets
    /// </summary>
    private static SnippetData CreateDefaultData()
    {
        var personalCollection = new SnippetCollection
        {
            Name = "Personal",
            Description = "Personal snippets",
            Prefix = null,
            Suffix = null
        };

        var workCollection = new SnippetCollection
        {
            Name = "Work",
            Description = "Work-related snippets",
            Prefix = "Work: ",
            Suffix = null
        };

        var snippets = new[]
        {
            new Snippet
            {
                CollectionId = personalCollection.Id,
                Title = "Email Address",
                Keyword = "myemail",
                ExpansionText = "user@example.com",
                Tags = new[] { "email", "contact" }
            },
            new Snippet
            {
                CollectionId = personalCollection.Id,
                Title = "Current Date",
                Keyword = "today",
                ExpansionText = "{date}",
                Tags = new[] { "date", "time" }
            },
            new Snippet
            {
                CollectionId = personalCollection.Id,
                Title = "Greeting with Name",
                Keyword = "greet",
                ExpansionText = "Hello {input:Enter name}! How are you?",
                Tags = new[] { "greeting", "template" }
            },
            new Snippet
            {
                CollectionId = workCollection.Id,
                Title = "Meeting Notes Template",
                Keyword = "meeting",
                ExpansionText = "# Meeting Notes - {date}\n\n## Attendees\n- \n\n## Agenda\n- \n\n## Action Items\n- \n\n## Next Steps\n- ",
                Tags = new[] { "work", "template", "notes" }
            }
        };

        return new SnippetData
        {
            Collections = new[] { personalCollection, workCollection },
            Snippets = snippets
        };
    }
}
