using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ProjektDjAladar
{
    class CommandEvents
    {
        private readonly EventId BotEventId = new EventId(42, "Bot-Ex04");

        public Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            e.Context.Client.Logger.LogInformation(BotEventId, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'");
            return Task.CompletedTask;
        }

        public async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            e.Context.Client.Logger.LogError(BotEventId, $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

            if (e.Exception is ChecksFailedException ex)
            {
                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Access denied",
                    Description = $"{emoji} You do not have the permissions required to execute this command.",
                    Color = new DiscordColor(0xFF0000)
                };
                await e.Context.RespondAsync(embed);
            }
        }
    }
}
