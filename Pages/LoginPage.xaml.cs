using System;
using Microsoft.Maui.Controls;
using WorkersApp.Models;
using WorkersApp.Services;

namespace WorkersApp.Pages
{
    public partial class LoginPage : ContentPage
    {
        private Configuration _config;
        private AuthService _authService;

        public LoginPage()
        {
            InitializeComponent();
            LoadConfiguration();
        }

        private async void LoadConfiguration()
        {
            var configManager = new ConfigManager();
            _config = await configManager.LoadConfigAsync();
            if (_config == null)
            {
                await DisplayAlert("Error", "Error al cargar la configuración.", "OK");
                return;
            }
            _authService = new AuthService($"http://{_config.ServerIP}:{_config.ServerPort}");
        }

        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            string username = UsernameEntry.Text;
            string password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Por favor, complete todos los campos.", "OK");
                return;
            }

            try
            {
                var (user, errorMessage) = await _authService.AuthenticateUserAsync(username, password);
                if (user != null)
                {
                    await Navigation.PushAsync(new MainPage(user.Username, user.CompanyNumber));
                }
                else
                {
                    await DisplayAlert("Error", errorMessage ?? "Credenciales inválidas.", "OK");
                }
            }
            catch (HttpRequestException ex)
            {
                await DisplayAlert("Error", $"Error de conexión: {ex.Message}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error inesperado: {ex.Message}", "OK");
            }
        }
    }
}
