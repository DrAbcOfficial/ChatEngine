using ChatEngine.Storage.Config;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatEngine.Storage;

[JsonSerializable(typeof(Root))]
[JsonSerializable(typeof(Censor))]
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
    private static ConfigManager? _instance;
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
        if (File.Exists(CONFIG_FILE))
        {
            string json = File.ReadAllText(CONFIG_FILE);
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
        string json = JsonSerializer.Serialize(Config, ConfigContext.Default.Root);
        File.WriteAllText(CONFIG_FILE, json);
    }
}