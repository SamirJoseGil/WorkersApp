using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using WorkersApp.Pages;
using WorkersApp.Services;

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
            await Task.Delay(3000); // Espera 3 segundos

            ConfigManager configManager = new ConfigManager();
            Configuration config = await configManager.LoadConfigAsync();

            if (config == null)
            {
                Application.Current.MainPage = new ConfigPage(); // Cambia a la página de configuración
            }
            else
            {
                Application.Current.MainPage = new MainPage(); // Cambia a la página principal
            }
        }
    }
}
