using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Extensions;

namespace HeadNonSub.Clients.Discord.Commands {

    [BlacklistEnforced]
    [CommandContextType(InteractionContextType.Guild)]
    public class Fun : BetterModuleBase {

        public enum RandomType {
            [ChoiceDisplay("sub (twitch and patreon)")]
            Sub,
            [ChoiceDisplay("non-sub")]
            NonSub,
            [ChoiceDisplay("tier 3")]
            Tier3,
            [ChoiceDisplay("admin")]
            Admin,
            [ChoiceDisplay("mod")]
            Mod,
            [ChoiceDisplay("tree")]
            Tree,
            [ChoiceDisplay("5'8\"")]
            FiveEight
        }

        [SlashCommand("dating", "Speed dating")]
        public async Task Dating() {
            IUserMessage message = await BetterReplyAsync($"Haha {BetterUserFormat(nicknameOnly: true)}, you are alone.");

            IEmote emote = Context.Guild.Emotes.FirstOrDefault(x => x.Id == 461043064979456012);
            if (emote is IEmote) {
                await message.AddReactionAsync(emote);
            }
        }

        [SlashCommand("wubbysays", "When to watch wubby")]
        [SubscriberOnly]
        [Cooldown(300)]
        public async Task WubbySays() => await BetterReplyAsync("You can watch what wubby says live every Wednesday, Friday, Sunday, and often Mondays @ 8:30pm et / 5:30pm pt!");

        [SlashCommand("executie", "Executie someone you like")]
        [SubscriberOnly]
        [Cooldown(300)]
        public async Task Executie([Summary("user", "Who to executie")] SocketGuildUser user, [Summary("reason", "Why they are being executie'd")] string reason = "") {
            string parameters = $"{user} ({user.Id}); {reason}";

            if (!user.Roles.Any(x => x.Id == WubbysFunHouse.TwitchSubscriberRoleId)) {
                await BetterReplyAsync("You can not executie a non-sub. Executie is for someone you like and no one likes a non-sub.", parameters: parameters);
                return;
            } else if (user.Roles.Any(x => x.Id == WubbysFunHouse.MutedRoleId)) {
                await BetterReplyAsync("You can not executie a muted user. They are to be ignored and given no affection.", parameters: parameters);
                return;
            }

            await BetterReplyAsync($"{user.Mention} you have 10 seconds to say last words before you are executie'd.", parameters: parameters);
            await Task.Delay(10000);

            await BetterReplyAsync($"10 seconds over. Now executie-ing {user} for reason: `{reason.Replace("`", "")}`...", parameters: parameters);
            await Task.Delay(1000);

            await BetterReplyAsync("3", parameters: parameters);
            await Task.Delay(1000);

            await BetterReplyAsync("2", parameters: parameters);
            await Task.Delay(1000);

            await BetterReplyAsync("1", parameters: parameters);
            await Task.Delay(200);

            IUserMessage message = await BetterReplyAsync($"The target, `{user}` has been hugged and their messages from the past 69 days have also been hugged. :heart:", parameters: parameters);
            await message.AddReactionAsync(new Emoji("🍆"));
        }

        [SlashCommand("random", "Pick a random user")]
        public async Task Random([Summary("type", "What kind of user to pick")] RandomType type) {
            SocketGuildUser randomUser = type switch {
                RandomType.Sub => Context.Guild.Users.Where(x => x.Roles.Any(r => r.Id == WubbysFunHouse.TwitchSubscriberRoleId)).PickRandom(),
                RandomType.NonSub => Context.Guild.Users.Where(x => !x.Roles.Any(r => r.Id == WubbysFunHouse.TwitchSubscriberRoleId)).PickRandom(),
                RandomType.Tier3 => Context.Guild.Users.Where(x => x.Roles.Any(r => r.Id == WubbysFunHouse.TwitchSubscriberTier3RoleId)).PickRandom(),
                RandomType.Admin => Context.Guild.Users.Where(x => x.Roles.Any(r => r.Id == WubbysFunHouse.AdminsRoleId)).PickRandom(),
                RandomType.Mod => Context.Guild.Users.Where(x => x.Roles.Any(r => r.Id == WubbysFunHouse.ModsRoleId | r.Id == WubbysFunHouse.ModLiteRoleId | r.Id == WubbysFunHouse.TwitchModRoleId | r.Id == WubbysFunHouse.SubredditModRoleId)
                                        & !x.Roles.Any(r => r.Id == WubbysFunHouse.AdminsRoleId | r.Id == WubbysFunHouse.GingerBoyRoleId)).PickRandom(),
                RandomType.Tree => Context.Guild.GetUser(339786294895050753), // Amanda
                _ => Context.Guild.GetUser(177657233025400832) // Wubby
            };

            if (!(randomUser is SocketGuildUser)) {
                await BetterReplyAsync("Failed to select a random user.", parameters: type.ToString());
                return;
            }

            EmbedBuilder builder = new EmbedBuilder {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = "Picking a random user...",
                ThumbnailUrl = Constants.LoadingGifUrl,
                Footer = new EmbedFooterBuilder() {
                    Text = $"Random user requested by {BetterUserFormat(formatChar: "")}"
                }
            };

            IUserMessage message = await BetterReplyAsync(builder.Build(), parameters: type.ToString());

            await Task.Delay(8000);

            builder.Title = BetterUserFormat(randomUser);
            builder.ThumbnailUrl = null;
            builder.AddField("Account created", $"{randomUser.CreatedAt.DateTime.ToShortDateString()} {randomUser.CreatedAt.DateTime.ToShortTimeString()}", true);

            if (randomUser.JoinedAt.HasValue) {
                builder.AddField("Joined server", $"{randomUser.JoinedAt.Value.DateTime.ToShortDateString()} {randomUser.JoinedAt.Value.DateTime.ToShortTimeString()}", true);
            }

            await message.ModifyAsync(x => { x.Embed = builder.Build(); });
        }

        [SlashCommand("d20", "Roll a d20")]
        public async Task Roll([Summary("reason", "What you are rolling for")] string reason = "") {
            int needed = System.Random.Shared.Next(1, 21);
            int roll = System.Random.Shared.Next(1, 21);
            bool rollMet = roll <= needed;

            if (string.IsNullOrWhiteSpace(reason)) {
                await BetterReplyAsync($":game_die: {Context.User.Mention} forgot the reason for their roll, will they instantly die? {(rollMet ? "Yes, in extreme pain" : "Unfortunately no")} (**{needed}** needed and rolled **{roll}**)");
            } else {
                await BetterReplyAsync($":game_die: {Context.User.Mention} rolls for `{reason.Replace("`", "")}`.. (**{needed}** needed and rolled **{roll}**)");
            }
        }

        [SlashCommand("nonsub", "How to get the subscriber role")]
        [Cooldown(20)]
        public async Task NonSub() => await BetterReplyAsync($"Sub <{WubbysFunHouse.TwitchSubscribeUrl}> or link your Twitch <{WubbysFunHouse.LinkTwitchToDiscordUrl}>.");

    }

}
