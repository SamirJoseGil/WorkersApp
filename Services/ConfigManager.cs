using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using WorkersApp.Models;

namespace WorkersApp.Services
{
    public class ConfigManager
    {
        private const string ConfigFileName = "config.json"; // Nombre del archivo de configuración

        // Método para guardar la configuración de forma asincrónica
        public async Task SaveConfigAsync(Configuration config)
        {
            try
            {
                // Serializa la configuración a formato JSON
                string configJson = JsonSerializer.Serialize(config);
                // Obtiene la ruta completa del archivo de configuración
                string configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ConfigFileName);
                // Escribe el JSON en el archivo
                await File.WriteAllTextAsync(configFilePath, configJson);
            }
            catch (Exception ex)
            {
                // Lanza una nueva excepción con un mensaje de error
                throw new Exception("Error saving config: " + ex.Message);
            }
        }

        // Método para cargar la configuración de forma asincrónica
        public async Task<Configuration?> LoadConfigAsync()
        {
            try
            {
                // Obtiene la ruta completa del archivo de configuración
                string configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ConfigFileName);
                // Verifica si el archivo de configuración existe
                if (!File.Exists(configFilePath))
                {
                    // Retorna un objeto Configuration vacío si el archivo no existe
                    return new Configuration();
                }

                // Lee el contenido del archivo de configuración
                string configJson = await File.ReadAllTextAsync(configFilePath);
                // Deserializa el JSON a un objeto Configuration
                var config = JsonSerializer.Deserialize<Configuration>(configJson);
                // Verifica que la deserialización no haya retornado null
                return config ?? new Configuration();
            }
            catch (Exception ex)
            {
                // Lanza una nueva excepción con un mensaje de error
                throw new Exception("Error loading config: " + ex.Message);
            }
        }
    }
}

