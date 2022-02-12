using DSharpPlus.CommandsNext;
using DSharpPlus.Lavalink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektDjAladar
{
    public class TrackRequest
    {
        private readonly CommandContext _ctx;
        private readonly LavalinkTrack _track;

        public TrackRequest(CommandContext ctx, LavalinkTrack track)
        {
            _ctx = ctx;
            _track = track;
        }

        public CommandContext GetRequestCtx()
        {
            return _ctx;
        }

        public LavalinkTrack GetRequestTrack()
        {
            return _track;
        }
    }
}