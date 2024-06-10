using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace WorkersApp
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
            await Task.Delay(5000); // Espera 3 segundos
            Application.Current.MainPage = new AppShell(); // Cambia a la página principal
        }
    }
}
