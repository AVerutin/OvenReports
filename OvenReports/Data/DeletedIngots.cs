using System;

namespace OvenReports.Data
{
    public class DeletedIngots
    {
        public int NodeId { get; set; }             // Идент ТУ
        public string NodeCode { get; set; }        // Наименование ТУ
        public string MeltNumber { get; set; }      // ЕУ
        public int UnitId { get; set; }             // Идент ЕУ
        public int IngotId { get; set; }            // Идент ССМ
        public DateTime TimeBegin { get; set; }     // Время входа в ТУ
        public DateTime TimeEnd { get; set; }       // Время выхода из ТУ

        public DeletedIngots()
        {
            NodeId = default;
            NodeCode = default;
            MeltNumber = default;
            UnitId = default;
            IngotId = default;
            TimeBegin = DateTime.MinValue;
            TimeEnd = DateTime.MinValue;
        }
    }
}