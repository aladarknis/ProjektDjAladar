using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace ProjektDjAladar
{
    public class JsonSettings
    {
        private readonly string _jsonPath;

        public struct ConfigJson
        {
            [JsonProperty("lavalink_addr")] public string LavalinkAddr { get; private set; }

            [JsonProperty("prefix")] public string CommandPrefix { get; private set; }

            [JsonProperty("vymitani")] public string VymitaniUrl { get; private set; }
        }

        public ConfigJson LoadedSettings;


        public JsonSettings()
        {
            _jsonPath = "config.json";
            LoadSettings();
        }

        public JsonSettings(string path)
        {
            _jsonPath = path;
            LoadSettings();
        }

        private void LoadSettings()
        {
            string json;
            using (var fs = File.OpenRead(_jsonPath))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = sr.ReadToEnd();
            LoadedSettings = JsonConvert.DeserializeObject<ConfigJson>(json);
        }
    }
}