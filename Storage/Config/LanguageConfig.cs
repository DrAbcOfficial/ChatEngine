namespace ChatEngine.Storage.Config;

internal class LanguageConfig
{
    public Dictionary<string, string> Lang { get; set; } = new Dictionary<string, string>
    {
        {"yes", "是"},
        {"no", "否" },
        {"plugin.reloaded", "Chat插件已被重新加载，插件将暂时停用" },
        {"plugin.loaded", "Chat插件已成功加载" },
        {"player.kicked", "你已被服务器踢出，请文明游戏" },
        {"player.banned", "你已被服务器封禁{0}分钟，请文明游戏" },
        {"player.garded", "你已被服务器禁言{0}分钟，请文明游戏" },
        {"player.banwarn", "已检测到 {0} 次敏感词汇，达到上限将封禁" },
        {"player.leaved", "玩家: {0} 已经退出游戏，信息如下\nIP: {1}\nSteamID: {2}" },
        {"player.banned.connect", "你已被服务器封禁，解封时间: {0}" },

        {"command.forbidden", "你没有使用该命令的权限，至少需要[{0}]的权限"},
        {"command.registed", "客户端命令：{0} 已经成功注册" },
        {"command.exec.success", "成功执行客户端命令：{0}" },
        {"command.exec.failed", "无法执行客户端命令：{0}，请参考命令帮助信息" },

        {"command.help", "所有可用命令如下：" },
        {"command.help.command", "命令" },
        {"command.help.description", "描述" },
        {"command.help.arguments", "参数" },
        {"command.help.admin", "权限" }
    };
}
