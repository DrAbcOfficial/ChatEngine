using ChatEngine.Commands.Enum;

namespace ChatEngine.Commands;

internal class MetaModArgument
{
    protected object _value;
    protected ArgumentType _type;

    internal ArgumentType Type => _type;

    internal MetaModArgument(string str)
    {
        _value = str;
        _type = ArgumentType.String;
    }
    internal MetaModArgument(bool b)
    {
        _value = b;
        _type = ArgumentType.Bool;
    }
    internal MetaModArgument(int i)
    {
        _value = i;
        _type = ArgumentType.Int;
    }
    internal MetaModArgument(float f)
    {
        _value = f;
        _type = ArgumentType.Float;
    }

    public static implicit operator string(MetaModArgument obj)
    {
        return obj._type switch
        {
            ArgumentType.Bool => ((bool)obj._value).ToString(),
            ArgumentType.String => ((string)obj._value).ToString(),
            ArgumentType.Int => ((int)obj._value).ToString(),
            ArgumentType.Float => ((float)obj._value).ToString(),
            _ => obj._value.ToString() ?? throw new ArgumentException("Unsupportted implicit convert"),
        };
    }

    public static implicit operator int(MetaModArgument obj)
    {
        return obj._type switch
        {
            ArgumentType.Int => (int)obj._value,
            ArgumentType.Float => (int)(float)obj._value,
            ArgumentType.Bool => (bool)obj._value ? 1 : 0,
            ArgumentType.String => int.Parse((string)obj._value),
            _ => throw new ArgumentException("Unsupportted implicit convert"),
        };
    }

    public static implicit operator float(MetaModArgument obj)
    {
        return obj._type switch
        {
            ArgumentType.Int => (int)obj._value,
            ArgumentType.Float => (float)obj._value,
            ArgumentType.Bool => (bool)obj._value ? 1 : 0,
            ArgumentType.String => float.Parse((string)obj._value),
            _ => throw new ArgumentException("Unsupportted implicit convert"),
        };
    }

    public static implicit operator bool(MetaModArgument obj)
    {
        return obj._type switch
        {
            ArgumentType.Int => (int)obj._value > 0,
            ArgumentType.Float => (float)obj._value > 0,
            ArgumentType.Bool => (bool)obj._value,
            ArgumentType.String => bool.Parse((string)obj._value),
            _ => throw new ArgumentException("Unsupportted implicit convert"),
        };
    }
}
