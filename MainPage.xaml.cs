using Microsoft.Maui.Controls;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace WorkersApp
{
    public partial class MainPage : ContentPage
    {
        private string selectedFilePath;
        // Archivos permitidos
        private readonly string[] allowedExtensions = { ".rar", ".zip" };

        public MainPage()
        {
            InitializeComponent();
            CompanyNumberEntry.TextChanged += OnCompanyNumberEntryTextChanged;
        }

        // Label del numero de la compania
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
            }
        }

        // Boton seleccionar Archivo
        private async void OnSelectFileButtonClicked(object sender, EventArgs e)
        {
            var result = await FilePicker.Default.PickAsync();
            if (result != null)
            {
                selectedFilePath = result.FullPath;
                if (IsAllowedFileType(selectedFilePath))
                {
                    SelectedFilePathLabel.Text = Path.GetFileName(selectedFilePath);
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
                }
            }
        }


        // Boton seleccionar Archivo
        private void OnDeleteFileButtonClicked(object sender, EventArgs e)
        {
            selectedFilePath = null;
            SelectedFilePathLabel.Text = "Ningún archivo seleccionado";
            DeleteFileButton.IsVisible = false;
            UploadFileButton.IsVisible = false;
            UploadFileLayout.IsVisible = false;
        }


        // Boton subir Archivo
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

        // Verificar la extension del archivo ".rar"
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


        // Conexion con el servicio
        private async Task UploadFileAsync(string filePath, string companyNumber)
        {
            // Url
            var serverUrl = $"http://localhost:8081/files/upload/{companyNumber}";
            var fileInfo = new FileInfo(filePath);
            var totalBytes = fileInfo.Length;
            var bufferSize = 4096;
            var buffer = new byte[bufferSize];

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {   
                // Time out 30 seg
                using (var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
                {
                    var content = new MultipartFormDataContent();
                    var streamContent = new StreamContent(fileStream);
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    content.Add(streamContent, "file", Path.GetFileName(filePath));

                    var progressContent = new ProgressableStreamContent(content, bufferSize, (sentBytes) =>
                    {
                        UploadProgressBar.Progress = (double)sentBytes / totalBytes;
                    });

                    var response = await httpClient.PostAsync(serverUrl, progressContent);
                    var responseMessage = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        await DisplayAlert("Error", "El archivo ya existe.", "OK");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.UnsupportedMediaType)
                    {
                        await DisplayAlert("Error", "Tipo de archivo no permitido.", "OK");
                    }
                    else if (!response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Error", responseMessage, "OK");
                    }
                    else
                    {
                        await DisplayAlert("Estado de la subida", responseMessage, "OK");
                    }
                }
            }
        }
    }

    // Barra de progreso
    public class ProgressableStreamContent : HttpContent
    {
        private readonly HttpContent _content;
        private readonly int _bufferSize;
        private readonly Action<long> _progress;

        public ProgressableStreamContent(HttpContent content, int bufferSize, Action<long> progress)
        {
            _content = content;
            _bufferSize = bufferSize;
            _progress = progress;

            foreach (var header in _content.Headers)
            {
                Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            var buffer = new byte[_bufferSize];
            TryComputeLength(out var size);
            var uploaded = 0;

            using (var contentStream = await _content.ReadAsStreamAsync())
            {
                while (true)
                {
                    var length = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                    if (length <= 0) break;

                    uploaded += length;
                    _progress?.Invoke(uploaded);

                    await stream.WriteAsync(buffer, 0, length);
                    await stream.FlushAsync();
                }
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}
