using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using WorkersApp.Models;

namespace WorkersApp.Services
{
    public class FileUploader
    {
        private readonly ProgressBar _progressBar;
        private readonly Label _progressLabel;
        private readonly Configuration _config;

        public FileUploader(ProgressBar progressBar, Label progressLabel, Configuration config)
        {
            _progressBar = progressBar;
            _progressLabel = progressLabel;
            _config = config;
        }

        public async Task UploadFileAsync(string filePath, string companyName, string username, string password, CancellationToken cancellationToken, IProgress<long> progress)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(30);
                    var serverUrl = $"http://{_config.ServerIP}:{_config.ServerPort}/api/upload";
                    using (var content = new MultipartFormDataContent())
                    {
                        content.Add(new StringContent(companyName), "CompanyName");
                        content.Add(new StringContent(username), "Username");
                        content.Add(new StringContent(password), "Password");
                        content.Add(new StreamContent(File.OpenRead(filePath)), "file", Path.GetFileName(filePath));

                        var response = await client.PostAsync(serverUrl, content, cancellationToken);
                        response.EnsureSuccessStatusCode();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al subir el archivo: " + ex.Message);
            }
        }
    }
}