using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.VoiceNext;

namespace ProjektDjAladar
{
    public class VoiceCommands : BaseCommandModule
    {
        private static readonly string[] Units = { "", "ki", "Mi", "Gi" };
        private readonly Queue _trackQueue = new();
        private bool _loop;

        [Command("join"), Description("Joins a voice channel.")]
        public async Task Join(CommandContext ctx)
        {
            var vnext = await CheckAndGetVNext(ctx);
            if (vnext == null) return;

            // Check whether we aren't already connected
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
            {
                await ctx.RespondAsync("Already connected in this guild.");
                return;
            }

            // Get member's voice state
            var vstat = ctx.Member?.VoiceState;
            if (vstat?.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            var channel = vstat.Channel;
            var audio = await FillAudio(ctx);
            await audio.Node.ConnectAsync(channel);
            await vnext.ConnectAsync(channel);
            await ctx.RespondAsync($"Connected b to `{channel.Name}`");
        }


        [Command("leave"), Description("Leaves a voice channel.")]
        public async Task Leave(CommandContext ctx)
        {
            var audio = await FillAudio(ctx);
            var vnext = await CheckAndGetVNext(ctx);
            if (vnext == null) return;

            if (await ConnectionCheck(ctx, audio.Conn))
            {
                await audio.Conn.DisconnectAsync();
                await ctx.RespondAsync("Disconnected");
            }
            else
            {
                await ctx.RespondAsync("Can't disconnect if not joined");
            }
        }

        [Command("play"), Description("Plays an audio file from YouTube")]
        public async Task Play(
            CommandContext ctx,
            [RemainingText, Description("Full path to the file to play.")]
            string search
        )

        {

            if (ctx.Member != null && ctx.Client.GetLavalink().ConnectedNodes.Values.First()
                    .GetGuildConnection(ctx.Member.VoiceState.Guild) == null)
            {
                await ctx.RespondAsync("Not connected, joining");
                await Join(ctx);
            }

            var audio = await FillAudio(ctx);
            var uri = new Uri(search);
            var loadResult = await audio.Node.Rest.GetTracksAsync(uri);

            if (loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed for {search}.");
                return;
            }

            foreach (var track in loadResult.Tracks)
            {
                _trackQueue.Enqueue(new TrackRequest(ctx, track));
            }

            if (await ConnectionCheck(ctx, audio.Conn))
            {
                if (audio.Conn.CurrentState.CurrentTrack == null)
                {
                    var request = (TrackRequest)_trackQueue.Dequeue();
                    if (_loop)
                    {
                        _trackQueue.Enqueue(request);
                    }

                    await audio.Conn.PlayAsync(request?.GetRequestTrack());
                    await ctx.RespondAsync($"Now playing {request?.GetRequestTrack().Title}!");
                    audio.Conn.PlaybackFinished += Conn_PlaybackFinished;
                }
            }
        }

        [Command("forceplay"), Description("Add song to the top of the que")]
        public async Task ForcePlay(CommandContext ctx,
            [RemainingText, Description("Full path to the file to play.")]
            string search)
        {
            var audio = await FillAudio(ctx);
            var uri = new Uri(search);
            var loadResult = await audio.Node.Rest.GetTracksAsync(uri);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed for {search}.");
                return;
            }

            var oldQueue = _trackQueue.ToArray();
            _trackQueue.Clear();
            foreach (var track in loadResult.Tracks)
            {
                _trackQueue.Enqueue(new TrackRequest(ctx, track));
            }

            foreach (var request in oldQueue)
            {
                _trackQueue.Enqueue(request);
            }
        }

        [Command("playpartial"), Description("Plays a part of a song")]
        public async Task PlayPartial(CommandContext ctx,
            [RemainingText, Description("Full path to the file to play.")]
            string search, TimeSpan start, TimeSpan stop)
        {
            var audio = await FillAudio(ctx);
            var uri = new Uri(search);
            var loadResult = await audio.Node.Rest.GetTracksAsync(uri);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed for {search}.");
                return;
            }

            var track = loadResult.Tracks.First();
            _trackQueue.Enqueue(new TrackRequest(ctx, track));
            if (await ConnectionCheck(ctx, audio.Conn))
            {
                if (audio.Conn.CurrentState.CurrentTrack == null)
                {
                    var request = (TrackRequest)_trackQueue.Dequeue();
                    await audio.Conn.PlayPartialAsync(request.GetRequestTrack(), start, stop);
                    await ctx.RespondAsync($"Now playing {track.Title}!");
                    audio.Conn.PlaybackFinished += Conn_PlaybackFinished;
                }
            }
        }

        [Command("skip"), Description("Skips song in playing")]
        public async Task Skip(CommandContext ctx)
        {
            var vnext = await CheckAndGetVNext(ctx);
            if (vnext == null) return;
            var audio = await FillAudio(ctx);
            await audio.Conn.StopAsync();
            await ctx.Message.RespondAsync("Skipped playing!");
        }

        [Command("stop"), Description("Stops playing audio")]
        public async Task Stop(CommandContext ctx)
        {
            var vnext = await CheckAndGetVNext(ctx);
            if (vnext == null) return;
            var audio = await FillAudio(ctx);
            await audio.Conn.StopAsync();
            await ctx.Message.RespondAsync("Stopped playing!");
        }

        [Command("clear"), Description("Clears queue")]
        public async Task Clear(CommandContext ctx)
        {
            _trackQueue.Clear();
            await ctx.Message.RespondAsync("Queue cleared!");
        }

        [Command("pause"), Description("Stops playing audio")]
        public async Task Pause(CommandContext ctx)
        {
            var audio = await FillAudio(ctx);
            if (await ConnectionCheck(ctx, audio.Conn))
            {
                if (await AnyTracksLoaded(ctx, audio.Conn)) return;
                await ctx.RespondAsync("Paused playing!");
                await audio.Conn.PauseAsync();
            }
        }

        [Command("resume"), Description("Stops playing audio")]
        public async Task Resume(CommandContext ctx)
        {
            var audio = await FillAudio(ctx);
            if (await ConnectionCheck(ctx, audio.Conn))
            {
                if (await AnyTracksLoaded(ctx, audio.Conn)) return;
                await ctx.RespondAsync("Resumed playing!");
                await audio.Conn.ResumeAsync();
            }
        }

        [Command("np"), Description("Shows what's being currently played.")]
        public async Task NowPlayingAsync(CommandContext ctx)
        {
            var audio = await FillAudio(ctx);
            if (await ConnectionCheck(ctx, audio.Conn))
            {
                if (await AnyTracksLoaded(ctx, audio.Conn)) return;
                var state = audio.Conn.CurrentState;
                var track = state.CurrentTrack;
                await ctx.RespondAsync(
                        $"Now playing: {track.Title} by {track.Author} [{state.PlaybackPosition}/{track.Length}].")
                    .ConfigureAwait(false);
            }
        }

        [Command("queue"), Description("Shows actual queued songs.")]
        public async Task Queue(CommandContext ctx)
        {
            var audio = await FillAudio(ctx);
            if (_trackQueue.Count == 0)
            {
                await ctx.RespondAsync("Queue is empty!").ConfigureAwait(false);
                return;
            }

            if (await ConnectionCheck(ctx, audio.Conn))
            {
                var sb = new StringBuilder();
                int counter = 0;
                sb.Append("Queue: ```");

                foreach (TrackRequest request in _trackQueue)
                {
                    counter++;
                    sb.Append($"{request.GetRequestTrack().Title} by {request.GetRequestTrack().Author}").AppendLine();
                    if (counter > 10)
                    {
                        sb.Append($"... of total {_trackQueue.Count} songs").AppendLine();
                        break;
                    }
                }

                sb.Append("```");
                await ctx.RespondAsync(sb.ToString()).ConfigureAwait(false);
            }
        }

        [Command("seek"), Description("Seeks in the current track.")]
        public async Task SeekAsync(CommandContext ctx, TimeSpan position)
        {
            var audio = await FillAudio(ctx);
            if (await ConnectionCheck(ctx, audio.Conn))
            {
                if (await AnyTracksLoaded(ctx, audio.Conn)) return;
                await audio.Conn.SeekAsync(position).ConfigureAwait(false);
                await ctx.RespondAsync($"Seeking to {position}.").ConfigureAwait(false);
            }
        }

        [Command("loop"), Description("Loops or unloops current track")]
        public async Task LoopTrack(CommandContext ctx)
        {
            _loop = !_loop;
            if (_loop)
            {
                await ctx.RespondAsync("Loop enabled");
            }
            else
            {
                await ctx.RespondAsync("Loop disabled");
            }
        }

        [Command("volume"), Description("Changes playback volume.")]
        public async Task VolumeAsync(CommandContext ctx, int volume)
        {
            var audio = await FillAudio(ctx);
            if (await ConnectionCheck(ctx, audio.Conn))
            {
                await audio.Conn.SetVolumeAsync(volume).ConfigureAwait(false);
                await ctx.RespondAsync($"Volume set to {volume}%.").ConfigureAwait(false);
            }
        }


        [Command("eqreset"), Description("Sets or resets equalizer settings.")]
        public async Task EqualizerAsync(CommandContext ctx)
        {
            var audio = await FillAudio(ctx);
            if (await ConnectionCheck(ctx, audio.Conn))
            {
                await audio.Conn.ResetEqualizerAsync().ConfigureAwait(false);
                await ctx.RespondAsync("All equalizer bands were reset.").ConfigureAwait(false);
            }
        }

        [Command("eq"), Description("Sets or resets equalizer settings.")]
        public async Task EqualizerAsync(CommandContext ctx, int band, float gain)
        {
            var audio = await FillAudio(ctx);
            if (await ConnectionCheck(ctx, audio.Conn))
            {
                await audio.Conn.AdjustEqualizerAsync(new LavalinkBandAdjustment(band, gain)).ConfigureAwait(false);
                await ctx.RespondAsync($"Band {band} adjusted by {gain}").ConfigureAwait(false);
            }
        }

        [Command("stats"), Description("Displays Lavalink statistics.")]
        public async Task StatsAsync(CommandContext ctx)
        {
            var audio = await FillAudio(ctx);
            if (await ConnectionCheck(ctx, audio.Conn))
            {
                var stats = audio.Node.Statistics;
                var sb = new StringBuilder();
                sb.Append("Lavalink resources usage statistics: ```")
                    .Append("Uptime:                    ").Append(stats.Uptime).AppendLine()
                    .Append("Players:                   ")
                    .AppendFormat("{0} active / {1} total", stats.ActivePlayers, stats.TotalPlayers).AppendLine()
                    .Append("CPU Cores:                 ").Append(stats.CpuCoreCount).AppendLine()
                    .Append("CPU Usage:                 ").AppendFormat("{0:#,##0.0%} lavalink / {1:#,##0.0%} system",
                        stats.CpuLavalinkLoad, stats.CpuSystemLoad).AppendLine()
                    .Append("RAM Usage:                 ").AppendFormat(
                        "{0} allocated / {1} used / {2} free / {3} reservable", SizeToString(stats.RamAllocated),
                        SizeToString(stats.RamUsed), SizeToString(stats.RamFree), SizeToString(stats.RamReservable))
                    .AppendLine()
                    .Append("Audio frames (per minute): ")
                    .AppendFormat("{0:#,##0} sent / {1:#,##0} nulled / {2:#,##0} deficit",
                        stats.AverageSentFramesPerMinute, stats.AverageNulledFramesPerMinute,
                        stats.AverageDeficitFramesPerMinute).AppendLine()
                    .Append("```");
                await ctx.RespondAsync(sb.ToString()).ConfigureAwait(false);
            }
        }

        [Command("vymitani"), Description("Negre seber se a vypadni!")]
        public async Task VymitaniAsync(CommandContext ctx)
        {
            await ctx.RespondAsync("Negre seber se a vypadni!");
            await Play(ctx, new JsonSettings().LoadedSettings.VymitaniUrl);
        }

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

        private static async Task<bool> ConnectionCheck(CommandContext ctx, LavalinkGuildConnection conn)
        {
            if (ctx.Member != null && (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null))
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

        private static async Task<VoiceNextExtension> CheckAndGetVNext(CommandContext ctx)
        {
            // Check if VNext is enabled
            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return null;
            }

            return vnext;
        }

        private async Task<Audio> FillAudio(CommandContext ctx)
        {
            var conn = new Audio
            {
                Lava = ctx.Client.GetLavalink(),
            };
            if (!conn.Lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("Can't connect to Lavalink");
                throw new Exception("Can't connect to Lavalink");
            }

            conn.Node = conn.Lava.ConnectedNodes.Values.First();
            if (ctx.Member != null) conn.Conn = conn.Node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            return conn;
        }

        private async Task Conn_PlaybackFinished(LavalinkGuildConnection conn, TrackFinishEventArgs e)
        {
            if (_trackQueue.Count != 0)
            {
                await PlayFromQueue(conn);
            }
        }

        private async Task PlayFromQueue(LavalinkGuildConnection conn)
        {
            var request = (TrackRequest)_trackQueue.Dequeue();
            if (_loop)
            {
                _trackQueue.Enqueue(request);
            }

            var track = request?.GetRequestTrack();
            var ctx = request?.GetRequestCtx();
            if (conn.CurrentState.CurrentTrack == null)
            {
                await conn.PlayAsync(track);

                if (!_loop)
                {
                    await ctx.RespondAsync($"Now playing {track?.Title}!");
                }

                conn.PlaybackFinished += Conn_PlaybackFinished;
            }
        }

        private static async Task<bool> AnyTracksLoaded(CommandContext ctx, LavalinkGuildConnection conn)
        {
            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return true;
            }

            return false;
        }
    }
}