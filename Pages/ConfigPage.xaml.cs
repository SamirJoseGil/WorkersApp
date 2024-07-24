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

        // Maneja el evento de clic en el bot�n de guardar configuraci�n.
        private async void OnSaveConfigButtonClicked(object sender, EventArgs e)
        {
            // Obtiene la direcci�n IP del servidor desde el campo de entrada.
            string serverIP = ServerIpEntry.Text;

            // Verifica que el puerto del servidor sea un n�mero v�lido.
            if (!int.TryParse(ServerPortEntry.Text, out int serverPort))
            {
                await DisplayAlert("Error", "El puerto del servidor no es v�lido.", "OK");
                return;
            }

            // Crea una nueva configuraci�n con los valores ingresados.
            var config = new Configuration { ServerIP = serverIP, ServerPort = serverPort };

            // Guarda la configuraci�n de forma asincr�nica.
            var configManager = new ConfigManager();
            await configManager.SaveConfigAsync(config);

            // Verifica que Application.Current no sea NULL antes de acceder a MainPage.
            if (Application.Current != null)
            {
                // Navega a la p�gina de inicio de sesi�n.
                Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
            else
            {
                await DisplayAlert("Error", "No se pudo cambiar la p�gina principal.", "OK");
            }
        }
    }
}
