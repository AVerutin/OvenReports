using System;

namespace OvenReports.Data
{
    public class CheckDtData
    {
        /* [00] */public int LandingId { get; set; }              // id_posad
        /* [02] */ public string IngotMes { get; set; }            // eu_mes
        /* [01] */ public string CoilId { get; set; }              // id_coil
        /* [03] */ public int CoilWeightMes { get; set; }          // coil_weight
        /* [04] */ public DateTime DateClose { get; set; }         // date_close
        /* [05] */ public string IngotDt { get; set; }             // unit_dt
        /* [08] */ public int CoilWeightDt { get; set; }           // coil_weight_dt
        /* [09] */ public DateTime CoilDateParam { get; set; }     // coil_date_param
        /* [10] */ public DateTime TimeBegin { get; set; }         // tm_beg
        /* [11] */ public DateTime TimeEnd { get; set; }           // tm_end
        /* [12] */ public int BilletWeight { get; set; }           // billet_weight
        /* [13] */ public DateTime BilletDate { get; set; }        // billet_date
        /* [6] */ public int UnitDt { get; set; }                 // unit_dt
        /* [07] */ public string IngotId { get; set; }                // ingot_id
        /* [14] */ public string IngotCompare { get; set; }        // eu_dt_compare
        /* [15] */ public bool Cutting { get; set; }               // sig_cut

        public CheckDtData()
        {
            LandingId = default;
            IngotMes = default;
            CoilId = default;
            CoilWeightMes = default;
            CoilWeightDt = default;
            DateClose = DateTime.MinValue;
            IngotDt = default;
            IngotMes = default;
            CoilDateParam = DateTime.MinValue;
            TimeBegin = DateTime.MinValue;
            TimeEnd = DateTime.MinValue;
            BilletWeight = default;
            BilletDate = DateTime.MinValue;
            UnitDt = default;
            IngotId = default;
            IngotCompare = default;
            Cutting = false;
        }
    }
}