using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using WorkersApp.Models;

namespace WorkersApp.Services
{
    public class ConfigManager
    {
        private const string ConfigFileName = "config.json"; // Nombre del archivo de configuraci�n

        // M�todo para guardar la configuraci�n de forma asincr�nica
        public async Task SaveConfigAsync(Configuration config)
        {
            try
            {
                // Serializa la configuraci�n a formato JSON
                string configJson = JsonSerializer.Serialize(config);
                // Obtiene la ruta completa del archivo de configuraci�n
                string configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ConfigFileName);
                // Escribe el JSON en el archivo
                await File.WriteAllTextAsync(configFilePath, configJson);
            }
            catch (Exception ex)
            {
                // Lanza una nueva excepci�n con un mensaje de error
                throw new Exception("Error saving config: " + ex.Message);
            }
        }

        // M�todo para cargar la configuraci�n de forma asincr�nica
        public async Task<Configuration?> LoadConfigAsync()
        {
            try
            {
                // Obtiene la ruta completa del archivo de configuraci�n
                string configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ConfigFileName);
                // Verifica si el archivo de configuraci�n existe
                if (!File.Exists(configFilePath))
                {
                    // Retorna un objeto Configuration vac�o si el archivo no existe
                    return new Configuration();
                }

                // Lee el contenido del archivo de configuraci�n
                string configJson = await File.ReadAllTextAsync(configFilePath);
                // Deserializa el JSON a un objeto Configuration
                var config = JsonSerializer.Deserialize<Configuration>(configJson);
                // Verifica que la deserializaci�n no haya retornado null
                return config ?? new Configuration();
            }
            catch (Exception ex)
            {
                // Lanza una nueva excepci�n con un mensaje de error
                throw new Exception("Error loading config: " + ex.Message);
            }
        }
    }
}

