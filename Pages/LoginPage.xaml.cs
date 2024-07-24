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
        // Campos para la configuraci�n y el servicio de autenticaci�n.
        private Configuration _config = new Configuration(); // Inicializaci�n
        private AuthService _authService = new AuthService(""); // Inicializaci�n con un valor por defecto

        public LoginPage()
        {
            InitializeComponent();
            // Carga la configuraci�n al iniciar.
            LoadConfiguration();
        }

        // Carga la configuraci�n de forma asincr�nica.
        private async void LoadConfiguration()
        {
            var configManager = new ConfigManager();
            var loadedConfig = await configManager.LoadConfigAsync();
            if (loadedConfig == null)
            {
                await DisplayAlert("Error", "Error al cargar la configuraci�n.", "OK");
                return;
            }
            _config = loadedConfig;
            // Inicializa el servicio de autenticaci�n con la configuraci�n cargada.
            _authService = new AuthService($"http://{_config.ServerIP}:{_config.ServerPort}");
        }

        // Maneja el evento de clic en el bot�n de inicio de sesi�n.
        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            // Verifica que los campos no est�n vac�os.
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
                    // Si la autenticaci�n es exitosa, navega a la p�gina principal.
                    await Navigation.PushAsync(new MainPage(user.Username, user.CompanyName));
                }
                else
                {
                    // Muestra un mensaje de error si la autenticaci�n falla.
                    await DisplayAlert("Error", errorMessage ?? "Credenciales inv�lidas.", "OK");
                }
            }
            catch (HttpRequestException ex)
            {
                // Maneja errores de conexi�n.
                if (ex.Message.Contains("deneg� expresamente"))
                {
                    await DisplayAlert("Error", "No se pudo conectar con el servidor. Por favor, intente nuevamente m�s tarde.", "OK");
                }
                else
                {
                    await DisplayAlert("Error", $"Error de conexi�n: {ex.Message}", "OK");
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

        // Maneja el evento de clic en el bot�n de configuraciones.
        private async void OnSettingsButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ConfigPage());
        }
    }
}

