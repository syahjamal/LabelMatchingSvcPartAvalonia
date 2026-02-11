using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using LabelMatchingSvcPartAvalonia.Controllers;
using LabelMatchingSvcPartAvalonia.Models; // Pastikan namespace ini ada
using System;

namespace LabelMatchingSvcPartAvalonia
{
    public partial class MainWindow : Window
    {
        // Controller Utama
        private ScanController controller = new ScanController();

        // Controller Konfigurasi (Untuk Database & Line Name)
        private ConfigController configController = new ConfigController();

        // --- PALET WARNA (Sesuai Brand Daikin & Indikator Industri) ---
        private IBrush ColorReady = Brush.Parse("#A0A0A0");   // Abu-abu (Standby)
        private IBrush ColorProcess = Brush.Parse("#00A0E4"); // German Blue (Sedang Proses)
        private IBrush ColorOK = Brush.Parse("#6CC24A");      // Hijau (OK)
        private IBrush ColorNG = Brush.Parse("#E04F5F");      // Merah (NG)
        private IBrush ColorWarning = Brush.Parse("#FF8C00"); // Orange (Error)

        public MainWindow()
        {
            InitializeComponent();

            SetupAwal();
            lblOperatorName.Text = UserSession.FullName;
            lblLineName.Text = UserSession.LineName;
            LoadSystemConfig();

            this.KeyDown += OnWindowKeyDown;
            this.Opened += (s, e) =>
            {
                txtInput1.IsEnabled = true;
                txtInput1.Focus();
            };
        }

        // --- BAGIAN KONFIGURASI ---
        private void LoadSystemConfig()
        {
            // 1. Load Local Config (Untuk dapat Connection String & Line Name)
            configController.LoadConfig();

            // 2. Set Line Name (Dari Local Config)
            lblLineName.Text = configController.CurrentConfig.LineName;

            // 3. Set Program Title (Coba Ambil dari Database)
            // Logika: Ambil Connection String dari Local -> Konek DB -> Ambil Title
            string dbTitle = configController.GetProgramTitleFromDb();

            // Set Title Window
            this.Title = dbTitle;
        }

        private async void OnSettingsClick(object? sender, RoutedEventArgs e)
        {
            // Buka Window Setting sebagai Pop-up Dialog
            var settingsWindow = new SettingsWindow();
            await settingsWindow.ShowDialog(this);

            // Jika user menekan tombol Save di window setting
            if (settingsWindow.SavedConfig != null)
            {
                // Reload config di MainWindow agar langsung berubah
                configController.LoadConfig();
                lblLineName.Text = configController.CurrentConfig.LineName;
            }
        }

        // --- BAGIAN UTAMA (LOGIKA PROGRAM) ---

        private void SetupAwal()
        {
            // Reset Input Form
            txtInput1.Text = "";
            txtInput2.Text = "";
            txtInput1.IsEnabled = true;
            txtInput2.IsEnabled = false;
            txtInput1.Focus();

            // Reset Panel Status
            pnlStatus.Background = ColorReady;
            lblStatusBig.Text = "READY";
            lblStatusDetail.Text = "SCAN LABEL FOR MATCHING";

            // Reset Detail Data
            lblInfoLabel1.Text = "-";
            lblInfoLabel2.Text = "-";

            // Reset Logic Controller
            controller.Reset();
        }

        private void OnWindowKeyDown(object? sender, KeyEventArgs e)
        {
            // Reset Manual menggunakan tombol F5
            if (e.Key == Key.F5)
            {
                SetupAwal();
            }
        }

        private void OnInput1KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string raw = txtInput1.Text ?? "";

                // Proses Scan Label 1 (QR Code)
                if (controller.ProcessLabel1(raw, out string msg))
                {
                    // Sukses -> Ubah Status jadi PROSES (Biru)
                    pnlStatus.Background = ColorProcess;
                    lblStatusBig.Text = "SCAN 2";
                    lblStatusDetail.Text = "OK. SCAN COMPONENT (BARCODE)";

                    // Tampilkan Data Label 1 di Panel Info
                    lblInfoLabel1.Text = controller.DataLabel1.MATNO;

                    // Pindah Fokus ke Input 2
                    txtInput1.IsEnabled = false;
                    txtInput2.IsEnabled = true;
                    txtInput2.Focus();
                }
                else
                {
                    // Gagal Scan
                    ShowError(msg);
                    txtInput1.SelectAll();
                }
            }
        }

        private void OnInput2KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string raw = txtInput2.Text ?? "";

                // Proses Scan Label 2 (Barcode)
                if (controller.ProcessLabel2(raw, out string msg))
                {
                    // Jika format benar, lakukan Matching
                    PerformMatching();
                }
                else
                {
                    ShowError(msg);
                    txtInput2.SelectAll();
                }
            }
        }

        private void PerformMatching()
        {
            string matNo = controller.DataLabel1?.MATNO ?? "-";
            string pcbNo = controller.DataLabel2?.PcbPartNo ?? "-";

            // Update Panel Info Data (Bawah Kiri)
            lblInfoLabel1.Text = matNo;
            lblInfoLabel2.Text = pcbNo;

            // Cek Matching
            if (controller.IsMatch(out string result))
            {
                // HASIL OK (Hijau)
                pnlStatus.Background = ColorOK;
                lblStatusBig.Text = "OK";
                lblStatusDetail.Text = "MATCHING SUCCESS";

                AddToHistory("OK", matNo, pcbNo);
            }
            else
            {
                // HASIL NG (Merah)
                pnlStatus.Background = ColorNG;
                lblStatusBig.Text = "NG";
                lblStatusDetail.Text = "PART MISMATCH!";

                AddToHistory("NG", matNo, pcbNo);
            }

            // AUTO RESET FLOW
            // Kita kembalikan ke Input 1 agar operator bisa langsung scan unit berikutnya
            txtInput1.IsEnabled = true;
            txtInput2.IsEnabled = false;
            txtInput1.Focus();
            txtInput1.SelectAll(); // Block text agar scan berikutnya langsung menimpa
        }

        private void ShowError(string msg)
        {
            pnlStatus.Background = ColorWarning;
            lblStatusBig.Text = "ERR";
            lblStatusDetail.Text = msg;
        }

        private void AddToHistory(string status, string matNo, string pcbNo)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            string log = $"[{time}] {status} | SVC:{matNo} | COMP:{pcbNo}";

            // Masukkan log baru ke urutan paling atas list
            lstHistory.Items.Insert(0, log);
        }
    }
}