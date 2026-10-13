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
* **Message Content** - invite and spam filters, message edit and delete logs, `@Head Non-Sub` replies

### Bot permissions

The bot role only needs these, permission integer `671214784`.

Permission | Used for
--- | ---
View Channels | Everything
Send Messages | Command replies and logs
Embed Links | `/help`, `/random` and mod log embeds
Attach Files | `/image`, `/says`, `/warm` and `/emoji`
Read Message History | `/xathz react`
Add Reactions | `/dating`, `/executie` and `/xathz`
Manage Messages | Removing Discord invites and animated emote spam
Manage Nicknames | The bong trap
Manage Webhooks | `/xathz puppet`
View Audit Log | Who banned a user and why in the mod log

## Running

`Watchdog.ps1` starts the bot and restarts it if it stops. Exit codes:

Exit code | Meaning | Watchdog
--- | --- | ---
`0` | Stopped normally | Restarts it
`1` | Lost the connection to Discord and did not reconnect within 10 minutes, or failed to start | Restarts it
`2` | Needs fixing, like a bad settings file, a token Discord rejects or missing privileged intents | Waits

On exit code `2` the bot writes `HeadNonSub/HeadNonSub.failfast` with the reason. The watchdog does not start the bot while that file exists. Fix the problem then delete the file. Starting the bot by hand also removes it.
