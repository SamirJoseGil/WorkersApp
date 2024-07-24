using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Windows.UI.ApplicationSettings;
using WorkersApp.Models;
using WorkersApp.Services;

namespace WorkersApp.Pages
{
    public partial class LoginPage : ContentPage
    {
        // Campos para la configuración y el servicio de autenticación.
        private Configuration _config = new Configuration(); // Inicialización
        private AuthService _authService = new AuthService(""); // Inicialización con un valor por defecto

        public LoginPage()
        {
            InitializeComponent();
            // Carga la configuración al iniciar.
            LoadConfiguration();
        }

        // Carga la configuración de forma asincrónica.
        private async void LoadConfiguration()
        {
            var configManager = new ConfigManager();
            var loadedConfig = await configManager.LoadConfigAsync();
            if (loadedConfig == null)
            {
                await DisplayAlert("Error", "Error al cargar la configuración.", "OK");
                return;
            }
            _config = loadedConfig;
            // Inicializa el servicio de autenticación con la configuración cargada.
            _authService = new AuthService($"http://{_config.ServerIP}:{_config.ServerPort}");
        }

        // Maneja el evento de clic en el botón de inicio de sesión.
        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            // Verifica que los campos no estén vacíos.
            string username = UsernameEntry.Text;
            string password = PasswordEntry.Text;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Por favor, complete todos los campos.", "OK");
                return;
            }

            try
            {
                // Intenta autenticar al usuario.
                var (user, errorMessage) = await _authService.AuthenticateUserAsync(username, password);
                if (user != null)
                {
                    // Si la autenticación es exitosa, navega a la página principal.
                    await Navigation.PushAsync(new MainPage(user.Username, user.CompanyName));
                }
                else
                {
                    // Muestra un mensaje de error si la autenticación falla.
                    await DisplayAlert("Error", errorMessage ?? "Credenciales inválidas.", "OK");
                }
            }
            catch (HttpRequestException ex)
            {
                // Maneja errores de conexión.
                if (ex.Message.Contains("denegó expresamente"))
                {
                    await DisplayAlert("Error", "No se pudo conectar con el servidor. Por favor, intente nuevamente más tarde.", "OK");
                }
                else
                {
                    await DisplayAlert("Error", $"Error de conexión: {ex.Message}", "OK");
                }
            }
            catch (Exception ex)
            {
                // Maneja cualquier otro tipo de error.
                await DisplayAlert("Error", $"Error inesperado: {ex.Message}", "OK");
            }
        }

        // Maneja el evento de cambio de estado del CheckBox.
        private void OnShowPasswordCheckBoxChanged(object sender, CheckedChangedEventArgs e)
        {
            PasswordEntry.IsPassword = !e.Value;
        }

        // Maneja el evento de clic en el botón de configuraciones.
        private async void OnSettingsButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ConfigPage());
        }
    }
}

