using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

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

        // Método para subir el archivo al servidor
        public async Task UploadFileAsync(string filePath, string companyNumber, CancellationToken cancellationToken, IProgress<long> progress)
        {
            // Dirección IP pública
            string serverAddress = "181.138.138.58";
            int port = 5001;
            long fileSize = new FileInfo(filePath).Length;
            long offset = 0;

            try
            {
                using (var client = new TcpClient())
                {
                    await client.ConnectAsync(serverAddress, port, cancellationToken);

                    using (var networkStream = client.GetStream())
                    using (var binaryWriter = new BinaryWriter(networkStream))
                    using (var binaryReader = new BinaryReader(networkStream))
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        // Enviar número de empresa
                        binaryWriter.Write(companyNumber);
                        // Enviar nombre del archivo
                        binaryWriter.Write(Path.GetFileName(filePath));
                        // Enviar tamaño del archivo
                        binaryWriter.Write(fileSize);

                        // Leer el estado del servidor
                        string serverResponse = binaryReader.ReadString();
                        if (serverResponse == "El archivo ya existe y está completo")
                        {
                            Console.WriteLine("El archivo ya existe y está completo.");
                            return;
                        }
                        else if (serverResponse != "Continuar")
                        {
                            Console.WriteLine("Error: " + serverResponse);
                            return;
                        }

                        // Leer el offset del servidor
                        offset = binaryReader.ReadInt64();
                        fileStream.Seek(offset, SeekOrigin.Begin);

                        // Transferir archivo en paquetes pequeños y reportar progreso
                        byte[] buffer = new byte[5 * 1024 * 1024]; // Paquetes de 5 MB
                        int bytesRead;
                        long bytesTransferred = offset;

                        while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                        {
                            await networkStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                            bytesTransferred += bytesRead;
                            progress.Report(bytesTransferred); // Reportar el progreso actual

                            // Mostrar progreso en consola
                            Console.WriteLine($"Progreso: {bytesTransferred}/{fileSize} bytes ({(bytesTransferred * 100.0 / fileSize):F2}%)");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: {ex.Message}");
            }
        }
    }
}
