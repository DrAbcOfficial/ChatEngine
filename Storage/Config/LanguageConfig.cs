namespace ChatEngine.Storage.Config;

internal class LanguageConfig
{
    public Dictionary<string, string> Lang { get; set; } = new Dictionary<string, string>
    {
        {"plugin_reloaded", "Chat插件已被重新加载，插件将暂时停用" },
        {"plugin_loaded", "Chat插件已成功加载" },
        {"player_kicked", "你已被服务器踢出，请文明游戏" },
        {"player_banned", "你已被服务器封禁{0}分钟，请文明游戏" },
        {"player_banwarn", "已检测到 {0} 次敏感词汇，达到上限将封禁" },
        {"player_leaved", "玩家: {0} 已经退出游戏，信息如下\nIP: {1}\nSteamID: {2}" },
        {"storage_loaeded", "已从数据库中读取{0}个封禁用户" },

        {"command_forbidden", "你没有使用该命令的权限，至少需要[{0}]的权限"},
        {"command_registed", "客户端命令：{0} 已经成功注册" },
        {"command_exec_success", "成功执行客户端命令：{0}" },
        {"command_exec_failed", "无法执行客户端命令：{0}，请参考命令帮助信息" },

        {"command_help", "所有可用命令如下：" },
        {"command_help_command", "命令" },
        {"command_help_description", "描述" },
        {"command_help_arguments", "参数" },
        {"command_help_admin", "权限" }
    };
}
