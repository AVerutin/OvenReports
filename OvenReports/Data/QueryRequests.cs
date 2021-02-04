using System;

namespace OvenReports.Data
{
    public class QueryRequests
    {
        private string _meltsInOwenRequets;

        public QueryRequests()
        {
            _meltsInOwenRequets =
                "WITH t0 as (select l.unit_id,r1.unit_id_parent as id_posad,min(tm_beg) tm_beg " +
                "from mts.locations l join mts.units_relations r on r.unit_id_child = l.unit_id " +
                "join mts.units_relations r1 on r1.unit_id_child = r.unit_id_parent " +
                "where exists (select * from mts.locations l1 where l1.tm_beg between '{0}'::timestamp and '{}1'::timestamp " +
                "and l1.unit_id = l.unit_id and l1.node_id = 30100) " +
                "and l.node_id = 30100 " +
                "group by l.unit_id, r1.unit_id_parent " +
                "having min(tm_beg) between '{0}'::timestamp and '{1}'::timestamp) " +
                "SELECT pm.value_s as melt, sm.value_s as steel_mark, sc.value_s as section, pl.value_s as length, " +
                "pf.value_s as profile, pd.value_s as diameter, ps.value_s as standart, pc.value_s as customer, " +
                "pk.value_s prod_code, count(distinct t0.unit_id) cn_units, min(t0.tm_beg) min_beg, " +
                "case when max(case when l1.tm_end is null then 1 else 0 end) = 1 then null else max (l1.tm_end) end max_end " +
                "FROM t0 join mts.locations l1 on l1.unit_id = t0.unit_id and l1.node_id between 30100 and 30400 " +
                "join mts.passport pm on pm.unit_id = t0.id_posad and pm.param_id = 10000001 " +
                "join mts.passport pf on pf.unit_id = t0.id_posad and pf.param_id = 10000018 " +
                "join mts.passport pd on pd.unit_id = t0.id_posad and pd.param_id = 10000010 " +
                "join mts.passport sm on sm.unit_id = t0.id_posad and sm.param_id = 10000002 " +
                "join mts.passport sc on sc.unit_id = t0.id_posad and sc.param_id = 10000003 " +
                "join mts.passport pl on pl.unit_id = t0.id_posad and pl.param_id = 10000007 " +
                "join mts.passport ps on ps.unit_id = t0.id_posad and ps.param_id = 10000009 " +
                "join mts.passport pc on pc.unit_id = t0.id_posad and pc.param_id = 10000011 " +
                "join mts.passport pk on pk.unit_id = t0.id_posad and pk.param_id = 10000015 " +
                "GROUP BY pm.value_s, pf.value_s, pd.value_s, sm.value_s, sc.value_s, pl.value_s, ps.value_s, pc.value_s, pk.value_s " +
                "ORDER BY min(t0.tm_beg);";

        }

        /// <summary>
        /// Получить текст запроса на получение выборки списка плавок, посаженных в печь за период
        /// </summary>
        /// <param name="start">Начало периода</param>
        /// <param name="end">Конец периода</param>
        /// <returns>Текст запроса</returns>
        public string GetMeltsInOwenRequest(DateTime start, DateTime end)
        {
            return String.Format(_meltsInOwenRequets, start.ToString("O"), end.ToString("O"));
        }
    }
}