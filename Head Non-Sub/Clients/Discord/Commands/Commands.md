## Notes

### General

- All commands are slash commands, type `/` in Discord to see them
- Some commands also work the old way with `!`, like `!ttsays <text>`, these are listed under **Also** on the command
- Some commands have cooldowns either server wide or per-user
- Not all commands are listed, some are excluded here to prevent abuse
- Staff commands are hidden from members without the `Manage Messages` permission, server admins can change this in **Server Settings > Integrations**

### Access Scopes

Access | Scope / Description
--- | ---
All | Everyone
Sub | Users that have a `Twitch Subscriber` or `Patreon` role
Staff | **Built-in roles:** `Administrator`<br>**Guild (server) roles:** `Admins`, `Mods`, `Mod-Lite`<br>**Users:** Xathz
Xathz | Xathz only

## Commands

### Useful

Command | Options | Description | Access
--- | --- | --- | ---
`/help` | | A link to this document | All
`/ping` | | Check the delay from the bot to Discord | All
`/prices` | | Media share prices and info for [PaymoneyWubby](https://wub.by/ttv)'s stream | All
`/nonsub` | | How to get the subscriber role | All
`/stats true` | | Number of times `/image true` was used | All
`/stats says` | | Top `/says` templates and how many times they were used | All

### Images `/image <name>`

The name autocompletes as you type.

Name | Description | Access
--- | --- | ---
`1024nude` | [Scandalous](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/1024_nude.png) | All
`bigoof` | idk what [you were](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/big_oof.gif) expecting | All
`dab` | Why do peopl keep doing [this](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/dab.gif) | All
`dickdowndennis` | [Came twice](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/dickdowndennis.png) just typing this out | All
`fuckedup` | Not getting the [deposit](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/fucked_up.png) back. **Alias:** `ripdeposit` | All
`gimme` | :astonished: | All
`gongboy` | I should probability be banned for [this](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/gongboy.png) | All
`gottem` | gOtTheM [so hard](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/gottem.gif). **Alias:** `gotem` | All
`juan` | [Juan](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/juan.png) | All
`keyboard` | He dosn't even get [mad](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/wubbykeyboard.png) | All
`lucky` | [Lit and lucky](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/lucky.png) | All
`moneyshot` | Open wide [tt](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/moneyshot.png) | All
`night` | Big night sky with emotes | All
`potato` | Wubby... in [potato form](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/potato.png) | All
`radar` | Yes it really looks like [this](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/radar.png) | All
`relax` | Keeping holes [moist](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/relax.png) | All
`rnk` | A parody of [MEE6](https://mee6.xyz)'s `!rank` | All
`star` | May he [shine bright](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/star.gif) forever | All
`true` | [Speaking to the grass](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/true.png). **Alias:** `thatstrue` | All
`weebmas` | [Weebmas](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/wubbypadoru_xmas.gif) | All
`weebshit` | I have had it with these motherfucking [weebs](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/weebshit.png) in this motherfucking Discord! | All
`what` | [tt confuse](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/what.png) | All
`wubbycheeto` | [Aww](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/wubbycheeto.png) | All
`yum` | It was [so good](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/yum.png) | All

### Shitposting

Command | Options | Description | Access
--- | --- | --- | ---
`/says` | `who` `text` | Render text on a template. **Who:** `tt2468` (reflect upon something), `1024` (1024's screen), `Amanda` (engrave Amanda's plate), `Satazero` (now with icecream), `jiberjiber` (wet and waiting to reflect), `Strong Bad` (Strong Bad and his computer). **Also:** `!ttsays`, `!1024says`, `!amandasays`, `!satasays`, `!jibersays` and `!sbsays` followed by the text, with the same cooldowns | Sub
`/warm` | `user` `reason` | Warm someone up | Sub
`/executie` | `user` `reason` | A parody of the `!execute` command in [WubbyBot](https://github.com/tt2468/WubbyBot) | Sub
`/wubbysays` | | When to watch wubby live | Sub
`/dating` | | Haha | All
`/random` | `type` | Pick a random user. **Types:** `sub` *(includes twitch and patreon roles)*, `non-sub`, `tier 3`, `admin`, `mod`, `tree`, `5'8"` | All
`/d20` | `reason` | Roll a d20 | All
`/emoji` | `emoji` | Enlarge up to 5 emotes or emoji | All

### Staff

Command | Options | Description | Access
--- | --- | --- | ---
`/blacklist add` | `user` | Blacklist a user from using any bot commands | Staff
`/blacklist remove` | `user` | Removes a user from the blacklist | Staff
`/blacklist list` | | List all blacklisted users | Staff
`/emotes` | | List the server emotes and who can use them | Staff

## Mention Replies `@Head Non-Sub <phrase>`

The bot replies when mentioned with one of these phrases.

Phrase | Description | Access
--- | --- | ---
`you are great` | Bot will respond to you. **Also:** `how are you`, `i like you`, `i love you`, `nice to see you` | All
`you suck` | Bot will respond to you. **Also:** `die`, `fuck off`, `fuck you`, `i hate you` | All
`bad bot` | :angry: **Also:** `trash bot` | All
`good bot` | :smirk: | Xathz
