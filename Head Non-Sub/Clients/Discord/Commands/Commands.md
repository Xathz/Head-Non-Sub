## Notes

### General

- Some commands have cooldowns either server wide, per-channel, or per-user
  - Some are dynamic so the duration can not be listed here
- The raid recovery system commands are per-channel
  - If enabled at the same time in different channels all lists and ban lists are seperate
- Not all commands are listed, some are excluded here to prevent abuse

### Users

`<user>` parameter is **one** of
- `<user mention>` Example: `@User`
- `<username#delimiter>` Example: `User#1234`
- `<userid>` Example: `112233445566778899`

If using `<username#delimiter>` and the username contains a space, it must be encased in double quotes
- `"<username#delimiter>"`

### Access Scopes

Access | Scope / Description
--- | ---
All | Everyone
Sub | Users that have at least one role and do not have the `Non-sub` role<br>Users that have no roles can not use
Staff | **Built-in roles:** `Administrator`<br>**Guild (server) roles:** `Admins`, `Mods`, `Mod-Lite`<br>**Users:** `Xathz#6861`
Xathz | `Xathz#6861` only

## Exclamation Commands `!<command> <parameter(s)>`

Mainly used for memes and random things.

### Useful

Command | Parameters | Description | Access
--- | --- | --- | ---
`addnote` | `<user>` `<note>` | Add a note to a user | Staff
`notes` | `<user>` | Get a users notes | Staff
`deleteallnotes` | `<user>` | Delete all notes about a user | Staff
`deletenote` | `<user>` `<noteid>` | Delete a note. Get the note id's from the `notes` command | Staff
`prices` | | The donation prices for media share and special events on [PaymoneyWubby](https://wub.by/ttv)'s stream. **Alias:** `mediashare` | All
`randomclip` | | Get a random [PaymoneyWubby](https://wub.by/ttv) Twitch clip | All
`randomsong` | | Random song that Rythm Bot or Rythm Bot 2 has played | All
`sayscount` | | Top `says` command used and how many times | All
`stock` | | Get the [TwitchStocks](https://twitchstocks.com/stock/pymny) value of [PaymoneyWubby](https://wub.by/ttv)'s stock | All
`strawpoll` | `<title>` \| `<option 1>` \| `<option 2>` etc.. | Make a [Straw Poll](https://www.strawpoll.me). Title and options are seperated by a pipe `\|` | All
`strawpollresults` | `<url>` | Get the results of a [Straw Poll](https://www.strawpoll.me). **Alias:** `strawpollr` | All
`trashpoll` | `<text>` `<emotes and emoji>` | Sometimes it fails to use the emotes and emoji so it's trash | All
`truecount` | | Number of times `!true` was used | All
`twitchuser` | `<twitch username>` | Get most recent Twitch messages and mod actions | Staff

### Shitposting

Command | Parameters | Description | Access
--- | --- | --- | ---
`1024nude` | | [Scandalous](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/1024_nude.png) | All
`1024says` | `<text>` | Render text on 1024's screen | All
`amandasays` | `<text>` | Engrave Amanda's plate | All
`bigoof` | | idk what [you were](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/big_oof.gif) expecting | All
`dab` | | Why do peopl keep doing [this](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/dab.gif) | All
`dating` | | Haha. **Aliases:** `speeddating`, `datenight` | All
`dickdowndennis` | | [Came twice](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/dickdowndennis.png) just typing this out | All
`executie` | `<user>` `<reason>` | A parody of the `!execute` command in [WubbyBot](https://github.com/tt2468/WubbyBot) | All
`fuckedup` | | Not getting the [deposit](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/fucked_up.png) back. **Alias:** `ripdeposit` | All
`gimme` | | :astonished: | All
`gongboy` | | I should probability be banned for [this](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/gongboy.png) | All
`gottem` | | gOtTheM [so hard](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/gottem.gif) | All
`jibersays` | `<text>` | Wet and waiting to reflect | All
`keyboard` | | He dosn't even get [mad](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/wubbykeyboard.png) | All
`lucky` | | [Lit and lucky](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/lucky.png) | All
`moneyshot` | | Open wide [tt](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/moneyshot.png) | All
`night` | | Big night sky with emotes | All
`nonsub` | | [Gross](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/nonsub.png) | Sub
`potato` | | Wubby... in [potato form](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/potato.png) | All
`radar` | | Yes it really looks like [this](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/radar.png) | All
`random` | `<type>` | Pick a random user. **Types:** `sub` *(includes twitch and patreon roles)*, `nonsub`, `tier3`, `admin`, `mod`, `tree`, `5'8"` | All
`relax` | | Keeping holes [moist](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/relax.png) | All
`rnk` | | A parody of [MEE6](https://mee6.xyz)'s `!rank` | All
`satasays` | `<text>` | Now with icecream | All
`sbsays` | `<text>` | Strong Bad and his computer | All
`star` | | May he [shine bright](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/star.gif) forever | All
`true` | | [Speaking to the grass](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/true.png). **Alias:** `thatstrue` | All
`tts` | `<text>` | Text to speech voice used on [PaymoneyWubby](https://wub.by/ttv)'s stream (donos and subs). **Aliases:** `ttsdonate`, `ttssub`; **Voice:** Joanna | Sub
`tts2` | `<text>` | Text to speech voice. **Voice:** Justin | Sub
`tts3` | `<text>` | Text to speech voice. **Voice:** Brian | Sub
`tts4` | `<text>` | Text to speech voice. **Voice:** Mizuki | Sub
`tts5` | `<text>` | Text to speech voice used on [PaymoneyWubby](https://wub.by/ttv)'s stream (bits). **Alias:** `ttsbits`; **Voice:** Russell | Sub
`ttsays` | `<text>` | Make tt reflect upon something | All
`weebshit` | | I have had it with these motherfucking [weebs](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/weebshit.png) in this motherfucking Discord! | All
`what` | | [tt confuse](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/what.png) | All
`wubbycheeto` | | [Aww](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/wubbycheeto.png) | All
`yum` | | It was [so good](https://github.com/Xathz/Head-Non-Sub/blob/master/Content/yum.png) | All

## Mention Commands `@Head Non-Sub <command> <parameter(s)>`

Mainly used for bot and feature control/settings.

Command  | Parameters | Description | Access
--- | --- | --- | ---
`blacklist add` | `<user>` | Blacklist a user from using any bot commands | Staff
`blacklist list` | | List all blacklisted users | Staff
`blacklist remove` | `<user>` | Removes a user from the blacklist | Staff
`help` | | A link to this document | All
`ping` | | Check the delay from the bot to Discord | All
`emotemode` | `<mode>` | Enable/disable current channel mode. **Modes:** `off`, `textonly`, `emoteonly` | Staff
`rr ban` | `<minutes>` Optional: `<token>` | Ban suspected raiders from the past `x` minutes. You will be prompted with a list of users to be banned and a confirmation token. Remove a user from being banned with the `rr skip` command | Staff
`rr clean` | `<minutes>` | Delete messages from the suspected raiders from the past `x` minutes | Staff
`rr disable` | | Disable the raid recovery system | Staff
`rr enable` | | Enable the raid recovery system | Staff
`rr list` | `<minutes>` | List suspected raiders and how many messages they sent in the past `x` minutes | Staff
`rr skip` |  `<user>` | Remove a user from the ban command | Staff
`servermap` | | Direct message you a JSON file containing server info and layout | Staff
`membermap` | | Direct message you a JSON file containing all server members | Staff
`undo` | `<count>` | Delete ***your*** `x` most recent and bot replies | All
`undobot` | `<count>` | Delete `x` most recent bot messages | Staff
`undoemotes` | `<count>` | Delete `x` most messages that are mostly emotes | Staff
`good bot` | | :smirk: | Xathz
`you are great` | | Bot will respond to you. **Aliases:** `how are you`, `i like you`, `i love you`, `nice to see you` | All
`you suck` | | Bot will respond to you. **Aliases:** `die`, `fuck off`, `fuck you`, `i hate you`| All
