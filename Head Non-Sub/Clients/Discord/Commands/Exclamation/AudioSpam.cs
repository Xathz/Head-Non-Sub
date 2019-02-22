using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Clients.Discord.Services;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [RequireContext(ContextType.Guild)]
    public class AudioSpam : ModuleBase<SocketCommandContext> {

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
        public async Task FFSAsync(SocketVoiceChannel channel = null) {
            _ = Context.Message.DeleteAsync();

            if (ValidateChannel(channel) is SocketVoiceChannel validatedChannel) {
                IAudioClient audioClient = await validatedChannel.ConnectAsync();

                string clip = Path.Combine(Constants.AudioDirectory, "ffs.mp3");
                await Audio.PlayFileAsync(audioClient, clip);
            }
        }

        [Command("goodjob")]
        [Cooldown(120)]
        public async Task GoodJobAsync(SocketVoiceChannel channel = null) {
            _ = Context.Message.DeleteAsync();

            if (ValidateChannel(channel) is SocketVoiceChannel validatedChannel) {
                IAudioClient audioClient = await validatedChannel.ConnectAsync();

                string clip = Path.Combine(Constants.AudioDirectory, "goodjob.mp3");
                await Audio.PlayFileAsync(audioClient, clip);
            }
        }

        [Command("handwarmer")]
        [Cooldown(120)]
        public async Task HandWarmerAsync(SocketVoiceChannel channel = null) {
            _ = Context.Message.DeleteAsync();

            if (ValidateChannel(channel) is SocketVoiceChannel validatedChannel) {
                IAudioClient audioClient = await validatedChannel.ConnectAsync();

                string clip = Path.Combine(Constants.AudioDirectory, "handwarmer.mp3");
                await Audio.PlayFileAsync(audioClient, clip);
            }
        }

        [Command("lifeguard")]
        [Cooldown(120)]
        public async Task LifeguardAsync(SocketVoiceChannel channel = null) {
            _ = Context.Message.DeleteAsync();

            if (ValidateChannel(channel) is SocketVoiceChannel validatedChannel) {
                IAudioClient audioClient = await validatedChannel.ConnectAsync();

                string clip = Path.Combine(Constants.AudioDirectory, "lifeguard.mp3");
                await Audio.PlayFileAsync(audioClient, clip);
            }
        }

        [Command("producer")]
        [Cooldown(120)]
        public async Task ProducerAsync(SocketVoiceChannel channel = null) {
            _ = Context.Message.DeleteAsync();

            if (ValidateChannel(channel) is SocketVoiceChannel validatedChannel) {
                IAudioClient audioClient = await validatedChannel.ConnectAsync();

                string clip = Path.Combine(Constants.AudioDirectory, "producer.mp3");
                await Audio.PlayFileAsync(audioClient, clip);
            }
        }

        [Command("staypositive")]
        [Cooldown(120)]
        public async Task JoinChannel(SocketVoiceChannel channel = null) {
            _ = Context.Message.DeleteAsync();

            if (ValidateChannel(channel) is SocketVoiceChannel validatedChannel) {
                IAudioClient audioClient = await validatedChannel.ConnectAsync();

                string clip = Path.Combine(Constants.AudioDirectory, "staypositive.mp3");
                await Audio.PlayFileAsync(audioClient, clip);
            }
        }

    }

}
