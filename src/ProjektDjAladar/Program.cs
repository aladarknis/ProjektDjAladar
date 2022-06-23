using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.Logging;

namespace ProjektDjAladar
{
    public class Program
    {
        private DiscordClient Client { get; set; }
        private CommandsNextExtension Commands { get; set; }
        private VoiceNextExtension Voice { get; set; }
        private string Token { get; set; }
        private JsonSettings JsonSettings { get; set; }

        public static void Main(string[] args)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    new Program().RunBotAsync(args).GetAwaiter().GetResult();
                }

                catch (Exception e)
                {

                    Console.WriteLine(e);
                }
            }
        }

        private async Task RunBotAsync(string[] args)
        {
            JsonSettings = new JsonSettings();
            if (!SetToken()) return;

            var clientEve = new ClientEvents();
            var commandEve = new CommandEvents();
            Client = new DiscordClient(GetDiscordConfiguration());
            Client.Ready += clientEve.Client_Ready;
            Client.GuildAvailable += clientEve.Client_GuildAvailable;
            Client.ClientErrored += clientEve.Client_ClientError;
            Commands = Client.UseCommandsNext(GetCommandsNextConfiguration());
            Commands.CommandExecuted += commandEve.Commands_CommandExecuted;
            Commands.CommandErrored += commandEve.Commands_CommandErrored;
            Commands.RegisterCommands<VoiceCommands>();
            Voice = Client.UseVoiceNext();
            await Client.ConnectAsync(GetBotPlayingMsg());

            ConnectToLavalink();
            await Task.Delay(-1);
        }

        private CommandsNextConfiguration GetCommandsNextConfiguration()
        {
            return new CommandsNextConfiguration
            {
                StringPrefixes = new[] { JsonSettings.LoadedSettings.CommandPrefix },
                EnableDms = true,
                EnableMentionPrefix = true
            };
        }

        private DiscordConfiguration GetDiscordConfiguration()
        {
            return new DiscordConfiguration
            {
                Token = Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
            };
        }

        private bool SetToken()
        {
            Token = Environment.GetEnvironmentVariable("ALADAR_BOT");
            if (Token == "")
            {
                Console.WriteLine("Empty token, set an environment variable ALADAR_BOT");
                return false;
            }

            Console.WriteLine($"api token: {Token}");
            return true;
        }

        private DiscordActivity GetBotPlayingMsg()
        {
            var hash = ProcessRunner.RunProcess("git", "rev-parse HEAD");
            var activity = new DiscordActivity
            {
                Name = $"{JsonSettings.LoadedSettings.CommandPrefix}help | {hash.Substring(0, 7)}"
            };
            return activity;
        }

        private async void ConnectToLavalink()
        {
            var endpoint = new ConnectionEndpoint
            {
                Hostname = JsonSettings.LoadedSettings.LavalinkAddr,
                Port = 2333
            };
            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = "youshallnotpass",
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            var lavalink = Client.UseLavalink();
            await lavalink.ConnectAsync(lavalinkConfig);
        }
    }
}