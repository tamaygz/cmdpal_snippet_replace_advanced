// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using cmdpal_snippet_replace_advanced.Pages.Snippets;
using cmdpal_snippet_replace_advanced.Services;
using System;

namespace cmdpal_snippet_replace_advanced;

public partial class cmdpal_snippet_replace_advancedCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;
    private readonly SnippetStorageService _storageService;
    private readonly VariableExpansionService _variableService;
    private readonly ConfigurationService _configService;
    private readonly AutoExpansionService _autoExpansionService;

    public cmdpal_snippet_replace_advancedCommandsProvider()
    {
        DisplayName = "Snippets";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        
        _storageService = new SnippetStorageService();
        _variableService = new VariableExpansionService();
        _configService = new ConfigurationService();
        _autoExpansionService = new AutoExpansionService(_storageService, _variableService);
        
        // Initialize auto-expansion if enabled in config
        InitializeAutoExpansionAsync();
        
        _commands = [
            new CommandItem(new SnippetsMainPage(_storageService, _variableService)) 
            { 
                Title = "Snippets",
                Subtitle = "Manage and expand text snippets"
            },
            new CommandItem(new SnippetSearchPage(_storageService, _variableService)) 
            { 
                Title = "Search Snippets",
                Subtitle = "Search all snippets by keyword or content"
            },
            new CommandItem(new AutoExpansionSettingsPage(_autoExpansionService, _configService)) 
            { 
                Title = "Auto-Expansion Settings",
                Subtitle = "Configure automatic snippet expansion"
            },
            new CommandItem(new CollectionManagementPage(_storageService)) 
            { 
                Title = "Manage Collections",
                Subtitle = "Create and organize snippet collections"
            },
            new CommandItem(new SnippetStatisticsPage(_storageService)) 
            { 
                Title = "Snippet Statistics",
                Subtitle = "View usage analytics and insights"
            },
            new CommandItem(new ImportExportPage(_storageService)) 
            { 
                Title = "Import/Export Snippets",
                Subtitle = "Backup and restore your snippets"
            }
        ];
    }

    private async void InitializeAutoExpansionAsync()
    {
        try
        {
            var config = await _configService.LoadConfigAsync();
            if (config.AutoExpansion.Enabled)
            {
                _autoExpansionService.TriggerDelayMs = config.AutoExpansion.DelayMs;
                _autoExpansionService.EnableInSecureFields = config.AutoExpansion.EnableInSecureFields;
                
                await _autoExpansionService.StartAsync();
                System.Diagnostics.Debug.WriteLine("Auto-expansion initialized from config");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to initialize auto-expansion: {ex.Message}");
        }
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }

}
