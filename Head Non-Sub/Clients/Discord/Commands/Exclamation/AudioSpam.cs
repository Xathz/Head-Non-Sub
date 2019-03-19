using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Clients.Discord.Services;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced, OwnerAdminWhitelist]
    [RequireContext(ContextType.Guild)]
    public class AudioSpam : BetterModuleBase {

        private SocketVoiceChannel ValidateChannel(SocketVoiceChannel channel) {
            channel = channel ?? (Context.User as SocketGuildUser)?.VoiceChannel;

            if (channel == null) {
                ReplyAsync("You must be in a voice channel, or a voice channel id must be passed as an argument.");
                return null;
            }

            try {
                if (!Context.Guild.Channels.Any(x => x.Id == channel.Id)) {
                    ReplyAsync("Invalid voice channel id.");
                    return null;
                }
            } catch {
                ReplyAsync("Invalid voice channel id.");
                return null;
            }

            return channel;
        }

        [Command("ffs")]
        [Cooldown(120)]
        public async Task FFS(SocketVoiceChannel channel = null) {
            _ = Context.Message.DeleteAsync();

            if (ValidateChannel(channel) is SocketVoiceChannel validatedChannel) {
                IAudioClient audioClient = await validatedChannel.ConnectAsync();

                string clip = Path.Combine(Constants.AudioDirectory, "ffs.mp3");
                await Audio.PlayFileAsync(audioClient, clip);

                TrackStatistics();
            }
        }

        [Command("goodjob")]
        [Cooldown(120)]
        public async Task GoodJob(SocketVoiceChannel channel = null) {
            _ = Context.Message.DeleteAsync();

            if (ValidateChannel(channel) is SocketVoiceChannel validatedChannel) {
                IAudioClient audioClient = await validatedChannel.ConnectAsync();

                string clip = Path.Combine(Constants.AudioDirectory, "goodjob.mp3");
                await Audio.PlayFileAsync(audioClient, clip);

                TrackStatistics();
            }
        }

        [Command("handwarmer")]
        [Cooldown(120)]
        public async Task HandWarmer(SocketVoiceChannel channel = null) {
            _ = Context.Message.DeleteAsync();

            if (ValidateChannel(channel) is SocketVoiceChannel validatedChannel) {
                IAudioClient audioClient = await validatedChannel.ConnectAsync();

                string clip = Path.Combine(Constants.AudioDirectory, "handwarmer.mp3");
                await Audio.PlayFileAsync(audioClient, clip);

                TrackStatistics();
            }
        }

        [Command("lifeguard")]
        [Cooldown(120)]
        public async Task Lifeguard(SocketVoiceChannel channel = null) {
            _ = Context.Message.DeleteAsync();

            if (ValidateChannel(channel) is SocketVoiceChannel validatedChannel) {
                IAudioClient audioClient = await validatedChannel.ConnectAsync();

                string clip = Path.Combine(Constants.AudioDirectory, "lifeguard.mp3");
                await Audio.PlayFileAsync(audioClient, clip);

                TrackStatistics();
            }
        }

        [Command("producer")]
        [Cooldown(120)]
        public async Task Producer(SocketVoiceChannel channel = null) {
            _ = Context.Message.DeleteAsync();

            if (ValidateChannel(channel) is SocketVoiceChannel validatedChannel) {
                IAudioClient audioClient = await validatedChannel.ConnectAsync();

                string clip = Path.Combine(Constants.AudioDirectory, "producer.mp3");
                await Audio.PlayFileAsync(audioClient, clip);

                TrackStatistics();
            }
        }

        [Command("staypositive")]
        [Cooldown(120)]
        public async Task StayPositive(SocketVoiceChannel channel = null) {
            _ = Context.Message.DeleteAsync();

            if (ValidateChannel(channel) is SocketVoiceChannel validatedChannel) {
                IAudioClient audioClient = await validatedChannel.ConnectAsync();

                string clip = Path.Combine(Constants.AudioDirectory, "staypositive.mp3");
                await Audio.PlayFileAsync(audioClient, clip);

                TrackStatistics();
            }
        }

    }

}
