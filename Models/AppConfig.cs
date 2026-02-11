namespace LabelMatchingSvcPartAvalonia.Models
{
    public class AppConfig
    {
        public string LineName { get; set; } = "F1"; // Default
        public string ProgramTitle { get; set; } = "SVC-LBMC";
        public string DbConnectionString { get; set; } = "Data Source=XE;User Id=system;Password=oracle;";
    }
}
