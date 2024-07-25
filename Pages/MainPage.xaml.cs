using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using WorkersApp.Models;
using WorkersApp.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WorkersApp.Pages
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private string? _selectedFilePath;
        private long _totalBytesToTransfer;
        private long _bytesTransferred;
        private CancellationTokenSource _uploadCancellationTokenSource;
        private Configuration? _config;
        private readonly string _username;
        private readonly string _companyName;
        private bool _isFilePickerOpen = false;
        private bool _isUploading;

        public bool IsNotUploading
        {
            get => !_isUploading;
            set
            {
                _isUploading = !value;
                OnPropertyChanged();
            }
        }

        public MainPage(string username, string companyName)
        {
            InitializeComponent();
            _username = username;
            _companyName = companyName;
            CompanyNameLabel.Text = _companyName;
            LoadConfiguration();
            ShowSelectFileButton();
            _uploadCancellationTokenSource = new CancellationTokenSource(); // Inicializa el campo
            BindingContext = this;
        }

        // Carga la configuración del usuario
        private async void LoadConfiguration()
        {
            await Task.Delay(1000);
            var configManager = new ConfigManager();
            _config = await configManager.LoadConfigAsync();
            if (_config == null)
            {
                await DisplayAlert("Error", "No se pudo cargar la configuración.", "OK");
                HideBottomRightMessage(); // Asegúrate de ocultar el mensaje en caso de error
                return;
            }
            HideBottomRightMessage();
        }

        // Muestra el botón de seleccionar archivo
        private void ShowSelectFileButton()
        {
            SelectFileButton.IsVisible = true;
            ActionButtonsStack.IsVisible = false;
            ProgressStack.IsVisible = false;
        }

        // Maneja el evento de clic del botón de seleccionar archivo
        private async void OnSelectFileButtonClicked(object sender, EventArgs e)
        {
            if (_isFilePickerOpen) return;
            _isFilePickerOpen = true;

            var result = await FilePicker.Default.PickAsync();
            _isFilePickerOpen = false;

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
                UploadFileButton.Text = "Subir archivo"; // Restablece el texto del botón
                _totalBytesToTransfer = fileInfo.Length;
                _bytesTransferred = await GetBytesTransferredAsync(_selectedFilePath);
            }
        }

        // Obtiene los bytes transferidos desde un archivo de progreso
        private async Task<long> GetBytesTransferredAsync(string filePath)
        {
            var progressFilePath = $"{filePath}.progress";
            if (File.Exists(progressFilePath))
            {
                var progressText = await File.ReadAllTextAsync(progressFilePath);
                return long.Parse(progressText);
            }
            return 0;
        }

        // Maneja el evento de clic del botón de eliminar archivo
        private void OnDeleteFileButtonClicked(object? sender, EventArgs? e)
        {
            _selectedFilePath = null;
            SelectedFilePathLabel.Text = "Archivo no seleccionado";
            FileSizeLabel.Text = "0 MB";
            SelectedFilePathLabel.IsVisible = false;
            FileSizeLabel.IsVisible = false;
            FileInfoStack.IsVisible = false;
            ShowSelectFileButton();
        }

        // Maneja el evento de clic del botón de subir archivo
        private async void OnUploadButtonClicked(object sender, EventArgs e)
        {
            if (_config == null)
            {
                await DisplayAlert("Error", "Error de configuración.", "OK");
                return;
            }
            if (string.IsNullOrEmpty(_selectedFilePath))
            {
                await DisplayAlert("Error", "Seleccione un archivo.", "OK");
                return;
            }

            IsNotUploading = false;
            UploadFileButton.IsVisible = false;
            SelectFileButton.IsVisible = false;
            DeleteFileButton.IsVisible = false;
            CancelUploadButton.IsVisible = true;
            ProgressStack.IsVisible = true;
            BackButton.IsVisible = false; // Oculta el botón "Volver Atrás" durante la carga

            var progress = new Progress<long>(bytesTransferred =>
            {
                _bytesTransferred = bytesTransferred;
                Dispatcher.Dispatch(() =>
                {
                    UploadProgressBar.Progress = (double)_bytesTransferred / _totalBytesToTransfer;
                    ProgressPercentageLabel.Text = $"{(double)_bytesTransferred / _totalBytesToTransfer:P0}";
                });
            });

            _uploadCancellationTokenSource = new CancellationTokenSource();

            try
            {
                var fileUploader = new FileUploader(UploadProgressBar, ProgressPercentageLabel, _config);
                await fileUploader.UploadFileAsync(_selectedFilePath, _companyName, _uploadCancellationTokenSource.Token, progress, _bytesTransferred);
                await DisplayAlert("Éxito", "Archivo subido.", "OK");
                File.Delete($"{_selectedFilePath}.progress");
                OnDeleteFileButtonClicked(this, EventArgs.Empty);
            }
            catch (OperationCanceledException)
            {
                await DisplayAlert("Cancelado", "Subida cancelada.", "OK");
                await SaveProgressAsync(_selectedFilePath, _bytesTransferred);
                ShowContinueOrDeleteButtons();
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse { Message = ex.Message };
                await DisplayAlert("Exepcion", errorResponse.Message, "OK");
                ShowContinueOrDeleteButtons();
            }
            finally
            {
                CancelUploadButton.IsVisible = false;
                ProgressStack.IsVisible = false;
                IsNotUploading = true;
                BackButton.IsVisible = true; // Muestra el botón "Volver Atrás" después de la carga
            }
        }

        // Maneja el evento de clic del botón de cancelar subida
        private void OnCancelUploadButtonClicked(object sender, EventArgs e)
        {
            _uploadCancellationTokenSource?.Cancel();
        }

        // Guarda el progreso de la subida en un archivo
        private async Task SaveProgressAsync(string filePath, long bytesTransferred)
        {
            await File.WriteAllTextAsync($"{filePath}.progress", bytesTransferred.ToString());
        }

        // Muestra el mensaje en la esquina inferior derecha
        private void ShowBottomRightMessage(string message)
        {
            BottomRightMessageLabel.Text = message;
            BottomRightMessageLabel.IsVisible = true;
        }

        // Oculta el mensaje en la esquina inferior derecha
        private void HideBottomRightMessage()
        {
            BottomRightMessageLabel.IsVisible = false;
        }

        // Muestra los botones de continuar o eliminar
        private void ShowContinueOrDeleteButtons()
        {
            UploadFileButton.Text = "Continuar";
            UploadFileButton.IsVisible = true;
            DeleteFileButton.IsVisible = true;
        }

        // Maneja el evento de clic del botón de regresar
        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        public new event PropertyChangedEventHandler? PropertyChanged;

        protected new void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
