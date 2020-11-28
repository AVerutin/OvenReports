using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OvenReports.Data
{
    public class LandingData
    {
        [Required]
        [StringLength(10, ErrorMessage = "Слишком длинный номер плавки (ограничение в 10 символов).")]
        public int LandingId { get; set; }
        public DateTime LandingDate { get; set; }
        public DateTime FirstWeighting { get; set; }
        public DateTime LastWeighting { get; set; }
        public string MeltNumber { get; set; }
        public string SteelMark { get; set; }
        public string IngotProfile { get; set; }
        public int IngotsCount { get; set; }
        public int WeightAll { get; set; }
        public int WeightOne { get; set; }
        public int WeightReal { get; set; }
        public int ProductCode { get; set; }
        public string ProductProfile { get; set; }
        public int IngotLength { get; set; }
        public string Standart { get; set; }
        public double Diameter { get; set; }
        public string Customer { get; set; }
        public string Shift { get; set; }
        public string IngotClass { get; set; }
        public int Weighted { get; set; }
        public string Specification { get; set; }
        public int Lot { get; set; }

        public List<CoilData> CoilList { get; set; }
        public LandingData()
        {
            MeltNumber = "";
            LandingDate = default;
            SteelMark = "";
            IngotProfile = "";
            Standart = default;
            IngotsCount = 0;
            WeightAll = 0;
            IngotLength = 0;
            WeightOne = 0;
            WeightReal = 0;
            ProductCode = 0;
            ProductProfile = default;
            Diameter = default;
            Customer = default;
            Shift = default;
            IngotClass = default;
            Weighted = default;
            Specification = default;
            Lot = default;
            
            CoilList = new List<CoilData>();
        }
    }
}
