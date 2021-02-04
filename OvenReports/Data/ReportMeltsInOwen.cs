using System;

namespace OvenReports.Data
{
    public class ReportMeltsInOwen
    {
        public string Melt { get; set; }            // Номер плавки
        public string SteelMark { get; set; }       // Марка стали
        public string Section { get; set; }         // Сечение заготовки
        public int IngotLength { get; set; }        // Длина заготовки
        public string IngotProfile { get; set; }    // Профиль проката
        public double Diameter { get; set; }        // Диаметр
        public string Standart { get; set; }        // Стандарт
        public string Customer { get; set; }        // Заказчик
        public int ProductCode { get; set; }        // Код продукции
        public int IngotsCount { get; set; }        // Количесто заготовок
        public DateTime TimeStart { get; set; }     // Время посада в печь
        public DateTime TimeEnd { get; set; }       // Время выхода из печи

        public ReportMeltsInOwen()
        {
            Melt = default;
            SteelMark = default;
            Section = default;
            IngotLength = default;
            IngotProfile = default;
            Diameter = default;
            Standart = default;
            Customer = default;
            ProductCode = default;
            IngotsCount = default;
            TimeStart = DateTime.MinValue;
            TimeEnd = DateTime.MinValue;
        }
    }
}