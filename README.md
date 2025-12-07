# ChatEngine

A comprehensive chat management and moderation plugin for Half-Life/GoldSrc servers, built with NuggetMod framework. This plugin provides advanced chat filtering, player management, and administrative tools to maintain a healthy gaming environment.

## Features

- **Profanity Filter**: DFA-based (Deterministic Finite Automaton) word detection with customizable dictionary
- **Auto-Moderation**: Automatic ban system for repeated violations
- **Player Management**: Ban, kick, and gag commands with persistent storage
- **Chat Logging**: Complete chat history tracking with SQLite database
- **Admin System**: Role-based permission system
- **Name Filtering**: Prevents players from joining with inappropriate usernames


## Installation

1. Download the latest release
2. Extract files to your game server directory:
   ```
   <game_directory>/
   ├── addons/
   │   └── chatengine/
   │       ├── dlls/
   │       │   └── e_sqlite3.dll (or .so/.dylib)
   │       ├── config.json
   │       └── censor_words.dic
   ```
3. Configure the plugin (see Configuration section)
4. Restart your server

## Configuration

The main configuration file is located at `addons/chatengine/config.json`.

### Configuration Structure

```json
{
  "censor": {
    "ignoreCharacters": [" ", "\t", "#", "!", "@", ...],
    "niceWords": ["I love this server", "Great game!", ...],
    "censorReplacement": "*",
    "maxLimitPerGame": 5,
    "banDurationMinutes": 30,
    "censorWordsFilePath": "addons/chatengine/censor_words.dic"
  },
  "language": {
    "lang": {
      "yes": "Yes",
      "no": "No",
      "player.banned": "You have been banned for {0} minutes",
      ...
    }
  },
  "chatTrigger": ["!", "/", "\\"],
  "sqlStoragePath": "addons/chatengine/chatengine.db",
  "commandPrefix": "cte_",
  "showPlayerInfoOnDisconnect": 1
}
```

### Configuration Options

#### Censor Settings

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `ignoreCharacters` | string[] | `[" ", "\t", "#", ...]` | Characters to ignore during profanity detection |
| `niceWords` | string[] | `["I love this server", ...]` | Replacement phrases for censored messages |
| `censorReplacement` | string | `"*"` | Character used to replace profanity |
| `maxLimitPerGame` | int | `5` | Maximum violations before auto-ban |
| `banDurationMinutes` | int | `30` | Auto-ban duration in minutes |
| `censorWordsFilePath` | string | `"addons/chatengine/censor_words.dic"` | Path to profanity dictionary file |

#### General Settings

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `chatTrigger` | string[] | `["!", "/", "\\"]` | Prefixes that trigger command parsing in chat |
| `sqlStoragePath` | string | `"addons/chatengine/chatengine.db"` | SQLite database file path |
| `commandPrefix` | string | `"cte_"` | Server console command prefix |
| `showPlayerInfoOnDisconnect` | int | `1` | Display player info on disconnect (0=off, 1=SteamID, 2=SteamID+IP) |

### Profanity Dictionary

Create a `censor_words.dic` file with one word/phrase per line:
```
badword1
badword2
...
```

The DFA algorithm will automatically handle variations with ignored characters.

## Commands

All commands can be executed from:
- **Server console**: `<commandPrefix><command>`
- **Client console**: `<commandPrefix><command>`
- **In-game chat**: `!<commandPrefix><command>` (or other configured triggers)

### Available Commands

| Command | Arguments | Permission | Description |
|---------|-----------|------------|-------------|
| `help` | - | Player | Display all available commands |
| `ban` | `<SteamID> <Minutes> [Reason]` | Admin | Ban a player (use -1 for permanent) |
| `removeban` | `<SteamID>` | Admin | Remove a player's ban |
| `kick` | `<SteamID> [Reason]` | Admin | Kick a player from the server |
| `gag` | `<SteamID> <Minutes> [Reason]` | Admin | Mute a player (use -1 for permanent) |
| `removegag` | `<SteamID>` | Admin | Remove a player's gag |

### Command Examples

**Server Console:**
```
cte_ban STEAM_0:1:12345678 60 "Spamming"
cte_kick STEAM_0:1:87654321 "Inappropriate behavior"
cte_gag STEAM_0:1:11111111 30
cte_removeban STEAM_0:1:12345678
```

**In-Game Chat:**
```
!cte_help
!cte_ban STEAM_0:1:12345678 60 Spamming
!cte_kick STEAM_0:1:87654321
```

**Client Console:**
```
cte_help
cte_ban STEAM_0:1:12345678 60 "Toxic behavior"
```

## Permission System

The plugin uses a role-based permission system:

| Role | Value | Description |
|------|-------|-------------|
| `Player` | 0 | Default role, can use basic commands |
| `Admin` | 1 | Can use moderation commands (ban, kick, gag) |

Admin permissions are stored in the SQLite database (`player_info` table, `Admin` column).

## Database Schema

The plugin uses SQLite with the following tables:

### player_info
Stores player profiles and moderation status.
```sql
SteamID TEXT PRIMARY KEY
NickName TEXT
IP TEXT
Banned TEXT (ISO 8601 datetime)
Gagged TEXT (ISO 8601 datetime)
Talked INTEGER (message count)
Admin INTEGER (permission level)
Flags TEXT (comma-separated)
```

### chat_log
Records all chat messages.
```sql
SteamID TEXT
Time TEXT (ISO 8601 datetime)
Type INTEGER (0=all, 1=team)
Content TEXT
```

### ban_log / gag_log
Tracks moderation actions.
```sql
SteamID TEXT
Operator TEXT (admin SteamID)
Time TEXT (ISO 8601 datetime)
Until TEXT (ISO 8601 datetime)
```

### detected_log
Logs profanity detections.
```sql
SteamID TEXT
Time TEXT (ISO 8601 datetime)
Type INTEGER (0=chat, 1=name)
Content TEXT (original message)
Detected TEXT (matched words)
```

## Troubleshooting

### Plugin fails to load
- Ensure `e_sqlite3.dll` (or `.so`/`.dylib`) is in `addons/chatengine/dlls/`
- Check server console for error messages

### Profanity filter not working
- Confirm `censor_words.dic` exists and contains words (one per line)
- Check `censorWordsFilePath` in config.json
- Review `ignoreCharacters` - too many may cause false negatives

### Commands not responding
- Verify `commandPrefix` matches your usage
- Check `chatTrigger` array for in-game chat commands

### Database errors
- Ensure `sqlStoragePath` directory exists and is writable
- Check file permissions on the `.db` file
- Verify SQLite library is loaded correctly

## Development

### Requirment

- .NET 10 SDK
- NuggetMod framework

### Building from Source

```bash
# !!! USE AOT PUBLISH !!!
dotnet publish -c Release -r win-x86 -o ./build -p:PublishAot=true
```

## Credits

- Built with [NuggetMod](https://github.com/DrAbcrealone/NuggetMod) framework
- Uses SQLitePCL for database operations
---

**Note**: This plugin is designed for Half-Life/GoldSrc servers. Ensure compatibility with your specific game mod and server configuration.
