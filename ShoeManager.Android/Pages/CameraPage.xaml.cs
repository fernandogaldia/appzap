using System;
using System.IO;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;

namespace ShoeManager.Android.Pages
{
    public partial class CameraPage : ContentPage
    {
        public string PhotoPath { get; private set; } = string.Empty;
        public bool IsFrontal { get; set; } = true;

        public CameraPage(bool isFrontal)
        {
            InitializeComponent();
            IsFrontal = isFrontal;
            PhotoTypeLabel.Text = IsFrontal ? "Foto: Frontal" : "Foto: Perfil";
        }

        private async void OnCaptureClicked(object sender, EventArgs e)
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.Camera>();
                    if (status != PermissionStatus.Granted)
                    {
                        await DisplayAlert("Permiso denegado", "Se requiere permiso de cámara", "OK");
                        return;
                    }
                }

                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    await DisplayAlert("No soportado", "La captura no está disponible", "OK");
                    return;
                }

                var photo = await MediaPicker.Default.CapturePhotoAsync();
                if (photo != null)
                {
                    string fileName = $"ShoeManager_{(IsFrontal ? "Frontal" : "Perfil")}_{DateTime.Now:yyyyMMddHHmmss}.jpg";
                    string localPath = Path.Combine(FileSystem.AppDataDirectory, "photos", fileName);
                    string? dir = Path.GetDirectoryName(localPath);
                    if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                    using var sourceStream = await photo.OpenReadAsync();
                    using var localStream = File.OpenWrite(localPath);
                    await sourceStream.CopyToAsync(localStream);

                    CapturedImage.Source = ImageSource.FromFile(localPath);
                    PhotoPath = localPath;
                    CaptureButton.Text = "📸 Retomar Foto";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo capturar: {ex.Message}", "OK");
            }
        }

        private async void OnUsePhotoClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PhotoPath))
            {
                await DisplayAlert("Sin foto", "Tome una foto primero", "OK");
                return;
            }
            await Navigation.PopAsync();
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            PhotoPath = string.Empty;
            await Navigation.PopAsync();
        }
    }
}