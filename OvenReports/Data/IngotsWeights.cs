using System;

namespace OvenReports.Data
{
    public class IngotsWeights
    {
        public int NodeId { get; set; }         // Идентификатор техузла
        public string NodeName { get; set; }    // Наименование техузла
        public int UnitId { get; set; }         // Идентификатор единицы учета
        public DateTime TimeBegin { get; set; } // Время вхождения ЕУ в ТУ
        public DateTime TimeEnd { get; set; }   // Время выхода ЕУ из ТУ
        public int BilletWeight { get; set; }   // Фактический вес заготовки
        public int Position { get; set; }       // Номер позиции заготовки в очереди
        public int CoilNumber { get; set; }     // Номер бунта
        public int CoilWeight { get; set; }     // Фактический вес бунта
        public string Melt { get; set; }        // Номер плавки
        public string IngotProfile { get; set; }// Сечение заготовки 
        public string SteelGrade { get; set; }  // Марка стали
        public string Profile { get; set; }     // Тип прокатываемого профиля
        public double Diameter { get; set; }    // Диаметр прокатываемого профиля
        public int CoilsCount { get; set; }     // Количество ЕУ в плавке
        public int WeightTotal { get; set; }    // Теоретический общий вес всех ЕУ в плавке
        public int IngotLength { get; set; }    // Длина заготовки 
        public string Standart { get; set; }    // Стандарт
        public int ProductCode { get; set; }    // Код продукции
        public string Customer { get; set; }    // Заказчик

        public IngotsWeights()
        {
            NodeId = default;
            NodeName = default;
            UnitId = default;
            TimeBegin = DateTime.MinValue;
            TimeEnd = DateTime.MinValue;
            BilletWeight = default;
            Position = default;
            CoilNumber = default;
            CoilWeight = default;
            SteelGrade = default;
            Diameter = default;
            CoilsCount = default;
            WeightTotal = default;
            IngotProfile = default;
            Profile = default;
            IngotLength = default;
            Standart = default;
            ProductCode = default;
            Customer = default;
        }
    }
}
