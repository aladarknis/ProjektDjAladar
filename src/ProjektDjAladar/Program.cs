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
        public DiscordClient Client { get; set; }
        public CommandsNextExtension Commands { get; set; }
        public VoiceNextExtension Voice { get; set; }
        public string token { get; set; }

        public static void Main(string[] args)
        {
            new Program().RunBotAsync(args).GetAwaiter().GetResult();
        }

        public DiscordConfiguration GetDiscordConfiguration()
        {
            var cfg = new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
            };
            return cfg;
        }

        public async void SetDiscordPlaying(string hash, string prefix)
        {
            DiscordActivity activity = new DiscordActivity();
            activity.Name = $"{prefix}help | {hash}";
            await this.Client.UpdateStatusAsync(activity);
        }

        public async Task RunBotAsync(string[] args)
        {
            token = Environment.GetEnvironmentVariable("ALADAR_BOT");
            if (token == "")
            {
                Console.WriteLine("Empty token, set an environment variable ALADAR_BOT");
                return;
            }

            Console.WriteLine($"api token: {token}");
            JsonSettings Settings = new JsonSettings();
            ClientEvents ClientEve = new ClientEvents();
            CommandEvents CommandEve = new CommandEvents();

            this.Client = new DiscordClient(GetDiscordConfiguration());

            this.Client.Ready += ClientEve.Client_Ready;
            this.Client.GuildAvailable += ClientEve.Client_GuildAvailable;
            this.Client.ClientErrored += ClientEve.Client_ClientError;

            var ccfg = new CommandsNextConfiguration
            {
                StringPrefixes = new[] {Settings.LoadedSettings.CommandPrefix},

                EnableDms = true,

                EnableMentionPrefix = true
            };

            this.Commands = this.Client.UseCommandsNext(ccfg);

            this.Commands.CommandExecuted += CommandEve.Commands_CommandExecuted;
            this.Commands.CommandErrored += CommandEve.Commands_CommandErrored;

            this.Commands.RegisterCommands<VoiceCommands>();

            this.Voice = this.Client.UseVoiceNext();

            await this.Client.ConnectAsync();
            
            var endpoint = new ConnectionEndpoint
            {
                Hostname = Settings.LoadedSettings.LavalinkAddr,
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
            string hash = ProcessRunner.RunProcess("git", "rev-parse HEAD");
            SetDiscordPlaying(hash?.Substring(0, 7), Settings.LoadedSettings.CommandPrefix);

            await Task.Delay(-1);
        }
    }
}