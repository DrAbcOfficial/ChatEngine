namespace ChatEngine.Storage.Config;

internal class Root
{
    public Censor Censor { get; set; } = new();
    public LanguageConfig Language { get; set; } = new();

    public List<string> ChatTrigger { get; set; } = ["!", "/", "\\"];
    //SQL储存路径
    public string SQLStoragePath { get; set; } = "addons/chatengine/chatengine.db";
    public string CommandPrefix { get; set; } = "cte_";
}
