using Microsoft.Data.Sqlite;
using System.Data;

namespace ChatEngine.Storage;

internal class SQL
{
    private string _connectionString = string.Empty;
    public void Initialize(string dbPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        _connectionString = $"Data Source={dbPath};";

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        // 创建所有表（IF NOT EXISTS）
        ExecuteNonQuery(connection, @"
            CREATE TABLE IF NOT EXISTS player_info (
                SteamID INTEGER PRIMARY KEY,
                NickName TEXT NOT NULL,
                Banned TEXT,
                Garged TEXT,
                Talked INTEGER NOT NULL DEFAULT 0,
                Admin INTEGER NOT NULL DEFAULT 0,
                Flags TEXT NOT NULL DEFAULT ''
            );");

        ExecuteNonQuery(connection, @"
            CREATE TABLE IF NOT EXISTS chat_log (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SteamID INTEGER NOT NULL,
                Time TEXT NOT NULL,
                Content TEXT NOT NULL
            );");

        ExecuteNonQuery(connection, @"
            CREATE TABLE IF NOT EXISTS ban_log (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SteamID INTEGER NOT NULL,
                Operator INTEGER,
                Time TEXT NOT NULL,
                Until TEXT NOT NULL
            );");

        ExecuteNonQuery(connection, @"
            CREATE TABLE IF NOT EXISTS garg_log (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SteamID INTEGER NOT NULL,
                Operator INTEGER,
                Time TEXT NOT NULL,
                Until TEXT NOT NULL
            );");

        ExecuteNonQuery(connection, @"
            CREATE TABLE IF NOT EXISTS detected_log (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SteamID INTEGER NOT NULL,
                Time TEXT NOT NULL,
                Content TEXT NOT NULL,
                Detected TEXT NOT NULL
            );");

        transaction.Commit();
    }

    private static void ExecuteNonQuery(SqliteConnection connection, string sql)
    {
        using var command = new SqliteCommand(sql, connection);
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
            INSERT INTO player_info (SteamID, NickName, Banned, Garged, Talked, Admin, Flags)
            VALUES ($steamId, $nickName, $banned, $garged, $talked, $admin, $flags)
            ON CONFLICT(SteamID) DO UPDATE SET
                NickName = excluded.NickName,
                Banned = excluded.Banned,
                Garged = excluded.Garged,
                Talked = excluded.Talked,
                Admin = excluded.Admin,
                Flags = excluded.Flags;";

        cmd.Parameters.AddWithValue("$steamId", player.SteamID);
        cmd.Parameters.AddWithValue("$nickName", player.NickName);
        cmd.Parameters.AddWithValue("$banned", player.BannedUntil?.ToString("yyyy-MM-ddTHH:mm:ss") ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$garged", player.GargedUntil?.ToString("yyyy-MM-ddTHH:mm:ss") ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$talked", player.TalkedCount);
        cmd.Parameters.AddWithValue("$admin", player.Admin ? 1 : 0);
        cmd.Parameters.AddWithValue("$flags", string.Join(",", player.Flags));

        cmd.ExecuteNonQuery();
    }

    public PlayerInfo GetOrCreatePlayerInfo(long steamId, string initialNickName = "Unknown")
    {
        var dbPlayer = GetPlayerInfo(steamId);
        if (dbPlayer != null)
            return dbPlayer;
        var newPlayer = new PlayerInfo
        {
            SteamID = steamId,
            NickName = initialNickName,
            BannedUntil = null,
            GargedUntil = null,
            TalkedCount = 0,
            Admin = false,
            Flags = []
        };
        UpsertPlayerInfo(newPlayer);
        return newPlayer;
    }
    public PlayerInfo? GetPlayerInfo(long steamId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT NickName, Banned, Garged, Talked, Admin, Flags FROM player_info WHERE SteamID = $steamId;";
        cmd.Parameters.AddWithValue("$steamId", steamId);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            return null;

        return new PlayerInfo
        {
            SteamID = steamId,
            NickName = reader.GetString("NickName"),
            BannedUntil = ParseDateTime(reader, "Banned"),
            GargedUntil = ParseDateTime(reader, "Garged"),
            TalkedCount = reader.GetInt64("Talked"),
            Admin = reader.GetBoolean("Admin"),
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
        return string.IsNullOrEmpty(flagsStr) ? Array.Empty<string>() : flagsStr.Split(',');
    }

    public void LogChat(long steamId, string content)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO chat_log (SteamID, Time, Content)
            VALUES ($steamId, $time, $content);";

        cmd.Parameters.AddWithValue("$steamId", steamId);
        cmd.Parameters.AddWithValue("$time", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"));
        cmd.Parameters.AddWithValue("$content", content);

        cmd.ExecuteNonQuery();
    }
}
