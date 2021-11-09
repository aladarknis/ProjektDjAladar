using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using DSharpPlus.Lavalink;
using System.Linq;
using System.Text;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using DSharpPlus.Lavalink.EventArgs;

namespace ProjektDjAladar
{
    public class VoiceCommands : BaseCommandModule
    {
        public Queue TrackQueue = new Queue();

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
            {
                chn = vstat.Channel;
                await ctx.RespondAsync($"Connected to `{chn.Name}`");
            }


            // connect

            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }

            var node = lava.ConnectedNodes.Values.First();
            await node.ConnectAsync(vstat.Channel);
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

            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }

            // disconnect
            vnc.Disconnect();
            await ctx.RespondAsync("Disconnected");
        }

        [Command("play"), Description("Plays an audio file from YouTube")]
        public async Task Play(CommandContext ctx, [RemainingText, Description("Full path to the file to play.")] string search)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            Uri uri = new Uri(search);
            LavalinkLoadResult loadResult = await node.Rest.GetTracksAsync(uri);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed for {search}.");
                return;
            }

            foreach (LavalinkTrack track in loadResult.Tracks)
            {
                TrackQueue.Enqueue(new TrackRequest(ctx, track));
            }

            if (await ConnectionCheck(ctx, conn))
            {
                if (conn.CurrentState.CurrentTrack == null)
                {
                    var request = (TrackRequest)TrackQueue.Dequeue();
                    await conn.PlayAsync(request.GetRequestTrack());

                    await ctx.RespondAsync($"Now playing {request.GetRequestTrack().Title}!");

                    conn.PlaybackFinished += Conn_PlaybackFinished;
                }
            }
        }

        private async Task Conn_PlaybackFinished(LavalinkGuildConnection conn, TrackFinishEventArgs e)
        {
            if (TrackQueue.Count != 0)
            {
                await PlayFromQueue(conn);
            }
        }

        private async Task PlayFromQueue(LavalinkGuildConnection conn)
        {
            var request = (TrackRequest)TrackQueue.Dequeue();
            var track = request.GetRequestTrack();
            var ctx = request.GetRequestCtx();
            if (conn.CurrentState.CurrentTrack == null)
            {
                await conn.PlayAsync(track);
                await ctx.RespondAsync($"Now playing {track.Title}!");
                //TODO Announce playing track pulled from queue
                //_ = conn.Channel.SendMessageAsync($"Now playing {((LavalinkTrack)track)?.Title}!");

                conn.PlaybackFinished += Conn_PlaybackFinished;
            }
        }

        [Command("playpartial"), Description("Plays a part of audio file from YouTube")]
        public async Task PlayPartial(CommandContext ctx, [RemainingText, Description("Full path to the file to play.")] string search, TimeSpan start, TimeSpan stop)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            Uri uri = new Uri(search);
            LavalinkLoadResult loadResult = await node.Rest.GetTracksAsync(uri);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed for {search}.");
                return;
            }

            var track = loadResult.Tracks.First();

            TrackQueue.Enqueue(new TrackRequest(ctx, track));

            if (await ConnectionCheck(ctx, conn))
            {
                if (conn.CurrentState.CurrentTrack == null)
                {
                    var request = (TrackRequest)TrackQueue.Dequeue();
                    await conn.PlayPartialAsync(request.GetRequestTrack(), start, stop);

                    await ctx.RespondAsync($"Now playing {track.Title}!");

                    conn.PlaybackFinished += Conn_PlaybackFinished;
                }
            }
        }

        [Command("skip"), Description("Skips song in playing")]
        public async Task Skip(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                // not enabled
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            await conn.StopAsync();

            await ctx.Message.RespondAsync($"Skipped playing!");
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
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            TrackQueue.Clear();

            await conn.StopAsync();

            await ctx.Message.RespondAsync($"Stopped playing!");
        }

        [Command("clear"), Description("Clears queue")]
        public async Task Clear(CommandContext ctx)
        {
            TrackQueue.Clear();
            await ctx.Message.RespondAsync($"Queue cleared!");
        }

        [Command("pause"), Description("Stops playing audio")]
        public async Task Pause(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (await ConnectionCheck(ctx, conn))
            {
                if (conn.CurrentState.CurrentTrack == null)
                {
                    await ctx.RespondAsync("There are no tracks loaded.");
                    return;
                }

                await ctx.RespondAsync($"Paused playing!");
                await conn.PauseAsync();
            }
        }

        [Command("resume"), Description("Stops playing audio")]
        public async Task Resume(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (await ConnectionCheck(ctx, conn))
            {
                if (conn.CurrentState.CurrentTrack == null)
                {
                    await ctx.RespondAsync("There are no tracks loaded.");
                    return;
                }

                await ctx.RespondAsync($"Resumed playing!");
                await conn.ResumeAsync();
            }
        }

        [Command("np"), Description("Shows what's being currently played.")]
        public async Task NowPlayingAsync(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (await ConnectionCheck(ctx, conn))
            {
                if (conn.CurrentState.CurrentTrack == null)
                {
                    await ctx.RespondAsync("There are no tracks loaded.");
                    return;
                }

                var state = conn.CurrentState;
                var track = state.CurrentTrack;
                await ctx.RespondAsync($"Now playing: {track.Title} by {track.Author} [{state.PlaybackPosition}/{track.Length}].").ConfigureAwait(false);
            }
        }

        [Command("queue"), Description("Shows actual queued songs.")]
        public async Task Queue(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (TrackQueue.Count == 0)
            {
                await ctx.RespondAsync("Queue is empty!").ConfigureAwait(false);
                return;
            }

            if (await ConnectionCheck(ctx, conn))
            {
                var sb = new StringBuilder();
                sb.Append("Queue: ```");

                if (TrackQueue.Count > 10)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        sb.Append($"{((TrackRequest)TrackQueue.ToArray()[i]).GetRequestTrack().Title} by {((TrackRequest)TrackQueue.ToArray()[i]).GetRequestTrack().Author}").AppendLine();
                    }
                    sb.Append($"... of total {TrackQueue.Count} songs").AppendLine();
                }
                else
                {
                    foreach (TrackRequest request in TrackQueue)
                    {
                        sb.Append($"{request.GetRequestTrack().Title} by {request.GetRequestTrack().Author}").AppendLine();
                    }
                }
                sb.Append("```");
                await ctx.RespondAsync(sb.ToString()).ConfigureAwait(false);
            }
        }

        [Command("seek"), Description("Seeks in the current track.")]
        public async Task SeekAsync(CommandContext ctx, TimeSpan position)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (await ConnectionCheck(ctx, conn))
            {
                if (conn.CurrentState.CurrentTrack == null)
                {
                    await ctx.RespondAsync("There are no tracks loaded.");
                    return;
                }

                await conn.SeekAsync(position).ConfigureAwait(false);
                await ctx.RespondAsync($"Seeking to {position}.").ConfigureAwait(false);
            }
        }

        [Command("volume"), Description("Changes playback volume.")]
        public async Task VolumeAsync(CommandContext ctx, int volume)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (await ConnectionCheck(ctx, conn))
            {
                await conn.SetVolumeAsync(volume).ConfigureAwait(false);
                await ctx.RespondAsync($"Volume set to {volume}%.").ConfigureAwait(false);
            }
        }


        [Command("eqreset"), Description("Sets or resets equalizer settings.")]
        public async Task EqualizerAsync(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (await ConnectionCheck(ctx, conn))
            {
                await conn.ResetEqualizerAsync().ConfigureAwait(false);
                await ctx.RespondAsync("All equalizer bands were reset.").ConfigureAwait(false);
            }
        }

        [Command("eq"), Description("Sets or resets equalizer settings.")]
        public async Task EqualizerAsync(CommandContext ctx, int band, float gain)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (await ConnectionCheck(ctx, conn))
            {
                await conn.AdjustEqualizerAsync(new LavalinkBandAdjustment(band, gain)).ConfigureAwait(false);
                await ctx.RespondAsync($"Band {band} adjusted by {gain}").ConfigureAwait(false);
            }
        }

        [Command("stats"), Description("Displays Lavalink statistics.")]
        public async Task StatsAsync(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (await ConnectionCheck(ctx, conn))
            {
                var stats = node.Statistics;
                var sb = new StringBuilder();
                sb.Append("Lavalink resources usage statistics: ```")
                    .Append("Uptime:                    ").Append(stats.Uptime).AppendLine()
                    .Append("Players:                   ").AppendFormat("{0} active / {1} total", stats.ActivePlayers, stats.TotalPlayers).AppendLine()
                    .Append("CPU Cores:                 ").Append(stats.CpuCoreCount).AppendLine()
                    .Append("CPU Usage:                 ").AppendFormat("{0:#,##0.0%} lavalink / {1:#,##0.0%} system", stats.CpuLavalinkLoad, stats.CpuSystemLoad).AppendLine()
                    .Append("RAM Usage:                 ").AppendFormat("{0} allocated / {1} used / {2} free / {3} reservable", SizeToString(stats.RamAllocated), SizeToString(stats.RamUsed), SizeToString(stats.RamFree), SizeToString(stats.RamReservable)).AppendLine()
                    .Append("Audio frames (per minute): ").AppendFormat("{0:#,##0} sent / {1:#,##0} nulled / {2:#,##0} deficit", stats.AverageSentFramesPerMinute, stats.AverageNulledFramesPerMinute, stats.AverageDeficitFramesPerMinute).AppendLine()
                    .Append("```");
                await ctx.RespondAsync(sb.ToString()).ConfigureAwait(false);
            }
        }

        [Command("vymitani"), Description("Negre seber se a vypadni!")]
        public async Task vymitaniAsync(CommandContext ctx)
        {
            await ctx.RespondAsync("Negre seber se a vypadni!");
            await Play(ctx, new JsonSettings().LoadedSettings.VymitaniUrl);
        }

        private static readonly string[] Units = new[] { "", "ki", "Mi", "Gi" };
        private static string SizeToString(long l)
        {
            double d = l;
            var u = 0;
            while (d >= 900 && u < Units.Length - 2)
            {
                u++;
                d /= 1024;
            }

            return $"{d:#,##0.00} {Units[u]}B";
        }

        public static async Task<bool> ConnectionCheck(CommandContext ctx, LavalinkGuildConnection conn)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return false;
            }

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return false;
            }

            return true;
        }
    }
}

