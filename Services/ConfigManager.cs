using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace WorkersApp
{
    public class ConfigManager
    {
        private readonly string configFilePath;

        public ConfigManager()
        {
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            string configDir = Path.Combine(exePath, "userConfig");
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }
            configFilePath = Path.Combine(configDir, "config.json");
        }

        public async Task<Configuration> LoadConfigAsync()
        {
            if (File.Exists(configFilePath))
            {
                string configJson = await File.ReadAllTextAsync(configFilePath);
                return JsonSerializer.Deserialize<Configuration>(configJson);
            }
            else
            {
                return null;
            }
        }

        public async Task SaveConfigAsync(Configuration config)
        {
            string configJson = JsonSerializer.Serialize(config);
            await File.WriteAllTextAsync(configFilePath, configJson);
        }
    }

    public class Configuration
    {
        public string ServerIP { get; set; }
        public int ServerPort { get; set; }
    }
}
