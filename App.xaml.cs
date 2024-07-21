using Microsoft.Maui.Controls;
using WorkersApp.Pages;

namespace WorkersApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new SplashPage();
        }
    }
}
