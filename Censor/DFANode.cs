using ChatEngine.Storage;
using System.Text;

namespace ChatEngine.Censor;

internal class DFANode(char key, bool terminal)
{
    private readonly char _key = key;
    private readonly Dictionary<char, DFANode> _children = [];
    private readonly bool _terminal = terminal;

    internal char Key => _key;
    internal bool IsTerminal => _terminal;
    internal DFANode Get(char key) => _children[key];
    internal bool Exists(char key) => _children.ContainsKey(key);
    internal bool TryGetValue(char key, out DFANode? value) => _children.TryGetValue(key, out value);
    internal void Insert(DFANode node)
    {
        _children[node.Key] = node;
    }

    private static string Preprocess(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var sb = new StringBuilder(input.Length);
        foreach (char c in input.ToLower())
        {
            if (!ConfigManager.Instance.IgnoreCharSet.Contains(c))
                sb.Append(c);
        }
        return sb.ToString().Trim();
    }

    internal static (DFANode, uint, uint) CreateFromFile(string path)
    {
        DFANode root = new('\0', false);
        uint iCount = 0;
        uint iTerminal = 0;
        using StreamReader sr = new(path);
        while (!sr.EndOfStream)
        {
            DFANode reader = root;
            string line = Preprocess(sr.ReadLine());
            if (string.IsNullOrEmpty(line))
                continue;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (reader.Exists(c))
                    reader = reader.Get(c);
                else
                {
                    bool terminal = i == line.Length - 1;
                    DFANode nNode = new(c, terminal);
                    reader.Insert(nNode);
                    reader = nNode;
                    iCount++;
                    if (terminal)
                        iTerminal++;
                }
            }
        }
        return (root, iCount, iTerminal);
    }

    internal struct CheckResult
    {
        internal int Index;
        internal int Length;
        internal string Matched;
    }
    internal CheckResult[] CheckString(string str)
    {
        str = Preprocess(str);
        if (string.IsNullOrEmpty(str)) return [];

        var results = new List<CheckResult>();
        for (int i = 0; i < str.Length; i++)
        {
            var current = this;
            int bestEnd = -1;
            for (int j = i; j < str.Length; j++)
            {
                if (!current.TryGetValue(str[j], out var next))
                    break;
                current = next;
                if (current!.IsTerminal)
                    bestEnd = j + 1;
            }
            if (bestEnd != -1)
            {
                results.Add(new CheckResult
                {
                    Index = i,
                    Length = bestEnd - i,
                    Matched = str[i..bestEnd]
                });
            }
        }
        return [.. results];
    }
}
