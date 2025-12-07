using ChatEngine.Storage.Config;
using NuggetMod.Enum.Metamod;
using NuggetMod.Interface;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatEngine.Storage;

[JsonSerializable(typeof(Root))]
[JsonSerializable(typeof(Config.Censor))]
[JsonSerializable(typeof(LanguageConfig))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Metadata | JsonSourceGenerationMode.Serialization)]
internal partial class ConfigContext : JsonSerializerContext { }

internal class ConfigManager
{
    private const string CONFIG_FILE = "addons/chatengine/config.json";
    private HashSet<char>? _ignoreCharSet;

    private static ConfigManager? _instance;

    public HashSet<char> IgnoreCharSet
    {
        get
        {
            _ignoreCharSet ??= [.. Config.Censor.IgnoreCharacters
                .Where(s => !string.IsNullOrEmpty(s) && s.Length == 1)
                .Select(s => s[0])];
            return _ignoreCharSet;
        }
    }
    public static ConfigManager Instance
    {
        get
        {
            _instance ??= new ConfigManager();
            return _instance;
        }
    }
    public Root Config { get; private set; }
    private ConfigManager()
    {
        string path = Path.Combine(MetaMod.MetaUtilFuncs.GetGameInfo(GetGameInfoType.GameDirectory), CONFIG_FILE);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            Config = JsonSerializer.Deserialize(json, ConfigContext.Default.Root) ?? new Root();
        }
        else
        {
            Config = new Root();
            SaveConfig();
        }
    }
    public void SaveConfig()
    {
        string path = Path.Combine(MetaMod.MetaUtilFuncs.GetGameInfo(GetGameInfoType.GameDirectory), CONFIG_FILE);
        string? dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir) && dir != null)
            Directory.CreateDirectory(dir);
        string json = JsonSerializer.Serialize(Config, ConfigContext.Default.Root);
        File.WriteAllText(path, json);
    }
}