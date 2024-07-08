using Microsoft.Maui.Controls;
using System;
using System.IO;
using System.Net.Sockets;
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
        private CancellationTokenSource uploadCancellationTokenSource;

        public MainPage()
        {
            InitializeComponent();
            CompanyNameEntry.TextChanged += OnCompanyNameEntryTextChanged;
            LoadConfiguration();
        }

        // Cargar la configuración de usuario
        private async void LoadConfiguration()
        {
            ShowBottomRightMessage("Cargando configuraciones de usuario...");
            await Task.Delay(1000); // Espera de 1 segundo para simular carga
            ConfigManager configManager = new ConfigManager();
            config = await configManager.LoadConfigAsync();
            if (config == null)
            {
                await DisplayAlert("Error", "Error al cargar la configuración.", "OK");
                return;
            }
            HideBottomRightMessage();
        }

        // Entrada de texto de la empresa
        private void OnCompanyNameEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CompanyNameEntry.Text))
            {
                ErrorMessageLabel.Text = "Por favor, ingrese el nombre de la empresa.";
                ErrorMessageLabel.IsVisible = true;
                SelectFileButton.IsVisible = false;
                ActionButtonsStack.IsVisible = false;
                FileInfoStack.IsVisible = false;
            }
            else
            {
                ErrorMessageLabel.IsVisible = false;
                SelectFileButton.IsVisible = true;
                ActionButtonsStack.IsVisible = false;
                FileInfoStack.IsVisible = false;
            }
        }

        // Botón de seleccionar archivo
        private async void OnSelectFileButtonClicked(object sender, EventArgs e)
        {
            var result = await FilePicker.Default.PickAsync();
            if (result != null)
            {
                selectedFilePath = result.FullPath;
                var fileInfo = new FileInfo(selectedFilePath);
                SelectedFilePathLabel.Text = Path.GetFileName(selectedFilePath);
                FileSizeLabel.Text = $"{(fileInfo.Length / (1024.0 * 1024.0)):F2} MB";
                SelectedFilePathLabel.IsVisible = true;
                FileSizeLabel.IsVisible = true;
                FileInfoStack.IsVisible = true;
                SelectFileButton.IsVisible = false;
                ActionButtonsStack.IsVisible = true;
                DeleteFileButton.IsVisible = true;
                UploadFileButton.IsVisible = true;
                totalBytesToTransfer = fileInfo.Length;
            }
        }

        // Botón de eliminar archivo
        private void OnDeleteFileButtonClicked(object sender, EventArgs e)
        {
            selectedFilePath = null;
            SelectedFilePathLabel.Text = "Archivo no seleccionado";
            FileSizeLabel.Text = "0 MB";
            SelectedFilePathLabel.IsVisible = false;
            FileSizeLabel.IsVisible = false;
            FileInfoStack.IsVisible = false;
            SelectFileButton.IsVisible = true;
            ActionButtonsStack.IsVisible = false;
            NotificationLabel.IsVisible = false;
        }

        // Botón de subir archivo
        private async void OnUploadButtonClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CompanyNameEntry.Text))
                {
                    await DisplayAlert("Error", "Por favor, ingrese el nombre de la empresa.", "OK");
                    return;
                }

                if (selectedFilePath == null)
                {
                    await DisplayAlert("Error", "Por favor, seleccione un archivo.", "OK");
                    return;
                }

                NotificationLabel.Text = "Cargando archivo...";
                NotificationLabel.IsVisible = true;

                await Task.Delay(2000); // Espera de 2 segundos para simular carga

                NotificationLabel.IsVisible = false;
                UploadFileButton.IsVisible = false;
                SelectFileButton.IsVisible = false;
                DeleteFileButton.IsVisible = false;
                PauseUploadButton.IsVisible = true;
                ProgressStack.IsVisible = true;

                fileUploader = new FileUploader(UploadProgressBar, ProgressPercentageLabel, config);
                uploadCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(30));
                var progress = new Progress<long>(bytesTransferred =>
                {
                    ProgressCallback(bytesTransferred);
                });

                await Task.Run(() => fileUploader.UploadFileAsync(selectedFilePath, CompanyNameEntry.Text, uploadCancellationTokenSource.Token, progress));

                await DisplayAlert("Éxito", "Archivo subido correctamente.", "OK");
            }
            catch (OperationCanceledException)
            {
                // Manejo de pausa sin mostrar mensaje de error
                PauseUploadButton.IsVisible = false;
                UploadFileButton.IsVisible = false;
                DeleteFileButton.IsVisible = false;
            }
            catch (SocketException)
            {
                await DisplayAlert("Error", "No se pudo conectar al servidor. Por favor, inténtelo de nuevo más tarde.", "OK");
            }
            catch (IOException)
            {
                await DisplayAlert("Error", "Ocurrió un error al leer o escribir el archivo. Por favor, inténtelo de nuevo.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
            }
            finally
            {
                UploadFileButton.IsVisible = true;
                SelectFileButton.IsVisible = false;
                DeleteFileButton.IsVisible = true;
                PauseUploadButton.IsVisible = false;
                ProgressStack.IsVisible = false;
                ProgressPercentageLabel.Text = "0%";
                UploadProgressBar.Progress = 0;
            }
        }

        // Botón de pausa
        private void OnPauseUploadButtonClicked(object sender, EventArgs e)
        {
            uploadCancellationTokenSource?.Cancel();
        }

        // Llamado a la barra de progreso
        private void ProgressCallback(long bytesTransferred)
        {
            double progress = (double)bytesTransferred / totalBytesToTransfer;
            Device.BeginInvokeOnMainThread(() =>
            {
                UploadProgressBar.Progress = progress;
                ProgressPercentageLabel.Text = $"{progress:P2}";
            });
        }

        // Mostrar mensaje de carga de usuario
        private void ShowBottomRightMessage(string message)
        {
            BottomRightMessageLabel.Text = message;
            BottomRightMessageLabel.IsVisible = true;
        }

        private void HideBottomRightMessage()
        {
            BottomRightMessageLabel.IsVisible = false;
        }
    }
}
