namespace LabelMatchingSvcPartAvalonia.Models
{
    // Model untuk Label 1 (JSON)
    public class ServiceLabel
    {
        public string TAGTYPE { get; set; }
        public string MATNO { get; set; }    // Kunci pencocokan
        public int QTY { get; set; }
        public string SVCPART { get; set; }
        public string WCNO { get; set; }
        public string TAGNO { get; set; }
        public string PTIME { get; set; }
    }

    // Model untuk Label 2 (String Barcode)
    public class ComponentLabel
    {
        public string PcbPartNo { get; set; } // Kunci pencocokan
        public string ModelCode { get; set; }
        public string Sequence { get; set; }
    }
}