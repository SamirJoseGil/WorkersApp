using Microsoft.Maui.Controls;
using System;
using WorkersApp.Models;
using WorkersApp.Services;

namespace WorkersApp.Pages
{
    public partial class ConfigPage : ContentPage
    {
        public ConfigPage()
        {
            InitializeComponent();
        }

        // Maneja el evento de clic en el botón de guardar configuración.
        private async void OnSaveConfigButtonClicked(object sender, EventArgs e)
        {
            // Obtiene la dirección IP del servidor desde el campo de entrada.
            string serverIP = ServerIpEntry.Text;

            // Verifica que el puerto del servidor sea un número válido.
            if (!int.TryParse(ServerPortEntry.Text, out int serverPort))
            {
                await DisplayAlert("Error", "El puerto del servidor no es válido.", "OK");
                return;
            }

            // Crea una nueva configuración con los valores ingresados.
            var config = new Configuration { ServerIP = serverIP, ServerPort = serverPort };

            // Guarda la configuración de forma asincrónica.
            var configManager = new ConfigManager();
            await configManager.SaveConfigAsync(config);

            // Verifica que Application.Current no sea NULL antes de acceder a MainPage.
            if (Application.Current != null)
            {
                // Navega a la página de inicio de sesión.
                Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
            else
            {
                await DisplayAlert("Error", "No se pudo cambiar la página principal.", "OK");
            }
        }
    }
}
