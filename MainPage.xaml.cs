using Microsoft.Maui.Controls;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace WorkersApp
{
    public partial class MainPage : ContentPage
    {
        private string selectedFilePath;
        private readonly string[] allowedExtensions = { ".rar", ".zip" };

        public MainPage()
        {
            InitializeComponent();
            CompanyNumberEntry.TextChanged += OnCompanyNumberEntryTextChanged;
        }

        private void OnCompanyNumberEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            if (CompanyNumberEntry.Text.Length == App.PasswordLength)
            {
                SelectFileButton.IsVisible = true;
            }
            else
            {
                SelectFileButton.IsVisible = false;
                UploadFileLayout.IsVisible = false;
                UploadFileButton.IsVisible = false;
                DeleteFileButton.IsVisible = false;
                UploadProgressBar.IsVisible = false;
                FileSizeLabel.IsVisible = false;
            }
        }

        private async void OnSelectFileButtonClicked(object sender, EventArgs e)
        {
            var result = await FilePicker.Default.PickAsync();
            if (result != null)
            {
                selectedFilePath = result.FullPath;
                if (IsAllowedFileType(selectedFilePath))
                {
                    var fileInfo = new FileInfo(selectedFilePath);
                    SelectedFilePathLabel.Text = Path.GetFileName(selectedFilePath);
                    FileSizeLabel.Text = $"Tamaño del archivo: {fileInfo.Length / (1024.0 * 1024.0):F2} MB";
                    FileSizeLabel.IsVisible = true;
                    DeleteFileButton.IsVisible = true;
                    UploadFileButton.IsVisible = true;
                    UploadFileLayout.IsVisible = true;
                    UploadProgressBar.IsVisible = true;
                }
                else
                {
                    await DisplayAlert("Error", "Tipo de archivo no permitido. Seleccione un archivo .rar o .zip.", "OK");
                    selectedFilePath = null;
                    SelectedFilePathLabel.Text = "Ningún archivo seleccionado";
                    FileSizeLabel.IsVisible = false;
                }
            }
        }

        private void OnDeleteFileButtonClicked(object sender, EventArgs e)
        {
            selectedFilePath = null;
            SelectedFilePathLabel.Text = "Ningún archivo seleccionado";
            DeleteFileButton.IsVisible = false;
            UploadFileButton.IsVisible = false;
            UploadFileLayout.IsVisible = false;
            FileSizeLabel.IsVisible = false;
        }

        private async void OnUploadButtonClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CompanyNumberEntry.Text))
            {
                await DisplayAlert("Error", "Por favor, ingrese el número de la empresa.", "OK");
                return;
            }

            if (CompanyNumberEntry.Text.Length != App.PasswordLength)
            {
                await DisplayAlert("Error", $"El número de la empresa debe tener {App.PasswordLength} dígitos.", "OK");
                return;
            }

            if (selectedFilePath == null)
            {
                await DisplayAlert("Error", "Por favor, seleccione un archivo.", "OK");
                return;
            }

            try
            {
                UploadProgressBar.IsVisible = true;
                UploadProgressBar.Progress = 0;
                await UploadFileAsync(selectedFilePath, CompanyNumberEntry.Text);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
            }
            finally
            {
                UploadProgressBar.IsVisible = false;
            }
        }

        private bool IsAllowedFileType(string filePath)
        {
            string fileExtension = Path.GetExtension(filePath).ToLower();
            foreach (var extension in allowedExtensions)
            {
                if (fileExtension == extension)
                {
                    return true;
                }
            }
            return false;
        }

        private async Task UploadFileAsync(string filePath, string companyNumber)
        {
            var serverAddress = "127.0.0.1";
            var port = 5000;
            var fileInfo = new FileInfo(filePath);
            var totalBytes = fileInfo.Length;
            var bufferSize = 4096;
            var buffer = new byte[bufferSize];

            try
            {
                using (var client = new TcpClient())
                {
                    await client.ConnectAsync(serverAddress, port);
                    using (var networkStream = client.GetStream())
                    {
                        var companyNumberBytes = System.Text.Encoding.UTF8.GetBytes(companyNumber + "\n");
                        await networkStream.WriteAsync(companyNumberBytes, 0, companyNumberBytes.Length);

                        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            int bytesRead;
                            long totalBytesRead = 0;

                            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                totalBytesRead += bytesRead;
                                await networkStream.WriteAsync(buffer, 0, bytesRead);
                                UploadProgressBar.Progress = (double)totalBytesRead / totalBytes;
                            }
                        }
                    }
                }

                await DisplayAlert("Estado de la subida", "Archivo subido correctamente", "OK");
            }
            catch (SocketException se)
            {
                await DisplayAlert("Error", $"Error de socket: {se.Message}", "OK");
            }
            catch (IOException ioe)
            {
                await DisplayAlert("Error", $"Error de IO: {ioe.Message}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
            }
        }
    }
}
