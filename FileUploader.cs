using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace WorkersApp
{
    public class FileUploader
    {
        private readonly ProgressBar uploadProgressBar;
        private readonly Label progressPercentageLabel;

        public FileUploader(ProgressBar progressBar, Label progressLabel)
        {
            uploadProgressBar = progressBar;
            progressPercentageLabel = progressLabel;
        }

        // Sube el archivo al servidor
        public async Task UploadFileAsync(string filePath, string companyNumber, CancellationToken cancellationToken, IProgress<long> progress)
        {
            var serverAddress = "127.0.0.1";
            var port = 5000;

            try
            {
                using (var client = new TcpClient())
                {
                    // Configurar timeout de 30 segundos
                    var timeout = TimeSpan.FromSeconds(30);
                    using (var cancellationTokenSource = new CancellationTokenSource(timeout))
                    {
                        await client.ConnectAsync(serverAddress, port, cancellationTokenSource.Token);
                        using (var networkStream = client.GetStream())
                        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            // Enviar número de empresa
                            var companyNumberBytes = System.Text.Encoding.UTF8.GetBytes(companyNumber + "\n");
                            await networkStream.WriteAsync(companyNumberBytes, 0, companyNumberBytes.Length, cancellationToken);

                            // Transferir archivo usando Stream y reportar progreso
                            var totalBytes = fileStream.Length;
                            progress.Report(0); // Iniciar progreso en 0%
                            long bytesTransferred = 0;
                            byte[] buffer = new byte[4096];
                            int bytesRead;
                            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                            {
                                await networkStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                                bytesTransferred += bytesRead;
                                progress.Report(bytesTransferred); // Reportar el progreso actual
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw new Exception("Imposible conectar al servidor. Por favor, inténtelo de nuevo más tarde o revise su conexión a internet.");
            }
            catch (SocketException se)
            {
                throw new Exception($"Error de socket: {se.Message}");
            }
            catch (IOException ioe)
            {
                throw new Exception($"Error de IO: {ioe.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: {ex.Message}");
            }
        }
    }
}
