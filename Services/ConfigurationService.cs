// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace cmdpal_snippet_replace_advanced.Services;

/// <summary>
/// Manages application configuration and settings
/// </summary>
public sealed class ConfigurationService
{
    private readonly string _configFilePath;
    private AppConfig? _cachedConfig;
    private readonly object _lock = new();

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ConfigurationService()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var configDirectory = Path.Combine(localAppData, "PowerToys", "cmdpal_snippets");
        Directory.CreateDirectory(configDirectory);
        _configFilePath = Path.Combine(configDirectory, "config.json");
    }

    /// <summary>
    /// Load configuration from disk
    /// </summary>
    public async Task<AppConfig> LoadConfigAsync()
    {
        lock (_lock)
        {
            if (_cachedConfig != null)
            {
                return _cachedConfig;
            }
        }

        if (!File.Exists(_configFilePath))
        {
            var defaultConfig = new AppConfig();
            await SaveConfigAsync(defaultConfig);
            return defaultConfig;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_configFilePath);
            var config = JsonSerializer.Deserialize<AppConfig>(json, _jsonOptions) ?? new AppConfig();
            
            lock (_lock)
            {
                _cachedConfig = config;
            }
            
            return config;
        }
        catch
        {
            return new AppConfig();
        }
    }

    /// <summary>
    /// Save configuration to disk
    /// </summary>
    public async Task SaveConfigAsync(AppConfig config)
    {
        try
        {
            var json = JsonSerializer.Serialize(config, _jsonOptions);
            await File.WriteAllTextAsync(_configFilePath, json);
            
            lock (_lock)
            {
                _cachedConfig = config;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save configuration: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Clear cache to force reload
    /// </summary>
    public void ClearCache()
    {
        lock (_lock)
        {
            _cachedConfig = null;
        }
    }
}

/// <summary>
/// Application configuration
/// </summary>
public sealed class AppConfig
{
    /// <summary>
    /// Auto-expansion settings
    /// </summary>
    [JsonPropertyName("autoExpansion")]
    public AutoExpansionConfig AutoExpansion { get; set; } = new();

    /// <summary>
    /// UI preferences
    /// </summary>
    [JsonPropertyName("ui")]
    public UIConfig UI { get; set; } = new();

    /// <summary>
    /// Backup and sync settings
    /// </summary>
    [JsonPropertyName("backup")]
    public BackupConfig Backup { get; set; } = new();

    /// <summary>
    /// Privacy and security settings
    /// </summary>
    [JsonPropertyName("privacy")]
    public PrivacyConfig Privacy { get; set; } = new();
}

/// <summary>
/// UI configuration
/// </summary>
public sealed class UIConfig
{
    /// <summary>
    /// Number of items to show per page
    /// </summary>
    [JsonPropertyName("itemsPerPage")]
    public int ItemsPerPage { get; set; } = 20;

    /// <summary>
    /// Default sort order for snippets
    /// </summary>
    [JsonPropertyName("defaultSortOrder")]
    public string DefaultSortOrder { get; set; } = "title"; // title, usage, recent

    /// <summary>
    /// Show usage statistics in snippet list
    /// </summary>
    [JsonPropertyName("showUsageStats")]
    public bool ShowUsageStats { get; set; } = true;

    /// <summary>
    /// Show preview in snippet list
    /// </summary>
    [JsonPropertyName("showPreview")]
    public bool ShowPreview { get; set; } = true;

    /// <summary>
    /// Maximum preview length
    /// </summary>
    [JsonPropertyName("maxPreviewLength")]
    public int MaxPreviewLength { get; set; } = 100;
}

/// <summary>
/// Backup and sync configuration
/// </summary>
public sealed class BackupConfig
{
    /// <summary>
    /// Enable automatic backup
    /// </summary>
    [JsonPropertyName("autoBackupEnabled")]
    public bool AutoBackupEnabled { get; set; } = false;

    /// <summary>
    /// Backup frequency in days
    /// </summary>
    [JsonPropertyName("backupFrequencyDays")]
    public int BackupFrequencyDays { get; set; } = 7;

    /// <summary>
    /// Backup directory path
    /// </summary>
    [JsonPropertyName("backupDirectory")]
    public string? BackupDirectory { get; set; }

    /// <summary>
    /// Maximum number of backups to keep
    /// </summary>
    [JsonPropertyName("maxBackups")]
    public int MaxBackups { get; set; } = 5;

    /// <summary>
    /// Enable cloud sync (future feature)
    /// </summary>
    [JsonPropertyName("cloudSyncEnabled")]
    public bool CloudSyncEnabled { get; set; } = false;

    /// <summary>
    /// Cloud sync provider (onedrive, github, etc.)
    /// </summary>
    [JsonPropertyName("cloudSyncProvider")]
    public string CloudSyncProvider { get; set; } = "none";
}

/// <summary>
/// Privacy and security configuration
/// </summary>
public sealed class PrivacyConfig
{
    /// <summary>
    /// Enable usage analytics
    /// </summary>
    [JsonPropertyName("analyticsEnabled")]
    public bool AnalyticsEnabled { get; set; } = true;

    /// <summary>
    /// Enable audit logging
    /// </summary>
    [JsonPropertyName("auditLogEnabled")]
    public bool AuditLogEnabled { get; set; } = false;

    /// <summary>
    /// Encrypt stored snippets
    /// </summary>
    [JsonPropertyName("encryptStorage")]
    public bool EncryptStorage { get; set; } = false;

    /// <summary>
    /// Require confirmation before clipboard operations
    /// </summary>
    [JsonPropertyName("confirmClipboardOps")]
    public bool ConfirmClipboardOps { get; set; } = false;
}
