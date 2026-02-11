using LabelMatchingSvcPartAvalonia.Models;
using Newtonsoft.Json;
using System;
using System.IO;
// using Oracle.ManagedDataAccess.Client; // Uncomment jika sudah install paket Oracle

namespace LabelMatchingSvcPartAvalonia.Controllers
{
    public class ConfigController
    {
        private const string ConfigFile = "appsettings.json";
        private const string AdminUser = "admin";
        private const string AdminPass = "daikin123";

        public AppConfig CurrentConfig { get; private set; }

        public ConfigController()
        {
            LoadConfig();
        }

        public void LoadConfig()
        {
            if (File.Exists(ConfigFile))
            {
                string json = File.ReadAllText(ConfigFile);
                CurrentConfig = JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
            }
            else
            {
                CurrentConfig = new AppConfig();
                SaveConfig(CurrentConfig);
            }
        }

        public void SaveConfig(AppConfig newConfig)
        {
            CurrentConfig = newConfig;
            string json = JsonConvert.SerializeObject(newConfig, Formatting.Indented);
            File.WriteAllText(ConfigFile, json);
        }

        public bool ValidateLogin(string user, string pass)
        {
            return user == AdminUser && pass == AdminPass;
        }

        // --- FUNGSI BARU: AMBIL NAMA DARI DATABASE ---
        public string GetProgramTitleFromDb()
        {
            string finalTitle = CurrentConfig.ProgramTitle; // Default: "SVC-LBMC"

            try
            {
                // CONTOH KONEKSI KE ORACLE (Pseudo-code)
                // Pastikan Connection String tidak kosong
                if (string.IsNullOrEmpty(CurrentConfig.DbConnectionString))
                    return finalTitle;

                /* * SILAHKAN UNCOMMENT DAN SESUAIKAN JIKA SUDAH INSTALL ORACLE CLIENT
                 * using (var conn = new OracleConnection(CurrentConfig.DbConnectionString))
                {
                    conn.Open();
                    string query = "SELECT PROGRAM_NAME FROM TBL_MASTER_CONFIG WHERE ID = 1"; // Sesuaikan Query
                    using (var cmd = new OracleCommand(query, conn))
                    {
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            finalTitle = result.ToString(); // Nama dari DB: "SVC-LBMC - DIID MSI"
                        }
                    }
                }
                */

                // SIMULASI KONEKSI (Hapus blok ini jika pakai DB asli)
                // Anggap saja kita berhasil konek dan dapat nama panjang
                // finalTitle = "SVC-LBMC - DIID MSI (CONNECTED)"; 
            }
            catch (Exception ex)
            {
                // Jika error (DB mati/putus), tetap return default title agar app tidak crash
                Console.WriteLine("Gagal connect DB: " + ex.Message);
            }

            return finalTitle;
        }
    }
}