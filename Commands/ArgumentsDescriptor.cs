using ChatEngine.Commands.Enum;

namespace ChatEngine.Commands;

internal class ArgumentsDescriptor(ArgumentType type, string name, string description, bool optional = false)
{
    protected ArgumentType _type = type;
    protected bool _optional = optional;
    protected string _name = name;
    protected string _description = description;

    internal ArgumentType Type => _type;
    internal bool Optional => _optional;
    internal string Name => _name;
    internal string Description => _description;
}
