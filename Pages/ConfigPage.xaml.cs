using Microsoft.Maui.Controls;
using System;
using WorkersApp.Services;

namespace WorkersApp.Pages
{
    public partial class ConfigPage : ContentPage
    {
        public ConfigPage()
        {
            InitializeComponent();
        }

        private async void OnSaveConfigButtonClicked(object sender, EventArgs e)
        {
            string serverIP = ServerIpEntry.Text;
            if (!int.TryParse(ServerPortEntry.Text, out int serverPort))
            {
                await DisplayAlert("Error", "El puerto del servidor no es válido.", "OK");
                return;
            }

            Configuration config = new Configuration { ServerIP = serverIP, ServerPort = serverPort };
            ConfigManager configManager = new ConfigManager();
            await configManager.SaveConfigAsync(config);

            Application.Current.MainPage = new MainPage(); // Cambia a la página principal
        }
    }
}
