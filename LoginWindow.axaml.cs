using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using LabelMatchingSvcPartAvalonia.Controllers;
using LabelMatchingSvcPartAvalonia.Models;

namespace LabelMatchingSvcPartAvalonia
{
    public partial class LoginWindow : Window
    {
        private ConfigController _config = new ConfigController();

        public LoginWindow()
        {
            InitializeComponent();

            // Fokus otomatis ke kotak input saat window terbuka
            this.Opened += (s, e) => txtUser.Focus();
        }

        private void OnLoginClick(object? sender, RoutedEventArgs e) => DoLogin();

        private void OnUserKeyDown(object? sender, KeyEventArgs e)
        {
            // Jika scanner QR mengirimkan tombol Enter
            if (e.Key == Key.Enter)
            {
                ParseUsername();
                DoLogin(); // Langsung proses login
            }
        }

        // Ekstrak 5 Digit ID Terakhir dari hasil Scan
        private void ParseUsername()
        {
            string rawUser = txtUser.Text ?? "";

            if (rawUser.Contains("|"))
            {
                string[] parts = rawUser.Split('|');
                string idPart = parts[0];

                if (idPart.Length >= 5)
                {
                    txtUser.Text = idPart.Substring(idPart.Length - 5);
                }
                else
                {
                    txtUser.Text = idPart;
                }

                txtUser.CaretIndex = txtUser.Text.Length;
            }
        }

        private void DoLogin()
        {
            ParseUsername(); // Pastikan data sudah terekstrak

            string user = txtUser.Text ?? "";

            // Karena password dihilangkan, kita kirimkan string kosong 
            // atau bisa juga mengirim password default jika diatur di DB
            string pass = "";

            if (string.IsNullOrEmpty(user))
            {
                lblMsg.Text = "Username/ID is required.";
                return;
            }

            if (_config.TryLogin(user, out string message))
            {
                // Buka MainWindow
                var main = new MainWindow();
                main.Show();

                // Tutup Jendela Login
                this.Close();
            }
            else
            {
                lblMsg.Text = message;
                txtUser.SelectAll();
                txtUser.Focus();
            }
        }
    }
}