// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace cmdpal_snippet_replace_advanced;

public partial class cmdpal_snippet_replace_advancedCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;

    public cmdpal_snippet_replace_advancedCommandsProvider()
    {
        DisplayName = "Snippet Replace Advanced";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        _commands = [
            new CommandItem(new cmdpal_snippet_replace_advancedPage()) { Title = DisplayName },
        ];
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }

}
