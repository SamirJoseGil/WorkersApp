using Microsoft.Maui.Controls;
using System.IO;
using Newtonsoft.Json.Linq; // Importar el espacio de nombres necesario
using Application = Microsoft.Maui.Controls.Application;

namespace WorkersApp
{
    public partial class App : Application
    {
        public static int PasswordLength { get; private set; }

        public App()
        {
            InitializeComponent();
            LoadSettings();
            MainPage = new SplashPage();
        }

        private void LoadSettings()
        {
            var settingsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AppSettings.json");
            if (File.Exists(settingsFile))
            {
                var json = File.ReadAllText(settingsFile);
                var settings = JObject.Parse(json);
                PasswordLength = (int)settings["PasswordLength"];
            }
            else
            {
                PasswordLength = 6; // Default value
            }
        }
    }
}
