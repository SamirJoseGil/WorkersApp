﻿using Microsoft.Maui.Controls;
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

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnSelectFileButtonClicked(object sender, EventArgs e)
        {
            var result = await FilePicker.Default.PickAsync();
            if (result != null)
            {
                selectedFilePath = result.FullPath;
                SelectedFilePathLabel.Text = Path.GetFileName(selectedFilePath);
            }
        }

        private void OnDeleteFileButtonClicked(object sender, EventArgs e)
        {
            selectedFilePath = null;
            SelectedFilePathLabel.Text = "No file selected";
        }

        private async void OnUploadButtonClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CompanyNumberEntry.Text))
            {
                await DisplayAlert("Error", "Please enter a company number.", "OK");
                return;
            }

            if (CompanyNumberEntry.Text.Length != 6)
            {
                await DisplayAlert("Error", "Company number must be 6 digits long.", "OK");
                return;
            }

            if (selectedFilePath == null)
            {
                await DisplayAlert("Error", "Please select a file.", "OK");
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
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
            finally
            {
                UploadProgressBar.IsVisible = false;
            }
        }

        private async Task UploadFileAsync(string filePath, string companyNumber)
        {
            var serverUrl = $"http://localhost:8081/files/upload/{companyNumber}";
            var fileInfo = new FileInfo(filePath);
            var totalBytes = fileInfo.Length;
            var bufferSize = 4096;
            var buffer = new byte[bufferSize];

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var httpClient = new HttpClient())
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
                await DisplayAlert("Upload Status", responseMessage, "OK");
            }
        }
    }

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
