using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using WorkersApp.Services;
using WorkersApp.Models;

namespace WorkersApp.Pages
{
    public partial class MainPage : ContentPage
    {
        private string _selectedFilePath;
        private long _totalBytesToTransfer;
        private CancellationTokenSource _uploadCancellationTokenSource;
        private Configuration _config;
        private readonly string _username;
        private readonly string _companyNumber;
        private string _password; // Definimos _password

        public MainPage(string username, string companyNumber)
        {
            InitializeComponent();
            _username = username;
            _companyNumber = companyNumber;
            LoadConfiguration();
        }

        private async void LoadConfiguration()
        {
            ShowBottomRightMessage("Cargando configuraciones de usuario...");
            await Task.Delay(1000);
            var configManager = new ConfigManager();
            _config = await configManager.LoadConfigAsync();
            if (_config == null)
            {
                await DisplayAlert("Error", "Error al cargar la configuración.", "OK");
                return;
            }
            HideBottomRightMessage();
        }

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

        private async void OnSelectFileButtonClicked(object sender, EventArgs e)
        {
            var result = await FilePicker.Default.PickAsync();
            if (result != null)
            {
                _selectedFilePath = result.FullPath;
                var fileInfo = new FileInfo(_selectedFilePath);
                SelectedFilePathLabel.Text = Path.GetFileName(_selectedFilePath);
                FileSizeLabel.Text = $"{(fileInfo.Length / (1024.0 * 1024.0)):F2} MB";
                SelectedFilePathLabel.IsVisible = true;
                FileSizeLabel.IsVisible = true;
                FileInfoStack.IsVisible = true;
                SelectFileButton.IsVisible = false;
                ActionButtonsStack.IsVisible = true;
                DeleteFileButton.IsVisible = true;
                UploadFileButton.IsVisible = true;
                _totalBytesToTransfer = fileInfo.Length;
            }
        }

        private void OnDeleteFileButtonClicked(object sender, EventArgs e)
        {
            _selectedFilePath = null;
            SelectedFilePathLabel.Text = "Archivo no seleccionado";
            FileSizeLabel.Text = "0 MB";
            SelectedFilePathLabel.IsVisible = false;
            FileSizeLabel.IsVisible = false;
            FileInfoStack.IsVisible = false;
            SelectFileButton.IsVisible = true;
            ActionButtonsStack.IsVisible = false;
            NotificationLabel.IsVisible = false;
        }

        private async void OnUploadButtonClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_companyNumber)) // Usamos _companyNumber en vez de CompanyNameEntry.Text
                {
                    await DisplayAlert("Error", "No se encontró el nombre de la empresa.", "OK");
                    return;
                }
                if (_selectedFilePath == null)
                {
                    await DisplayAlert("Error", "Por favor, seleccione un archivo.", "OK");
                    return;
                }

                var isValid = await ValidateCredentialsAsync(_username, _password, _companyNumber); // Usamos _companyNumber
                if (!isValid)
                {
                    await DisplayAlert("Error", "Credenciales inválidas.", "OK");
                    return;
                }

                NotificationLabel.Text = "Cargando archivo...";
                NotificationLabel.IsVisible = true;
                await Task.Delay(2000);
                NotificationLabel.IsVisible = false;
                UploadFileButton.IsVisible = false;
                SelectFileButton.IsVisible = false;
                DeleteFileButton.IsVisible = false;
                PauseUploadButton.IsVisible = true;
                ProgressStack.IsVisible = true;

                var progress = new Progress<long>(bytesTransferred =>
                {
                    ProgressCallback(bytesTransferred);
                });

                _uploadCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(30));
                var fileUploader = new FileUploader(UploadProgressBar, ProgressPercentageLabel, _config);
                await fileUploader.UploadFileAsync(_selectedFilePath, _companyNumber, _username, _password, _uploadCancellationTokenSource.Token, progress); // Usamos _companyNumber
                await DisplayAlert("Éxito", "Archivo subido correctamente.", "OK");
            }
            catch (OperationCanceledException)
            {
                PauseUploadButton.IsVisible = false;
                UploadFileButton.IsVisible = false;
                DeleteFileButton.IsVisible = false;
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message switch
                {
                    "No se pudo conectar al servidor" => "No se pudo conectar al servidor. Por favor, inténtelo de nuevo más tarde.",
                    "Tiempo de espera agotado" => "El tiempo de espera se ha agotado. Por favor, inténtelo de nuevo.",
                    _ => $"{ex.Message}"
                };
                await DisplayAlert("Error", errorMessage, "OK");
            }
            finally
            {
                UploadFileButton.IsVisible = true;
                SelectFileButton.IsVisible = true;
                DeleteFileButton.IsVisible = true;
                PauseUploadButton.IsVisible = false;
                ProgressStack.IsVisible = false;
            }
        }

        private async Task<bool> ValidateCredentialsAsync(string username, string password, string companyNumber)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var serverUrl = $"http://{_config.ServerIP}:{_config.ServerPort}/api/auth/validate";
                    var response = await client.PostAsJsonAsync(serverUrl, new { Username = username, Password = password });
                    if (response.IsSuccessStatusCode)
                    {
                        var user = await response.Content.ReadFromJsonAsync<User>();
                        if (user != null && user.CompanyNumber == companyNumber)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ProgressCallback(long bytesTransferred)
        {
            double progress = (double)bytesTransferred / _totalBytesToTransfer;
            UploadProgressBar.Progress = progress;
            ProgressPercentageLabel.Text = $"{progress:P2}";
        }

        private void OnPauseUploadButtonClicked(object sender, EventArgs e)
        {
            _uploadCancellationTokenSource?.Cancel();
            PauseUploadButton.IsVisible = false;
            UploadFileButton.IsVisible = true;
            SelectFileButton.IsVisible = true;
            DeleteFileButton.IsVisible = true;
            ProgressStack.IsVisible = false;
        }

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
