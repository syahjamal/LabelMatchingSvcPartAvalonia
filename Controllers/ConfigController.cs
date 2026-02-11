using LabelMatchingSvcPartAvalonia.Models;
using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Security.Cryptography; // Diperlukan untuk MD5
using System.Text;                // Diperlukan untuk Encoding
using Oracle.ManagedDataAccess.Client; // Pastikan NuGet Oracle.ManagedDataAccess.Core terpasang

namespace LabelMatchingSvcPartAvalonia.Controllers
{
    public class ConfigController
    {
        private const string ConfigFile = "appsettings.json";
        public AppConfig CurrentConfig { get; private set; }

        public ConfigController()
        {
            LoadConfig();
        }

        // Memuat konfigurasi dari file JSON lokal
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

        // Menyimpan konfigurasi ke file JSON lokal
        public void SaveConfig(AppConfig newConfig)
        {
            CurrentConfig = newConfig;
            string json = JsonConvert.SerializeObject(newConfig, Formatting.Indented);
            File.WriteAllText(ConfigFile, json);
        }

        // --- FUNGSI HELPER: MENGHASILKAN HASH MD5 ---
        // Digunakan untuk mencocokkan input password dengan hash di database
        private string GetMd5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    // Format X2 menghasilkan string hexadecimal huruf besar
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        // Logika verifikasi Login ke Database Oracle
        public bool TryLogin(string username, string password, out string message)
        {
            message = "";
            try
            {
                // Konversi password input menjadi MD5 Hash
                string hashedPassword = GetMd5Hash(password);

                using (var conn = new OracleConnection(CurrentConfig.DbConnectionString))
                {
                    conn.Open();

                    // Query untuk validasi User, Authority, dan Program
                    string query = @"
                        SELECT U.USER_ID, U.FULL_NAME, A.AUTH_LEVEL, P.LINE_NAME
                        FROM TBL_MASTER_USER U
                        JOIN TBL_USER_AUTHORITY A ON U.USER_ID = A.USER_ID
                        JOIN TBL_MASTER_PROGRAM P ON A.PROG_CODE = P.PROG_CODE
                        WHERE U.USERNAME = :uname 
                          AND U.PASSWORD = :pass
                          AND U.IS_ACTIVE = 'Y'
                          AND A.PROG_CODE = 'SVC-LBMC'";

                    using (var cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("uname", username));
                        cmd.Parameters.Add(new OracleParameter("pass", hashedPassword));

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Simpan data ke Session
                                UserSession.UserId = reader["USER_ID"].ToString();
                                UserSession.FullName = reader["FULL_NAME"].ToString();
                                UserSession.AuthLevel = reader["AUTH_LEVEL"].ToString();
                                UserSession.LineName = reader["LINE_NAME"].ToString();
                                UserSession.Username = username;

                                message = $"Welcome, {UserSession.FullName}";
                                return true;
                            }
                            else
                            {
                                message = "Username/Password salah atau akses ditolak.";
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Fallback untuk akun admin default jika database tidak terjangkau
                if (username == "admin" && password == "daikin123")
                {
                    UserSession.UserId = "001";
                    UserSession.FullName = "System Administrator (Offline)";
                    UserSession.AuthLevel = "ADMIN";
                    UserSession.LineName = CurrentConfig.LineName;
                    UserSession.Username = "admin";
                    return true;
                }

                message = "Database Error: " + ex.Message;
                return false;
            }
        }

        // Mengambil Nama Program dari Database
        public string GetProgramTitleFromDb()
        {
            string finalTitle = CurrentConfig.ProgramTitle; // Default: "SVC-LBMC"

            try
            {
                if (string.IsNullOrEmpty(CurrentConfig.DbConnectionString))
                    return finalTitle;

                using (var conn = new OracleConnection(CurrentConfig.DbConnectionString))
                {
                    conn.Open();
                    // Query mengambil PROG_NAME dari tabel TBL_MASTER_PROGRAM
                    string query = "SELECT PROG_NAME FROM TBL_MASTER_PROGRAM WHERE PROG_CODE = 'SVC-LBMC'";
                    using (var cmd = new OracleCommand(query, conn))
                    {
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            finalTitle = result.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Gagal mengambil title dari DB: " + ex.Message);
            }

            return finalTitle;
        }

        // Verifikasi login sederhana (untuk menu setting lokal)
        public bool ValidateLogin(string user, string pass)
        {
            // Login admin tetap menggunakan kredensial statis untuk akses setting
            return user == "admin" && pass == "daikin123";
        }
    }
}