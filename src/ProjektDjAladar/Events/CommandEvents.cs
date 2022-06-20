using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace ProjektDjAladar
{
    internal class CommandEvents : Event
    {
        public Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            e.Context.Client.Logger.LogInformation(
                BotEventId, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'"
            );
            return Task.CompletedTask;
        }

        public async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            var username = e.Context.User.Username;
            var qualifiedName = e.Command?.QualifiedName ?? "<unknown command>";
            var errorMessage = e.Exception.Message;
            e.Context.Client.Logger.LogError(BotEventId,
                $"{username} tried executing '{qualifiedName}' but it errored: {e.Exception.GetType()}: {errorMessage}",
                DateTime.Now
            );

            if (e.Exception is ChecksFailedException)
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