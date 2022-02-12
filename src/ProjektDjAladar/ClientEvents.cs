using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ProjektDjAladar
{
    class ClientEvents
    {
        private readonly EventId BotEventId = new EventId(42, "ProjektDjAladar");

        public Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            sender.Logger.LogInformation(BotEventId, "Client is ready to process events.");
            return Task.CompletedTask;
        }

        public Task Client_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            sender.Logger.LogInformation(BotEventId, $"Guild available: {e.Guild.Name}");
            return Task.CompletedTask;
        }

        public Task Client_ClientError(DiscordClient sender, ClientErrorEventArgs e)
        {
            sender.Logger.LogError(BotEventId, e.Exception, "Exception occured");
            return Task.CompletedTask;
        }
    }
}