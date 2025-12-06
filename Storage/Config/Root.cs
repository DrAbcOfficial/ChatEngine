namespace ChatEngine.Storage.Config;

internal class Root
{
    public Censor Censor { get; set; } = new();
    public LanguageConfig Language { get; set; } = new();

    public List<string> ChatTrigger { get; set; } = ["!", "/", "\\"];
}
