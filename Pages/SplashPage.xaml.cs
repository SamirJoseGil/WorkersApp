using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using WorkersApp.Services;
using WorkersApp.Models;

namespace WorkersApp.Pages
{
    public partial class SplashPage : ContentPage
    {
        public SplashPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            LoadingConfigLabel.IsVisible = true;
            await Task.Delay(3000); // Espera 3 segundos

            var configManager = new ConfigManager();
            var config = await configManager.LoadConfigAsync();

            if (config == null)
            {
                Application.Current.MainPage = new ConfigPage(); // Cambia a la p�gina de configuraci�n
            }
            else
            {
                Application.Current.MainPage = new NavigationPage(new LoginPage()); // Cambia a la p�gina de inicio de sesi�n
            }
        }
    }
}
