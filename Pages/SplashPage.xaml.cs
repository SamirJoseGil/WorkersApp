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
            LoadingConfigLabel.IsVisible = true; // Muestra el mensaje de carga
            await Task.Delay(3000); // Espera 3 segundos

            var configManager = new ConfigManager();
            var config = await configManager.LoadConfigAsync();

            if (config == null)
            {
                // Cambia a la página de configuración
                Application.Current!.MainPage = new ConfigPage();
            }
            else
            {
                // Cambia a la página de inicio de sesión
                Application.Current!.MainPage = new NavigationPage(new LoginPage());
            }
        }
    }
}
