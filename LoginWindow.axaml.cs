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
            txtUser.Focus();
        }

        private void OnLoginClick(object? sender, RoutedEventArgs e) => DoLogin();

        private void OnPassKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) DoLogin();
        }

        private void DoLogin()
        {
            string user = txtUser.Text ?? "";
            string pass = txtPass.Text ?? "";

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                lblMsg.Text = "Username and Password are required.";
                return;
            }

            if (_config.TryLogin(user, pass, out string message))
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
                txtPass.Text = "";
                txtPass.Focus();
            }
        }
    }
}