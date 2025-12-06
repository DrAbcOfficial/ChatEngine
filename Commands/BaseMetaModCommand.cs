using ChatEngine.Lang;
using ChatEngine.Storage;
using NuggetMod.Interface;
using NuggetMod.Wrapper.Engine;

namespace ChatEngine.Commands;

internal abstract class BaseMetaModCommand
{
    protected string _name;
    protected string _description;
    protected List<ArgumentsDescriptor> _arguments;
    protected bool _admin;

    internal string Name => _name;
    internal string Description => _description;
    internal List<ArgumentsDescriptor> Arguments => _arguments;
    internal bool Admin => _admin;

    protected BaseMetaModCommand(string name, string description, List<ArgumentsDescriptor>? arguments = null, bool admin = false)
    {
        _name = name;
        _description = description;
        if (arguments != null)
            _arguments = [.. arguments];
        else
            _arguments = [];
        _admin = admin;
    }
    private void ServerPreExcute()
    {
        List<MetaModArgument> arguments = [];
        for (int i = 1; i < MetaMod.EngineFuncs.Cmd_Argc(); i++)
        {
            string input = MetaMod.EngineFuncs.Cmd_Argv(i);
            if (string.IsNullOrWhiteSpace(input))
            {
                arguments.Add(new(input));
                continue;
            }
            string trimmed = input.Trim();
            if (bool.TryParse(trimmed, out bool b))
                arguments.Add(new(b));
            else if (int.TryParse(trimmed, out int d))
                arguments.Add(new(d));
            else if (float.TryParse(trimmed, out float f))
                arguments.Add(new(f));
            else
                arguments.Add(new(trimmed));
        }
        int required = _arguments.Count(x => { return !x.Optional; });
        if (arguments.Count < required)
        {
            Language.PrintWithLang("command.exec.failed", Language.PrintTarget.Server, null, Name);
            return;
        }
        Excute(arguments, Language.PrintTarget.Server);
    }
    internal bool ClientPreExcute(string[] args, Edict player, bool chat)
    {
        Language.PrintTarget target = chat ? Language.PrintTarget.ClientChat : Language.PrintTarget.ClientConsole;
        var playerinfo = PlayerInfo.GetPlayerInfo(player);
        if (playerinfo == null)
            return false;
        if (Admin && playerinfo.Admin == Enum.Admin.Player)
        {
            Language.PrintWithLang("command.forbidden", target, player, Enum.Admin.Admin);
            return false;
        }
        List<MetaModArgument> arguments = [];
        foreach (string input in args)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                arguments.Add(new(input));
                continue;
            }
            string trimmed = input.Trim();
            if (bool.TryParse(trimmed, out bool b))
                arguments.Add(new(b));
            else if (int.TryParse(trimmed, out int d))
                arguments.Add(new(d));
            else if (float.TryParse(trimmed, out float f))
                arguments.Add(new(f));
            else
                arguments.Add(new(trimmed));
        }
        int required = _arguments.Count(x => { return !x.Optional; });
        if (arguments.Count < required)
            return false;
        return Excute(arguments, target, player);
    }

    protected abstract bool Excute(List<MetaModArgument> arguments, Language.PrintTarget printTarget, Edict? player = null);

    internal static Dictionary<string, BaseMetaModCommand> Commands = [];

    private static readonly List<BaseMetaModCommand> _register = [
        new CommandHelp("help", "command.help.cmd_description"),
        new CommandBan("ban", "command.ban.description", [
            new ArgumentsDescriptor("SteamID", "command.ban.steamid.description"),
            new ArgumentsDescriptor("Time", "command.ban.bantime.description"),
            new ArgumentsDescriptor("Reason", "command.ban.reason.description", true),
        ]),
        new CommandRemoveBan("removeban", "command.removeban.description", [
            new ArgumentsDescriptor("SteamID", "command.removeban.steamid.description")
        ]),
        new CommandKick("kick", "command.kick.description", [
            new ArgumentsDescriptor("SteamID", "command.kick.steamid.description"),
            new ArgumentsDescriptor("Reason", "command.kick.reason.description", true),
        ]),
        new CommandGag("gag", "command.gag.description", [
            new ArgumentsDescriptor("SteamID", "command.gag.steamid.description"),
            new ArgumentsDescriptor("Time", "command.gag.gagtime.description"),
            new ArgumentsDescriptor("Reason", "command.gag.reason.description", true),
        ]),
        new CommandRemoveGag("removegag", "command.removegag.description", [
            new ArgumentsDescriptor("SteamID", "command.removegag.steamid.description")
        ]),
    ];
    internal static void RegisterCommands()
    {
        string prefix = ConfigManager.Instance.Config.CommandPrefix;
        foreach (var command in _register)
        {
            Commands.Add(command.Name, command);
            MetaMod.EngineFuncs.AddServerCommand($"{prefix}{command.Name}", command.ServerPreExcute);
        }
    }
}
