using ChatEngine.Lang;
using Metamod.Interface;

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

    protected BaseMetaModCommand(string name, string description, List<ArgumentsDescriptor>? arguments = null)
    {
        _name = name;
        _description = description;
        if (arguments != null)
            _arguments = [.. arguments];
        else
            _arguments = [];
    }
    private void PreExcute()
    {
        List<MetaModArgument> arguments = [];
        for(int i = 1; i < MetaMod.EngineFuncs.Cmd_Argc(); i++)
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
        if(arguments.Count < required)
        {
            Language.Print("args_mismatched", Language.PrintTarget.Server);
            return;
        }
        int max = Math.Max(arguments.Count, _arguments.Count);
        for(int i = 0; i < max; i++)
        {
            if (arguments[i].Type != _arguments[i].Type)
            {
                Language.Print("args_mismatched", Language.PrintTarget.Server);
                return;
            }
        }
        Excute(arguments, Language.PrintTarget.Server);
    }

    protected abstract bool Excute(List<MetaModArgument> arguments, Language.PrintTarget printTarget);

    internal static Dictionary<string, BaseMetaModCommand> Commands = [];

    private static List<BaseMetaModCommand> _register = [
        new CommandHelp("help", "cmd_help_des")
    ];
    internal static void RegisterCommands()
    {
        foreach(var command in _register)
        {
            Commands.Add(command.Name, command);
            MetaMod.EngineFuncs.AddServerCommand(command.Name, command.PreExcute);
        }
    }
}
