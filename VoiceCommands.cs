using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;

namespace ProjektDjAladar
{
    public class VoiceCommands : BaseCommandModule
    {
        int PlayProcessId;
        [Command("join"), Description("Joins a voice channel.")]
        public async Task Join(CommandContext ctx, DiscordChannel chn = null)
        {
            // check whether VNext is enabled
            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                // not enabled
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }

            // check whether we aren't already connected
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
            {
                // already connected
                await ctx.RespondAsync("Already connected in this guild.");
                return;
            }

            // get member's voice state
            var vstat = ctx.Member?.VoiceState;
            if (vstat?.Channel == null && chn == null)
            {
                // they did not specify a channel and are not in one
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            // channel not specified, use user's
            if (chn == null)
                chn = vstat.Channel;

            // connect
            vnc = await vnext.ConnectAsync(chn);
            await ctx.RespondAsync($"Connected to `{chn.Name}`");
        }

        [Command("leave"), Description("Leaves a voice channel.")]
        public async Task Leave(CommandContext ctx)
        {
            // check whether VNext is enabled
            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                // not enabled
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }

            // check whether we are connected
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                // not connected
                await ctx.RespondAsync("Not connected in this guild.");
                return;
            }

            // disconnect
            vnc.Disconnect();
            await ctx.RespondAsync("Disconnected");
        }

        [Command("play"), Description("Plays an audio file from YouTube")]
        public async Task Play(CommandContext ctx, [RemainingText, Description("Full path to the file to play.")] string filename)
        {
            var pathToFile = YoutubeDownload.DownloadVideo(filename);
            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                // not enabled
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }

            // check whether we aren't already connected
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                // already connected
                await ctx.RespondAsync("Not connected in this guild.");
                return;
            }

            // check if file exists
            if (!File.Exists(pathToFile))
            {
                // file does not exist
                await ctx.RespondAsync($"File `{pathToFile}` does not exist.");
                return;
            }

            // wait for current playback to finish
            while (vnc.IsPlaying)
                await vnc.WaitForPlaybackFinishAsync();

            // play
            Exception exc = null;
            await ctx.Message.RespondAsync($"Playing `{pathToFile}`");

            try
            {
                await vnc.SendSpeakingAsync(true);

                var psi = new ProcessStartInfo
                {
                    FileName = "ffmpeg.exe",
                    Arguments = $@"-i ""{pathToFile}"" -ac 2 -f s16le -ar 48000 pipe:1 -loglevel quiet",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                var ffmpeg = Process.Start(psi);
                PlayProcessId = ffmpeg.Id;
                var ffout = ffmpeg.StandardOutput.BaseStream;

                var txStream = vnc.GetTransmitSink();
                await ffout.CopyToAsync(txStream);
                await txStream.FlushAsync();
                await vnc.WaitForPlaybackFinishAsync();
            }
            catch (Exception ex) { exc = ex; }
            finally
            {
                await vnc.SendSpeakingAsync(false);
                await ctx.Message.RespondAsync($"Finished playing `{pathToFile}`");

                DirectoryInfo di = new DirectoryInfo(@"E:\bot");

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
            }

            if (exc != null)
                await ctx.RespondAsync($"An exception occured during playback: `{exc.GetType()}: {exc.Message}`");
        }

        [Command("directplay"), Description("Plays an audio file from file path.")]
        public async Task DirectPlay(CommandContext ctx, [RemainingText, Description("Full path to the file to play.")] string filename)
        {
            var pathToFile = Path.GetFullPath(filename);
            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                // not enabled
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }

            // check whether we aren't already connected
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                // already connected
                await ctx.RespondAsync("Not connected in this guild.");
                return;
            }

            // check if file exists
            if (!File.Exists(pathToFile))
            {
                // file does not exist
                await ctx.RespondAsync($"File `{pathToFile}` does not exist.");
                return;
            }

            // wait for current playback to finish
            while (vnc.IsPlaying)
                await vnc.WaitForPlaybackFinishAsync();

            // play
            Exception exc = null;
            await ctx.Message.RespondAsync($"Playing `{pathToFile}`");

            try
            {
                await vnc.SendSpeakingAsync(true);

                var psi = new ProcessStartInfo
                {
                    FileName = "ffmpeg.exe",
                    Arguments = $@"-i ""{pathToFile}"" -ac 2 -f s16le -ar 48000 pipe:1 -loglevel quiet",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                var ffmpeg = Process.Start(psi);
                PlayProcessId = ffmpeg.Id;
                var ffout = ffmpeg.StandardOutput.BaseStream;

                var txStream = vnc.GetTransmitSink();
                await ffout.CopyToAsync(txStream);
                await txStream.FlushAsync();
                await vnc.WaitForPlaybackFinishAsync();
            }
            catch (Exception ex) { exc = ex; }
            finally
            {
                await vnc.SendSpeakingAsync(false);
                await ctx.Message.RespondAsync($"Finished playing `{pathToFile}`");
            }

            if (exc != null)
                await ctx.RespondAsync($"An exception occured during playback: `{exc.GetType()}: {exc.Message}`");
        }

        [Command("stop"), Description("Stops playing audio")]
        public async Task Stop(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                // not enabled
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }
            var vnc = vnext.GetConnection(ctx.Guild);
            await vnc.SendSpeakingAsync(false);

            var psi = Process.GetProcessById(PlayProcessId);
            psi.Kill();
            psi.WaitForExit();
            psi.Dispose();

            await ctx.Message.RespondAsync($"Stopped playing");

            DirectoryInfo di = new DirectoryInfo(@"E:\bot");

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }

        }

        [Command("pause"), Description("Stops playing audio")]
        public async Task Pause(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                // not enabled
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }
            var vnc = vnext.GetConnection(ctx.Guild);
            var txStream = vnc.GetTransmitSink();
            txStream.Pause();
            vnc.Pause();

            await ctx.Message.RespondAsync($"Paused playback");
        }

        [Command("resume"), Description("Stops playing audio")]
        public async Task Resume(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                // not enabled
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }
            var vnc = vnext.GetConnection(ctx.Guild);
            var txStream = vnc.GetTransmitSink();
            await txStream.ResumeAsync();
            await vnc.ResumeAsync();

            await ctx.Message.RespondAsync($"Paused playback");
        }
    }
}
