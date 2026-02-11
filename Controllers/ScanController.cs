using LabelMatchingSvcPartAvalonia.Models;
using Newtonsoft.Json;
using System;

namespace LabelMatchingSvcPartAvalonia.Controllers
{
    public class ScanController
    {
        public ServiceLabel DataLabel1 { get; private set; }
        public ComponentLabel DataLabel2 { get; private set; }

        // Fungsi Reset
        public void Reset()
        {
            DataLabel1 = null;
            DataLabel2 = null;
        }

        // Proses Label 1 (JSON)
        public bool ProcessLabel1(string rawData, out string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rawData)) { message = "Input kosong."; return false; }

                DataLabel1 = JsonConvert.DeserializeObject<ServiceLabel>(rawData);

                if (DataLabel1 == null || string.IsNullOrEmpty(DataLabel1.MATNO))
                {
                    message = "Format JSON salah / MATNO hilang.";
                    return false;
                }

                message = $"Label 1 OK: {DataLabel1.MATNO}";
                return true;
            }
            catch
            {
                message = "Error: Bukan format JSON valid.";
                return false;
            }
        }

        // Proses Label 2 (String dipisah bintang *)
        public bool ProcessLabel2(string rawData, out string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rawData)) { message = "Input kosong."; return false; }

                // Contoh: 1001*0000001*3P734325-1*...
                string[] parts = rawData.Split('*');

                if (parts.Length < 3)
                {
                    message = "Format Barcode kurang lengkap (*).";
                    return false;
                }

                DataLabel2 = new ComponentLabel
                {
                    ModelCode = parts[0],
                    Sequence = parts[1],
                    PcbPartNo = parts[2] // Ambil index ke-2
                };

                message = $"Label 2 OK: {DataLabel2.PcbPartNo}";
                return true;
            }
            catch
            {
                message = "Error parsing Label 2.";
                return false;
            }
        }

        // Cek Apakah Cocok (Matching)
        public bool IsMatch(out string resultMessage)
        {
            if (DataLabel1 == null || DataLabel2 == null)
            {
                resultMessage = "Data belum lengkap.";
                return false;
            }

            // BANDINGKAN STRING (Trim untuk buang spasi tidak sengaja)
            bool match = (DataLabel1.MATNO.Trim() == DataLabel2.PcbPartNo.Trim());

            if (match)
            {
                resultMessage = "PART SESUAI";
                return true;
            }
            else
            {
                // Tampilkan info detail jika beda
                resultMessage = $"MISMATCH!\nSvc: {DataLabel1.MATNO}\nPCB: {DataLabel2.PcbPartNo}";
                return false;
            }
        }
    }
}