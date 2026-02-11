using Avalonia.Controls;
using Avalonia.Interactivity;
using LabelMatchingSvcPartAvalonia.Controllers;
using LabelMatchingSvcPartAvalonia.Models;

namespace LabelMatchingSvcPartAvalonia
{
    public partial class SettingsWindow : Window
    {
        private ConfigController _controller;
        public AppConfig SavedConfig { get; private set; } // Property untuk mengirim data balik

        public SettingsWindow()
        {
            InitializeComponent();
            _controller = new ConfigController();

            // Fokus otomatis ke input username saat window terbuka
            this.Opened += (s, e) => txtUser.Focus();
        }

        private void OnLoginClick(object? sender, RoutedEventArgs e)
        {
            string user = txtUser.Text ?? "";
            string pass = txtPass.Text ?? "";

            if (_controller.ValidateLogin(user, pass))
            {
                // Login Sukses
                lblLoginMsg.Text = "";

                // Pindah Tampilan ke Config
                pnlLogin.IsVisible = false;
                pnlConfig.IsVisible = true;

                // Load data yang ada di file JSON ke TextBox
                txtLineName.Text = _controller.CurrentConfig.LineName;
                txtDbConnection.Text = _controller.CurrentConfig.DbConnectionString;
            }
            else
            {
                lblLoginMsg.Text = "Username atau Password salah!";
                txtPass.Text = "";
                txtPass.Focus();
            }
        }

        private void OnSaveClick(object? sender, RoutedEventArgs e)
        {
            // Buat object config baru dari inputan user
            var newConfig = new AppConfig
            {
                LineName = txtLineName.Text ?? "F1",
                DbConnectionString = txtDbConnection.Text ?? ""
            };

            // Simpan ke JSON via Controller
            _controller.SaveConfig(newConfig);

            // Set property public agar MainWindow tahu ada perubahan
            SavedConfig = newConfig;

            this.Close(); // Tutup Window
        }

        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            this.Close(); // Tutup tanpa simpan
        }
    }
}