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
        public int WeightedIngots { get; set; }     // Взвешено заготовок (перед печью)
        public bool CanBeDeleted { get; set; }      // Признак возможности удаления посада
        public string Specification { get; set; }   // Спецификация
        public int Lot { get; set; }                // Лот
        public int IngotsInOwen { get; set; }       // Количество заготовок на поде печи
        public int IngotsInMill { get; set; }       // Количество заготовок, выданных из печи в стан
        public int IngotsReturned { get; set; }     // Количество возвращенных заготовок
        public int IngotsRejected { get; set; }       // Количество забракованных заготовок
        public int IngotsMilled { get; set; }       // Прокатано заготовок

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
            WeightedIngots = default;
            CanBeDeleted = false;
            Specification = default;
            Lot = default;
            IngotsInOwen = default;
            IngotsInMill = default;
            IngotsReturned = default;
            IngotsRejected = default;
            IngotsMilled = default;
            
            CoilList = new List<CoilData>();
        }
    }
}
