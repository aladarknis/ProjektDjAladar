using System.Diagnostics;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using DSharpPlus.VoiceNext;

namespace ProjektDjAladar
{
    public class Program
    {


        public DiscordClient Client { get; set; }
        public CommandsNextExtension Commands { get; set; }
        public VoiceNextExtension Voice { get; set; }

        public static void Main(string[] args)
        {
            new Program().RunBotAsync().GetAwaiter().GetResult();
        }

        public async Task RunBotAsync()
        {
            JsonSettings Settings = new JsonSettings();
            ClientEvents ClientEve = new ClientEvents();
            CommandEvents CommandEve = new CommandEvents();

            this.Client = new DiscordClient(Settings.GetDiscordConfiguration());

            this.Client.Ready += ClientEve.Client_Ready;
            this.Client.GuildAvailable += ClientEve.Client_GuildAvailable;
            this.Client.ClientErrored += ClientEve.Client_ClientError;

            var ccfg = new CommandsNextConfiguration
            {
                StringPrefixes = new[] { Settings.LoadedSettings.CommandPrefix },

                EnableDms = true,

                EnableMentionPrefix = true
            };

            this.Commands = this.Client.UseCommandsNext(ccfg);

            this.Commands.CommandExecuted += CommandEve.Commands_CommandExecuted;
            this.Commands.CommandErrored += CommandEve.Commands_CommandErrored;

            this.Commands.RegisterCommands<VoiceCommands>();

            this.Voice = this.Client.UseVoiceNext();

            await this.Client.ConnectAsync();

            //var lavalinkProcress = new Process();
            //lavalinkProcress.StartInfo.FileName = "java";
            //lavalinkProcress.StartInfo.Arguments = @"-jar " + "Lavalink/Lavalink.jar";
            //lavalinkProcress.Start();


            var endpoint = new ConnectionEndpoint
            {
                Hostname = "127.0.0.1",
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
            await Task.Delay(-1);
        }
    }
}