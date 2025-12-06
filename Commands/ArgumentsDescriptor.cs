namespace ChatEngine.Commands;

internal class ArgumentsDescriptor(string name, string description, bool optional = false)
{
    protected bool _optional = optional;
    protected string _name = name;
    protected string _description = description;

    internal bool Optional => _optional;
    internal string Name => _name;
    internal string Description => _description;
}
