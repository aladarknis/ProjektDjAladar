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
            new Program().RunBotAsync(args).GetAwaiter().GetResult();
        }

        private async Task RunBotAsync(string[] args)
        {
            JsonSettings = new JsonSettings();
            if (!SetToken()) return;

            ClientEvents clientEve = new ClientEvents();
            CommandEvents commandEve = new CommandEvents();
            this.Client = new DiscordClient(GetDiscordConfiguration());
            this.Client.Ready += clientEve.Client_Ready;
            this.Client.GuildAvailable += clientEve.Client_GuildAvailable;
            this.Client.ClientErrored += clientEve.Client_ClientError;
            this.Commands = this.Client.UseCommandsNext(GetCommandsNextConfiguration());
            this.Commands.CommandExecuted += commandEve.Commands_CommandExecuted;
            this.Commands.CommandErrored += commandEve.Commands_CommandErrored;
            this.Commands.RegisterCommands<VoiceCommands>();
            this.Voice = this.Client.UseVoiceNext();
            await this.Client.ConnectAsync();

            ConnectToLavalink();
            SetBotPlaying();
            
            await Task.Delay(-1);
        }

        private CommandsNextConfiguration GetCommandsNextConfiguration()
        {
            return new CommandsNextConfiguration
            {
                StringPrefixes = new[] {JsonSettings.LoadedSettings.CommandPrefix},
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

        private async void SetDiscordPlaying(string hash, string prefix)
        {
            DiscordActivity activity = new DiscordActivity
            {
                Name = $"{prefix}help | {hash}"
            };
            await this.Client.UpdateStatusAsync(activity);
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

        private void SetBotPlaying()
        {
            string hash = ProcessRunner.RunProcess("git", "rev-parse HEAD");
            SetDiscordPlaying(hash?.Substring(0, 7), JsonSettings.LoadedSettings.CommandPrefix);
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