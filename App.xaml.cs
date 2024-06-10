using Microsoft.Maui.Controls;
using Application = Microsoft.Maui.Controls.Application;

namespace WorkersApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new SplashPage(); // Configurar SplashPage como página inicial
        }
    }
}
