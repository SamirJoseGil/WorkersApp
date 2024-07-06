using Microsoft.Maui.Controls;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WorkersApp.Services;

namespace WorkersApp
{
    public partial class MainPage : ContentPage
    {
        private string selectedFilePath;
        private FileUploader fileUploader;
        private long totalBytesToTransfer;
        private Configuration config;

        public MainPage()
        {
            InitializeComponent();
            CompanyNumberEntry.TextChanged += OnCompanyNumberEntryTextChanged;
            LoadConfiguration();
        }

        private async void LoadConfiguration()
        {
            ConfigManager configManager = new ConfigManager();
            config = await configManager.LoadConfigAsync();
            if (config == null)
            {
                await DisplayAlert("Error", "Error al cargar la configuración.", "OK");
            }
        }

        // Maneja el evento de cambio de texto del campo de número de empresa
        private void OnCompanyNumberEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            int length = CompanyNumberEntry.Text.Length;

            if (length != App.PasswordLength)
            {
                // Muestra un mensaje de error si la longitud del número de empresa es incorrecta
                ErrorMessageLabel.Text = $"El número de la empresa debe tener {App.PasswordLength} caracteres.";
                ErrorMessageLabel.IsVisible = true;
                SelectFileButton.IsVisible = false;
                UploadFileButton.IsVisible = false;
            }
            else
            {
                // Oculta el mensaje de error y muestra los botones de selección y subida de archivo
                ErrorMessageLabel.IsVisible = false;
                SelectFileButton.IsVisible = true;
                UploadFileButton.IsVisible = true;
            }
        }

        // Maneja el evento de clic del botón para seleccionar archivo
        private async void OnSelectFileButtonClicked(object sender, EventArgs e)
        {
            var result = await FilePicker.Default.PickAsync();
            if (result != null)
            {
                // Actualiza la UI con la información del archivo seleccionado
                selectedFilePath = result.FullPath;
                var fileInfo = new FileInfo(selectedFilePath);
                SelectedFilePathLabel.Text = Path.GetFileName(selectedFilePath);
                FileSizeLabel.Text = $"Tamaño del archivo: {fileInfo.Length / (1024.0 * 1024.0):F2} MB";
                SelectedFilePathLabel.IsVisible = true;
                FileSizeLabel.IsVisible = true;
                UploadFileButton.IsVisible = true;
                totalBytesToTransfer = fileInfo.Length;
            }
        }

        // Maneja el evento de clic del botón para eliminar el archivo seleccionado
        private void OnDeleteFileButtonClicked(object sender, EventArgs e)
        {
            // Restablece la UI cuando se elimina un archivo
            selectedFilePath = null;
            SelectedFilePathLabel.Text = "Ningún archivo seleccionado";
            SelectedFilePathLabel.IsVisible = false;
            UploadFileButton.IsVisible = false;
        }

        // Maneja el evento de clic del botón para subir el archivo
        private async void OnUploadButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // Verifica que el número de la empresa esté completo
                if (string.IsNullOrWhiteSpace(CompanyNumberEntry.Text))
                {
                    await DisplayAlert("Error", "Por favor, ingrese el número de la empresa.", "OK");
                    return;
                }

                // Verifica que el número de la empresa tenga la longitud correcta
                if (CompanyNumberEntry.Text.Length != App.PasswordLength)
                {
                    await DisplayAlert("Error", $"El número de la empresa debe tener {App.PasswordLength} caracteres.", "OK");
                    return;
                }

                // Verifica que se haya seleccionado un archivo
                if (selectedFilePath == null)
                {
                    await DisplayAlert("Error", "Por favor, seleccione un archivo.", "OK");
                    return;
                }

                // Oculta botones y muestra la barra de progreso
                UploadFileButton.IsVisible = false;
                SelectFileButton.IsVisible = false;
                ProgressStack.IsVisible = true;

                // Inicia la subida del archivo en un hilo separado
                fileUploader = new FileUploader(UploadProgressBar, ProgressPercentageLabel, config);
                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(30)); // Aumenta el tiempo de espera a 30 minutos
                var progress = new Progress<long>(bytesTransferred =>
                {
                    ProgressCallback(bytesTransferred);
                });

                await Task.Run(() => fileUploader.UploadFileAsync(selectedFilePath, CompanyNumberEntry.Text, cancellationTokenSource.Token, progress));

                // Muestra mensaje de éxito
                await DisplayAlert("Éxito", "Archivo subido correctamente.", "OK");
            }
            catch (Exception ex)
            {
                // Muestra mensaje de error
                await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
            }
            finally
            {
                // Restaura visibilidad de botones y oculta barra de progreso
                UploadFileButton.IsVisible = true;
                SelectFileButton.IsVisible = true;
                ProgressStack.IsVisible = false;

                // Limpia el texto de progreso
                ProgressPercentageLabel.Text = "0%";
                UploadProgressBar.Progress = 0;
            }
        }

        // Método para actualizar la barra de progreso
        private void ProgressCallback(long bytesTransferred)
        {
            double progress = (double)bytesTransferred / totalBytesToTransfer;
            Device.BeginInvokeOnMainThread(() =>
            {
                UploadProgressBar.Progress = progress;
                ProgressPercentageLabel.Text = $"{progress:P2}";
            });
        }
    }
}
