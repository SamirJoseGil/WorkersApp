using Microsoft.Maui.Controls;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace WorkersApp
{
    public partial class MainPage : ContentPage
    {
        private string selectedFilePath;
        private FileUploader fileUploader;
        private long totalBytesToTransfer;

        public MainPage()
        {
            InitializeComponent();
            CompanyNumberEntry.TextChanged += OnCompanyNumberEntryTextChanged;
        }

        // Valida la longitud del número de la empresa y muestra la UI de selección de archivo si es válido
        private void OnCompanyNumberEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            int length = CompanyNumberEntry.Text.Length;

            if (length != App.PasswordLength)
            {
                ErrorMessageLabel.Text = $"El número de la empresa debe tener {App.PasswordLength} caracteres.";
                ErrorMessageLabel.IsVisible = true;
                SelectFileButton.IsVisible = false;
                UploadFileButton.IsVisible = false; // Ocultar también el botón de subir archivo si el número no tiene la longitud correcta
            }
            else
            {
                ErrorMessageLabel.IsVisible = false;
                SelectFileButton.IsVisible = true;
                UploadFileButton.IsVisible = true; // Mostrar el botón de subir archivo cuando el número tiene la longitud correcta
            }
        }

        // Maneja la selección de archivo y actualiza la UI en consecuencia
        private async void OnSelectFileButtonClicked(object sender, EventArgs e)
        {
            var result = await FilePicker.Default.PickAsync();
            if (result != null)
            {
                selectedFilePath = result.FullPath;
                var fileInfo = new FileInfo(selectedFilePath);
                SelectedFilePathLabel.Text = Path.GetFileName(selectedFilePath);
                FileSizeLabel.Text = $"Tamaño del archivo: {fileInfo.Length / (1024.0 * 1024.0):F2} MB";
                SelectedFilePathLabel.IsVisible = true;
                FileSizeLabel.IsVisible = true;
                UploadFileButton.IsVisible = true;
            }
        }

        // Maneja la acción de eliminar archivo y actualiza la UI
        private void OnDeleteFileButtonClicked(object sender, EventArgs e)
        {
            selectedFilePath = null;
            SelectedFilePathLabel.Text = "Ningún archivo seleccionado";
            SelectedFilePathLabel.IsVisible = false;
            UploadFileButton.IsVisible = false;
        }

        // Maneja la acción de subir archivo y actualiza la UI
        private async void OnUploadButtonClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CompanyNumberEntry.Text))
                {
                    await DisplayAlert("Error", "Por favor, ingrese el número de la empresa.", "OK");
                    return;
                }

                if (CompanyNumberEntry.Text.Length != App.PasswordLength)
                {
                    await DisplayAlert("Error", $"El número de la empresa debe tener {App.PasswordLength} caracteres.", "OK");
                    return;
                }

                if (selectedFilePath == null)
                {
                    await DisplayAlert("Error", "Por favor, seleccione un archivo.", "OK");
                    return;
                }

                // Ocultar botones y mostrar progreso
                UploadFileButton.IsVisible = false;
                SelectFileButton.IsVisible = false;
                ProgressStack.IsVisible = true;

                // Iniciar la carga del archivo
                fileUploader = new FileUploader(UploadProgressBar, ProgressPercentageLabel);
                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // Timeout de 30 segundos
                var progress = new Progress<long>(bytesTransferred =>
                {
                    ProgressCallback(bytesTransferred);
                });

                await fileUploader.UploadFileAsync(selectedFilePath, CompanyNumberEntry.Text, cancellationTokenSource.Token, progress);

                // Mostrar mensaje de éxito si es necesario
                await DisplayAlert("Éxito", "Archivo subido correctamente.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
            }
            finally
            {
                // Restaurar visibilidad de botones y ocultar progreso al finalizar
                UploadFileButton.IsVisible = true;
                SelectFileButton.IsVisible = true;
                ProgressStack.IsVisible = false;
            }
        }

        // Método para actualizar la barra de progreso
        private void ProgressCallback(long bytesTransferred)
        {
            double progress = (double)bytesTransferred / totalBytesToTransfer; // Calcular el progreso real
            Device.BeginInvokeOnMainThread(() =>
            {
                UploadProgressBar.Progress = progress;
                ProgressPercentageLabel.Text = $"{progress:P2}";
            });
        }
    }
}
