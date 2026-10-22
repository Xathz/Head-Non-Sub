<p align="center"><img  alt="Head Non-Sub" src="./ProfilePicture128.png"></p>

<div align="center">Non-subs are the worst, they come in here and act like they own the place. At least I am the head of them all.</div>

Made for the [PaymoneyWubby](https://www.twitch.tv/paymoneywubby) [discord](https://discordapp.com/invite/wubby) server

[![Discord server](https://img.shields.io/discord/328300333010911242.svg)](https://discordapp.com/invite/wubby)

## Features
* Trash

## Discord Setup

### Privileged gateway intents

Enable these in the [Discord Developer Portal](https://discord.com/developers/applications) under **Bot**.

* **Server Members** - member list, join and leave logs, user options in commands
* **Message Content** - invite and spam filters, message edit and delete logs, `@Head Non-Sub` replies, `!` aliases of slash commands like `!ttsays`

### Bot permissions

The bot role only needs these, permission integer `671214784`.

Permission | Used for
--- | ---
View Channels | Everything
Send Messages | Command replies and logs
Embed Links | `/help`, `/random` and mod log embeds
Attach Files | `/image`, `/says` and its `!` aliases, `/warm` and `/emoji`
Read Message History | `/xathz react`, replying to `!` commands
Add Reactions | `/dating`, `/executie` and `/xathz`
Manage Messages | Removing Discord invites and animated emote spam
Manage Nicknames | The bong trap
Manage Webhooks | `/xathz puppet`
View Audit Log | Who banned a user and why in the mod log

## Running

`Watchdog.ps1` starts the bot and restarts it when it stops, up to 3 times. It needs [PowerShell 7.4 or newer](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) and the .NET 10 runtime. Put it next to `HeadNonSub.dll` and run:

```powershell
pwsh ./Watchdog.ps1
```

Option | Default | Meaning
--- | --- | ---
`-MaxRestarts` | `3` | Restarts before the watchdog gives up and exits
`-RestartDelaySeconds` | `15` | Wait before each restart

The bot runs in the same console, so its console commands (`help`, `cache`, `exit`) still work. The watchdog will not start a second copy if the bot is already running.

What the watchdog does with the bot's exit code:

Exit code | Meaning | Watchdog
--- | --- | ---
`0` | Stopped normally, like the `exit` console command | Exits
`1` | Lost the connection to Discord and did not reconnect within 10 minutes, or failed to start | Restarts it, until the limit
`2` | Needs fixing, like a bad settings file, a token Discord rejects or missing privileged intents | Exits, the reason is in the bot's log
Anything else | Crashed | Restarts it, until the limit
