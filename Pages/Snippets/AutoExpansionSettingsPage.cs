// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using cmdpal_snippet_replace_advanced.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace cmdpal_snippet_replace_advanced.Pages.Snippets;

/// <summary>
/// Page for managing auto-expansion settings
/// </summary>
internal sealed class AutoExpansionSettingsPage : ListPage
{
    private readonly AutoExpansionService _autoExpansionService;
    private readonly ConfigurationService _configService;

    public AutoExpansionSettingsPage(AutoExpansionService autoExpansionService, ConfigurationService configService)
    {
        _autoExpansionService = autoExpansionService;
        _configService = configService;
        
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "Auto-Expansion Settings";
        Name = "Configure";
    }

    public override IListItem[] GetItems()
    {
        var items = new List<IListItem>();
        var config = _configService.LoadConfigAsync().GetAwaiter().GetResult();
        var isEnabled = _autoExpansionService.IsEnabled;

        // Status
        items.Add(new ListItem(new NoOpCommand())
        {
            Title = "📊 Status",
            Subtitle = isEnabled ? "✅ Auto-expansion is ENABLED" : "❌ Auto-expansion is DISABLED",
            Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png"),
            MoreInfoMarkdown = isEnabled
                ? "Auto-expansion is currently active. Snippets will be expanded automatically as you type.\n\n" +
                  $"**Active Snippets:** {_autoExpansionService.GetActiveSnippets().Count}\n\n" +
                  $"**Delay:** {_autoExpansionService.TriggerDelayMs}ms\n\n" +
                  $"**Secure Fields:** {(config.AutoExpansion.EnableInSecureFields ? "Enabled" : "Disabled")}"
                : "Auto-expansion is currently disabled. Enable it to automatically expand snippets as you type."
        });

        // Toggle service
        if (isEnabled)
        {
            items.Add(new ListItem(new ToggleAutoExpansionCommand(_autoExpansionService, _configService, false))
            {
                Title = "⏸️ Disable Auto-Expansion",
                Subtitle = "Stop automatically expanding snippets",
                Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png")
            });
        }
        else
        {
            items.Add(new ListItem(new ToggleAutoExpansionCommand(_autoExpansionService, _configService, true))
            {
                Title = "▶️ Enable Auto-Expansion",
                Subtitle = "Start automatically expanding snippets as you type",
                Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png"),
                MoreInfoMarkdown = "**Note:** Auto-expansion requires Windows accessibility permissions. " +
                    "When enabled, the extension will monitor your keystrokes system-wide to detect snippet keywords."
            });
        }

        // Settings
        items.Add(new ListItem(new NoOpCommand())
        {
            Title = "⚙️ Settings",
            Subtitle = "Configure auto-expansion behavior",
            Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png")
        });

        items.Add(new ListItem(new NoOpCommand())
        {
            Title = $"  Trigger Delay: {config.AutoExpansion.DelayMs}ms",
            Subtitle = "Time to wait before expanding (default: 300ms)",
            Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png"),
            MoreInfoMarkdown = "The delay in milliseconds before a snippet is expanded after typing its keyword. " +
                "Increase this if you're experiencing accidental expansions. Recommended: 200-1000ms."
        });

        items.Add(new ListItem(new NoOpCommand())
        {
            Title = $"  Secure Fields: {(config.AutoExpansion.EnableInSecureFields ? "Enabled" : "Disabled")}",
            Subtitle = "Allow expansion in password fields",
            Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png"),
            MoreInfoMarkdown = "**Warning:** Enabling this allows snippet expansion in password fields and secure inputs. " +
                "This is generally not recommended for security reasons."
        });

        // Active snippets
        items.Add(new ListItem(new NoOpCommand())
        {
            Title = "📋 Active Snippets",
            Subtitle = $"{_autoExpansionService.GetActiveSnippets().Count} snippets available for auto-expansion",
            Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png"),
            MoreInfoMarkdown = "Only snippets in collections with auto-expansion enabled will be active. " +
                "Check your collection settings to enable/disable auto-expansion per collection."
        });

        var activeSnippets = _autoExpansionService.GetActiveSnippets();
        if (activeSnippets.Count > 0)
        {
            foreach (var snippet in activeSnippets.Take(10))
            {
                items.Add(new ListItem(new NoOpCommand())
                {
                    Title = $"  {snippet.Keyword} → {snippet.Title}",
                    Subtitle = snippet.IsRegexTrigger ? "Regex trigger" : "Keyword trigger",
                    Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png")
                });
            }

            if (activeSnippets.Count > 10)
            {
                items.Add(new ListItem(new NoOpCommand())
                {
                    Title = $"  ... and {activeSnippets.Count - 10} more",
                    Subtitle = "Use Search Snippets to see all"
                });
            }
        }

        return items.ToArray();
    }
}

/// <summary>
/// Command to toggle auto-expansion on/off
/// </summary>
internal sealed class ToggleAutoExpansionCommand : Command
{
    private readonly AutoExpansionService _autoExpansionService;
    private readonly ConfigurationService _configService;
    private readonly bool _enable;

    public ToggleAutoExpansionCommand(AutoExpansionService autoExpansionService, ConfigurationService configService, bool enable)
    {
        _autoExpansionService = autoExpansionService;
        _configService = configService;
        _enable = enable;
    }

    public override async Task<ICommandResult> ExecuteAsync()
    {
        try
        {
            var config = await _configService.LoadConfigAsync();

            if (_enable)
            {
                // Enable auto-expansion
                _autoExpansionService.TriggerDelayMs = config.AutoExpansion.DelayMs;
                _autoExpansionService.EnableInSecureFields = config.AutoExpansion.EnableInSecureFields;
                
                await _autoExpansionService.StartAsync();
                
                config.AutoExpansion.Enabled = true;
                await _configService.SaveConfigAsync(config);

                return new CommandResult
                {
                    State = CommandResultState.Success,
                    Message = $"Auto-expansion enabled! {_autoExpansionService.GetActiveSnippets().Count} snippets active."
                };
            }
            else
            {
                // Disable auto-expansion
                _autoExpansionService.Stop();
                
                config.AutoExpansion.Enabled = false;
                await _configService.SaveConfigAsync(config);

                return new CommandResult
                {
                    State = CommandResultState.Success,
                    Message = "Auto-expansion disabled"
                };
            }
        }
        catch (Exception ex)
        {
            return new CommandResult
            {
                State = CommandResultState.Error,
                Message = $"Failed to toggle auto-expansion: {ex.Message}"
            };
        }
    }
}
