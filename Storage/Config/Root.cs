namespace ChatEngine.Storage.Config;

internal class Root
{
    public Censor Censor { get; set; } = new();
    public LanguageConfig Language { get; set; } = new();

    public List<string> ChatTrigger { get; set; } = ["!", "/", "\\"];
    //SQL储存路径
    public string SQLStoragePath { get; set; } = "addons/chatengine/chatengine.db";
    public string CommandPrefix { get; set; } = "cte_";
    //玩家退出后是否向所有人在控制台展示其信息
    //0 不展示
    //1 展示SteamID
    //2 展示SteamID和IP
    public int ShowPlayerInfoOnDisconnect { get; set; } = 1;
}
