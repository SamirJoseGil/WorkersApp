﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace WorkersApp.Services
{
    public class FileUploader
    {
        private readonly ProgressBar uploadProgressBar;
        private readonly Label progressPercentageLabel;
        private readonly Configuration config;

        public FileUploader(ProgressBar progressBar, Label progressLabel, Configuration configuration)
        {
            uploadProgressBar = progressBar;
            progressPercentageLabel = progressLabel;
            config = configuration ?? throw new ArgumentNullException(nameof(configuration), "La configuración no puede ser nula");
        }

        // Método para subir el archivo al servidor
        public async Task UploadFileAsync(string filePath, string companyName, CancellationToken cancellationToken, IProgress<long> progress)
        {
            string serverAddress = config.ServerIP;
            int port = config.ServerPort;
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
                        // Enviar nombre de la empresa
                        binaryWriter.Write(companyName);
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