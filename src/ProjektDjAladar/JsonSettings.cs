
using DSharpPlus;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ProjektDjAladar
{
	class JsonSettings
	{
		private string JsonPath;

		public struct ConfigJson
		{
			[JsonProperty("token")]
			public string Token { get; private set; }

			[JsonProperty("prefix")]
			public string CommandPrefix { get; private set; }
		}

		public ConfigJson LoadedSettings;


		public JsonSettings()
		{
			JsonPath = "config.json";
			LoadSettings();
		}

		public JsonSettings(string path)
		{
			JsonPath = path;
			LoadSettings();
		}

		private void LoadSettings()
		{
			var json = "";
			using (var fs = File.OpenRead(JsonPath))
			using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
				json = sr.ReadToEnd();
			LoadedSettings = JsonConvert.DeserializeObject<ConfigJson>(json);
		}

		public DiscordConfiguration GetDiscordConfiguration()
		{
			var cfg = new DiscordConfiguration
			{
				Token = LoadedSettings.Token,
				TokenType = TokenType.Bot,

				AutoReconnect = true,
				MinimumLogLevel = LogLevel.Debug,
			};
			return cfg;
		}
	}
}
