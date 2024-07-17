using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using WorkersApp.Models;

namespace WorkersApp.Services
{
    public class ConfigManager
    {
        private const string ConfigFileName = "config.json";

        public async Task SaveConfigAsync(Configuration config)
        {
            try
            {
                string configJson = JsonSerializer.Serialize(config);
                string configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ConfigFileName);
                await File.WriteAllTextAsync(configFilePath, configJson);
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving config: " + ex.Message);
            }
        }

        public async Task<Configuration> LoadConfigAsync()
        {
            try
            {
                string configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ConfigFileName);
                if (!File.Exists(configFilePath))
                {
                    return null;
                }

                string configJson = await File.ReadAllTextAsync(configFilePath);
                return JsonSerializer.Deserialize<Configuration>(configJson);
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading config: " + ex.Message);
            }
        }
    }
}
