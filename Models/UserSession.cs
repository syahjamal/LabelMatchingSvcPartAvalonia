namespace LabelMatchingSvcPartAvalonia.Models
{
    public static class UserSession
    {
        // Data User
        public static string UserId { get; set; } = "";
        public static string Username { get; set; } = "";
        public static string FullName { get; set; } = "";
        public static string AuthLevel { get; set; } = ""; // ADMIN / OPERATOR

        // Data Program & Line (Didapat saat login dari tabel TBL_MASTER_PROGRAM)
        public static string ProgCode { get; set; } = "SVC-LBMC";
        public static string LineName { get; set; } = ""; // F1, F2

        // Helper untuk cek apakah sudah login
        public static bool IsLoggedIn => !string.IsNullOrEmpty(UserId);

        public static void Clear()
        {
            UserId = ""; Username = ""; FullName = ""; AuthLevel = ""; LineName = "";
        }
    }
}