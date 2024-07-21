using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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

        public async Task UploadFileAsync(string filePath, string companyName, CancellationToken cancellationToken, IProgress<long> progress)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(60); // Aumenta el tiempo de espera

                    var serverUrl = $"http://{_config.ServerIP}:{_config.ServerPort}/api/fileupload";

                    using (var content = new MultipartFormDataContent())
                    {
                        content.Add(new StringContent(companyName), "companyName");

                        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                        var streamContent = new StreamContent(fileStream);
                        content.Add(streamContent, "file", Path.GetFileName(filePath));

                        Console.WriteLine($"Sending request to {serverUrl}");
                        Console.WriteLine($"CompanyName: {companyName}");
                        Console.WriteLine($"FilePath: {filePath}");

                        var response = await client.PostAsync(serverUrl, content, cancellationToken);
                        response.EnsureSuccessStatusCode();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during file upload: {ex.Message}");
                throw new Exception("Error al subir el archivo: " + ex.Message);
            }
        }
    }
}
