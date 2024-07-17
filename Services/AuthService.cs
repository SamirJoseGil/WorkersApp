using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace WorkersApp.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _serverUrl;

        public AuthService(string serverUrl)
        {
            _httpClient = new HttpClient();
            _serverUrl = serverUrl;
        }

        public async Task<(User user, string errorMessage)> AuthenticateUserAsync(string username, string password)
        {
            var credentials = new
            {
                Username = username,
                Password = password
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_serverUrl}/api/auth/validate", credentials);
                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<User>();
                    return (user, null);
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return (null, errorMessage);
                }
            }
            catch (HttpRequestException ex)
            {
                return (null, $"Error de conexión: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (null, $"Error inesperado: {ex.Message}");
            }
        }
    }

    public class User
    {
        public string Username { get; set; }
        public string CompanyNumber { get; set; }
    }
}
