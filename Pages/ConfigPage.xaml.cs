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

        private async void OnSaveConfigButtonClicked(object sender, EventArgs e)
        {
            string serverIP = ServerIpEntry.Text;
            if (!int.TryParse(ServerPortEntry.Text, out int serverPort))
            {
                await DisplayAlert("Error", "El puerto del servidor no es válido.", "OK");
                return;
            }

            var config = new Configuration { ServerIP = serverIP, ServerPort = serverPort };
            var configManager = new ConfigManager();
            await configManager.SaveConfigAsync(config);

            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
    }
}
