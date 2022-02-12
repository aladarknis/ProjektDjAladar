using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace ProjektDjAladar
{
    class JsonSettings
    {
        private string JsonPath;

        public struct ConfigJson
        {
            [JsonProperty("lavalink_addr")] public string LavalinkAddr { get; private set; }

            [JsonProperty("prefix")] public string CommandPrefix { get; private set; }

            [JsonProperty("vymitani")] public string VymitaniUrl { get; private set; }
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
    }
}