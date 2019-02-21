using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord.Audio;

namespace HeadNonSub.Clients.Discord.Services {

    public static class Audio {

        public static async Task PlayFileAsync(IAudioClient client, string path) {
            using (Process ffmpeg = CreateStream(path))
            using (Stream output = ffmpeg.StandardOutput.BaseStream)
            using (AudioOutStream discord = client.CreatePCMStream(AudioApplication.Mixed)) {
                try {
                    await output.CopyToAsync(discord);
                } finally {
                    await discord.FlushAsync();
                }
            }

            await client.StopAsync();
        }

        private static Process CreateStream(string path) => Process.Start(new ProcessStartInfo {
            FileName = Path.Combine(Constants.RuntimesDirectory, "ffmpeg_win64.exe"),
            Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true
        });

    }

}

