using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using WorkersApp.Models;

namespace WorkersApp.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _serverUrl;

        public AuthService(string serverUrl)
        {
            _httpClient = new HttpClient();
            _serverUrl = serverUrl; // URL del servidor donde se valida la autenticación.
        }

        public async Task<(User? user, string? errorMessage)> AuthenticateUserAsync(string username, string password)
        {
            var credentials = new
            {
                Username = username,
                Password = password
            };

            try
            {
                // Realiza una solicitud POST al servidor con las credenciales.
                var response = await _httpClient.PostAsJsonAsync($"{_serverUrl}/api/auth/validate", credentials);

                if (response.IsSuccessStatusCode)
                {
                    // Si la respuesta es exitosa, deserializa y devuelve el usuario.
                    var user = await response.Content.ReadFromJsonAsync<User>();
                    return (user, null);
                }
                else
                {
                    // Si la respuesta indica un fallo, intenta leer el mensaje de error como JSON.
                    var errorContent = await response.Content.ReadAsStringAsync();
                    string? errorMessage;

                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent);
                        errorMessage = errorResponse?.Message;
                    }
                    catch (JsonException)
                    {
                        // Si la deserialización falla, trata el contenido como texto plano.
                        errorMessage = errorContent;
                    }

                    return (null, errorMessage);
                }
            }
            catch (HttpRequestException ex)
            {
                // Maneja errores específicos de la solicitud HTTP, como problemas de conexión.
                if (ex.Message.Contains("No se puede establecer una conexión"))
                {
                    return (null, "No se pudo conectar con el servidor. Por favor, intente nuevamente más tarde.");
                }
                return (null, $"Error de conexión: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Maneja cualquier otro tipo de error inesperado.
                return (null, $"Error inesperado: {ex.Message}");
            }
        }
    }

    // Clase para deserializar la respuesta de error del servidor.
    public class ErrorResponse
    {
        public string? Message { get; set; }
    }
}
