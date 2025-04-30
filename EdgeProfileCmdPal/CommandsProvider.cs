using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace EdgeProfileCmdPal;

public partial class CommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;

    public CommandsProvider()
    {
        DisplayName = "Open Edge by Profile";
        Icon = IconHelpers.FromRelativePath(@"Assets\EdgeProfile.png");
        _commands = [
            new CommandItem(new ProfileList()) { Title = DisplayName },
        ];
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }

}
