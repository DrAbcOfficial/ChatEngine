using ChatEngine.Commands.Enum;
using Microsoft.Data.Sqlite;
using NuggetMod.Enum.Metamod;
using NuggetMod.Interface;
using System.Data;

namespace ChatEngine.Storage;

internal class SQL
{
    private string _connectionString = string.Empty;
    public void Initialize(string dbPath)
    {
        string gamedir = MetaMod.MetaUtilFuncs.GetGameInfo(GetGameInfoType.GameDirectory);
        string path = Path.Combine(gamedir, dbPath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        _connectionString = $"Data Source={path};";

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        // 创建所有表（IF NOT EXISTS）
        ExecuteNonQuery(connection, transaction, @"
            CREATE TABLE IF NOT EXISTS player_info (
                SteamID TEXT PRIMARY KEY,
                NickName TEXT NOT NULL DEFAULT '',
                IP TEXT NOT NULL DEFAULT '',
                Banned TEXT,
                Gagged TEXT,
                Talked INTEGER NOT NULL DEFAULT 0,
                Admin INTEGER NOT NULL DEFAULT 0,
                Flags TEXT NOT NULL DEFAULT ''
            );");

        ExecuteNonQuery(connection, transaction, @"
            CREATE TABLE IF NOT EXISTS chat_log (
                SteamID TEXT NOT NULL,
                Time TEXT NOT NULL,
                Type INTEGER NOT NULL,
                Content TEXT NOT NULL
            );");

        ExecuteNonQuery(connection, transaction, @"
            CREATE TABLE IF NOT EXISTS ban_log (
                SteamID TEXT NOT NULL,
                Operator INTEGER,
                Time TEXT NOT NULL,
                Until TEXT NOT NULL
            );");

        ExecuteNonQuery(connection, transaction, @"
            CREATE TABLE IF NOT EXISTS gag_log (
                SteamID TEXT NOT NULL,
                Operator INTEGER,
                Time TEXT NOT NULL,
                Until TEXT NOT NULL
            );");

        ExecuteNonQuery(connection, transaction, @"
            CREATE TABLE IF NOT EXISTS detected_log (
                SteamID TEXT NOT NULL,
                Time TEXT NOT NULL,
                Content TEXT NOT NULL,
                Detected TEXT NOT NULL
            );");

        transaction.Commit();
    }

    private static void ExecuteNonQuery(SqliteConnection connection, SqliteTransaction transaction, string sql)
    {
        using var command = new SqliteCommand(sql, connection)
        {
            Transaction = transaction
        };
        command.ExecuteNonQuery();
    }

    public void UpsertPlayerInfo(PlayerInfo player)
    {
        if (player.NickName.Length > 64)
            throw new ArgumentException("NickName exceeds 64 characters.", nameof(player.NickName));

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO player_info (SteamID, NickName, IP, Banned, Gagged, Talked, Admin, Flags)
            VALUES ($steamId, $nickName, $ip, $banned, $gagged, $talked, $admin, $flags)
            ON CONFLICT(SteamID) DO UPDATE SET
                NickName = excluded.NickName,
                IP = excluded.IP,
                Banned = excluded.Banned,
                Gagged = excluded.Gagged,
                Talked = excluded.Talked,
                Admin = excluded.Admin,
                Flags = excluded.Flags;";

        cmd.Parameters.AddWithValue("$steamId", player.SteamID);
        cmd.Parameters.AddWithValue("$nickName", player.NickName);
        cmd.Parameters.AddWithValue("$ip", player.IP);
        cmd.Parameters.AddWithValue("$banned", player.BannedUntil?.ToString("yyyy-MM-ddTHH:mm:ss") ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$gagged", player.GaggedUntil?.ToString("yyyy-MM-ddTHH:mm:ss") ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$talked", player.TalkedCount);
        cmd.Parameters.AddWithValue("$admin", (int)player.Admin);
        cmd.Parameters.AddWithValue("$flags", string.Join(",", player.Flags));

        cmd.ExecuteNonQuery();
    }

    public PlayerInfo GetOrCreatePlayerInfo(string steamId, string initialNickName, string ip)
    {
        var dbPlayer = GetPlayerInfo(steamId);
        if (dbPlayer != null)
            return dbPlayer;
        var newPlayer = new PlayerInfo
        {
            SteamID = steamId,
            NickName = initialNickName,
            IP = ip,
            BannedUntil = null,
            GaggedUntil = null,
            TalkedCount = 0,
            Admin = Admin.Player,
            Flags = []
        };
        UpsertPlayerInfo(newPlayer);
        return newPlayer;
    }
    public PlayerInfo? GetPlayerInfo(string steamId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT NickName, IP, Banned, Gagged, Talked, Admin, Flags FROM player_info WHERE SteamID = $steamId;";
        cmd.Parameters.AddWithValue("$steamId", steamId);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            return null;

        return new PlayerInfo
        {
            SteamID = steamId,
            NickName = reader.GetString("NickName"),
            IP = reader.GetString("IP"),
            BannedUntil = ParseDateTime(reader, "Banned"),
            GaggedUntil = ParseDateTime(reader, "Gagged"),
            TalkedCount = reader.GetInt64("Talked"),
            Admin = (Admin)reader.GetInt32("Admin"),
            Flags = ParseFlags(reader.GetString("Flags"))
        };
    }
    private static DateTime? ParseDateTime(SqliteDataReader reader, string columnName)
    {
        var value = reader[columnName];
        return value == DBNull.Value ? null : DateTime.Parse((string)value);
    }

    private static string[] ParseFlags(string flagsStr)
    {
        return string.IsNullOrEmpty(flagsStr) ? [] : flagsStr.Split(',');
    }

    public void LogChat(string steamId, string content, bool say_team)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO chat_log (SteamID, Time, Type, Content)
            VALUES ($steamId, $time, $type, $content);";

        cmd.Parameters.AddWithValue("$steamId", steamId);
        cmd.Parameters.AddWithValue("$time", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"));
        cmd.Parameters.AddWithValue("$type", say_team ? 1 : 0);
        cmd.Parameters.AddWithValue("$content", content);

        cmd.ExecuteNonQuery();
    }

    public void LogBan(string steamId, string operatorId, DateTime until)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO ban_log (SteamID, Operator, Time, Until)
            VALUES ($steamId, $operator, $time, $until);";

        cmd.Parameters.AddWithValue("$steamId", steamId);
        cmd.Parameters.AddWithValue("$operator", operatorId);
        cmd.Parameters.AddWithValue("$time", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"));
        cmd.Parameters.AddWithValue("$until", until.ToString("yyyy-MM-ddTHH:mm:ss"));

        cmd.ExecuteNonQuery();
    }

    public void LogGag(string steamId, string operatorId, DateTime until)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO gag_log (SteamID, Operator, Time, Until)
            VALUES ($steamId, $operator, $time, $until);";

        cmd.Parameters.AddWithValue("$steamId", steamId);
        cmd.Parameters.AddWithValue("$operator", operatorId);
        cmd.Parameters.AddWithValue("$time", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"));
        cmd.Parameters.AddWithValue("$until", until.ToString("yyyy-MM-ddTHH:mm:ss"));

        cmd.ExecuteNonQuery();
    }
}
