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

        // Constructor que inicializa los campos con las referencias proporcionadas.
        public FileUploader(ProgressBar progressBar, Label progressLabel, Configuration config)
        {
            // Verifica que los parámetros no sean NULL.
            _progressBar = progressBar ?? throw new ArgumentNullException(nameof(progressBar));
            _progressLabel = progressLabel ?? throw new ArgumentNullException(nameof(progressLabel));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        // Método asincrónico para subir un archivo en partes.
        public async Task UploadFileAsync(string filePath, string companyName, CancellationToken cancellationToken, IProgress<long> progress, long resumeFromBytes = 0)
        {
            const int chunkSize = 5 * 1024 * 1024; // 5 MB
            try
            {
                var fileInfo = new FileInfo(filePath);
                long totalBytes = fileInfo.Length;
                long totalBytesSent = resumeFromBytes;

                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    fileStream.Seek(resumeFromBytes, SeekOrigin.Begin);
                    int bytesRead;
                    byte[] buffer = new byte[chunkSize];

                    while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        using (var client = new HttpClient())
                        {
                            client.Timeout = TimeSpan.FromMinutes(60); // Aumenta el tiempo de espera

                            var serverUrl = $"http://{_config.ServerIP}:{_config.ServerPort}/api/fileupload";

                            using (var content = new MultipartFormDataContent())
                            {
                                // Agrega los datos del formulario.
                                content.Add(new StringContent(companyName), "companyName");
                                content.Add(new StringContent(resumeFromBytes.ToString()), "resumeFromBytes");

                                var streamContent = new StreamContent(new MemoryStream(buffer, 0, bytesRead));
                                content.Add(streamContent, "file", Path.GetFileName(filePath));

                                // Envía la solicitud POST al servidor.
                                var response = await client.PostAsync(serverUrl, content, cancellationToken);
                                response.EnsureSuccessStatusCode();

                                // Actualiza el progreso.
                                totalBytesSent += bytesRead;
                                resumeFromBytes += bytesRead;
                                progress?.Report(totalBytesSent);

                                // Actualiza la barra de progreso y la etiqueta.
                                _progressBar.Progress = (double)totalBytesSent / totalBytes;
                                _progressLabel.Text = $"{(double)totalBytesSent / totalBytes:P0}";
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw new Exception("La carga del archivo fue cancelada.");
            }
            catch (HttpRequestException ex)
            {
                // Maneja errores específicos de la solicitud HTTP, como problemas de conexión.
                if (ex.Message.Contains("No se puede establecer una conexión"))
                {
                    throw new Exception("No se pudo conectar con el servidor. Por favor, intente nuevamente más tarde.");
                }
                throw new Exception($"Error de conexión: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Maneja cualquier excepción y lanza una nueva excepción con un mensaje de error en español.
                string errorMessage = ex.Message switch
                {
                    "A task was canceled." => "La carga del archivo fue cancelada.",
                    _ => $"Error durante la subida del archivo: {ex.Message}"
                };
                Console.WriteLine(errorMessage);
                throw new Exception(errorMessage);
            }
        }
    }
}
