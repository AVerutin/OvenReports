using System;
using Microsoft.VisualBasic;

namespace OvenReports.Data
{
    public enum OrderTypes
    {
        OrderByMeltNumber = 0,
        OrderByPeriodStart,
        OrderByPeriodFinish
    }
    
    public class QueryRequests
    {
        private string _meltsInOwenRequets;     // Количество заготовок в печи за период
        private string _returnsByMelt;          // Список возвратов по номеру плавки
        private string _countReturnByMeltId;    // Количество возвратов (суммарное) по иденту посада (с прода)
        private string _returnsByPeriod;        // Список возвратов за период
        private string _chechDt;                // Проверка ДТ
        private string _deleted;                // Список удаленных ЕУ за период
        private string _rejections;             // Список плавок с количеством брака за период

        public QueryRequests()
        {
            // Количество заготовок в печи за период
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

            // Список возвратов по номеру плавки
            _returnsByMelt =
                "with t0 as (select rp.unit_id_parent as id_posad, pr.value_s as prod, " +
                "l.tm_beg::timestamp as tm_beg, l.tm_end::timestamp as tm_end, " +
                "l.unit_id as unit_dt, t.unit_id as unit_task, pm.value_s as melt, " +
                "t.pos, pc.value_s as count, t.date_reg::timestamp as date_reg, " +
                "p.value_n as billet_w, pw.value_n as coil_w, " +
                "l.node_id, n.node_code, l.id " +
                "from mts.locations l join mts.nodes n on n.id = l.node_id " +
                "join mts.units u on u.id= l.unit_id " +
                "left join mts.units_relations r on r.unit_id_child = u.id " +
                "left join mts.unit_tasks t on t.unit_id = r.unit_id_parent and t.node_id = 20100 " +
                "left join mts.units_relations rp on rp.unit_id_child = r.unit_id_parent " +
                "left join mts.passport pm on pm.unit_id = rp.unit_id_parent and pm.param_id=10000001 " +
                "left join mts.passport pc on pc.unit_id = rp.unit_id_parent and pc.param_id=10000004 " +
                "left join mts.passport pr on pr.unit_id = rp.unit_id_parent and pr.param_id=1 " +
                "left join mts.passport p on p.unit_id = l.unit_id and p.param_id=1000 " +
                "left join mts.passport pw on pw.unit_id = t.unit_id and pw.param_id=10000014 " +
                "where l.node_id = 50100 and pm.value_s='{0}' order by node_id, l.tm_beg) " +
                "select melt, tm_beg, tm_end, pos, count, date_reg, billet_w, node_code, node_id " +
                "from t0 group by melt, tm_beg, tm_end, pos, count, date_reg, billet_w, node_code, node_id order by tm_beg;";
            
            // Количество возвратов (суммарное) по иденту посада (с прода)
            _countReturnByMeltId =
                "select rp.unit_id_parent id_posad_test, pr.value_s as id_posad_prod, pm.value_s melt, " +
                "pc.value_n count_posad, count(distinct l.unit_id) count_dt " +
                "from mts.locations l join mts.units_relations r on r.unit_id_child = l.unit_id " +
                "join mts.unit_tasks t on t.unit_id = r.unit_id_parent and t.node_id=20100 " +
                "join mts.units_relations rp on rp.unit_id_child = r.unit_id_parent " +
                "join mts.passport pm on pm.unit_id = rp.unit_id_parent and pm.param_id=10000001 " +
                "join mts.passport pc on pc.unit_id = rp.unit_id_parent and pc.param_id=10000004 " +
                "join mts.passport pr on pr.unit_id = rp.unit_id_parent and pr.param_id=1 " +
                "join mts.passport p on p.unit_id = l.unit_id and p.param_id=1000 " +
                "where l.node_id = 50100 and pr.value_s = '{0}' " +
                "group by rp.unit_id_parent,pr.value_s,pm.value_s,pc.value_n;";
            
            // Список возвратов за период
            _returnsByPeriod = 
                "with t0 as (select rp.unit_id_parent as id_posad, pr.value_s as prod, " +
                "l.tm_beg::timestamp as tm_beg, l.tm_end::timestamp as tm_end, " +
                "l.unit_id as unit_dt, t.unit_id as unit_task, pm.value_s as melt, t.pos, " +
                "pc.value_s as count, t.date_reg::timestamp as date_reg, " +
                "p.value_n as billet_w, pw.value_n as coil_w, l.node_id, n.node_code, l.id " +
                "from mts.locations l join mts.nodes n on n.id = l.node_id " +
                "join mts.units u on u.id= l.unit_id " +
                "left join mts.units_relations r on r.unit_id_child = u.id " +
                "left join mts.unit_tasks t on t.unit_id = r.unit_id_parent and t.node_id = 20100 " +
                "left join mts.units_relations rp on rp.unit_id_child = r.unit_id_parent " +
                "left join mts.passport pm on pm.unit_id = rp.unit_id_parent and pm.param_id=10000001 " +
                "left join mts.passport pc on pc.unit_id = rp.unit_id_parent and pc.param_id=10000004 " +
                "left join mts.passport pr on pr.unit_id = rp.unit_id_parent and pr.param_id=1 " +
                "left join mts.passport p on p.unit_id = l.unit_id and p.param_id=1000 " +
                "left join mts.passport pw on pw.unit_id = t.unit_id and pw.param_id=10000014 " +
                "where l.node_id = 50100 and l.tm_beg between '{0}'::timestamp and '{1}'::timestamp " +
                "order by node_id, l.tm_beg) " +
                "select melt, tm_beg, tm_end, pos, count, date_reg, billet_w, node_code, node_id from t0 " +
                "group by melt, tm_beg, tm_end, pos, count, date_reg, billet_w, node_code, node_id order by tm_beg;";
            
            // Проверка ДТ
            _chechDt =
                "with mes as (SELECT queue_10000.id_posad,queue_10000.id_coil,queue_10000.pos, " +
                "queue_10000.date_close,queue_10000.coil_weight,queue_10000.param_date,queue_10000.coil_num,queue_10000.melt,queue_10000.count, " +
                "count(*) over () cn_mes,queue_10000.now_prod " +
                "FROM dblink('hostaddr=10.23.196.52 dbname=mtsbase user=postgres password=dfaf@we_jkjcld!'::text, " +
                "'select now() as now_prod,r.unit_id_parent c_id_posad, " +
                "t.unit_id c_id_coil, t.pos as c_pos, t.date_close::timestamp as date_close, pw.value_s::numeric as c_coil_weight,pw.param_date, " +
                "pn.value_s::numeric as c_coil_num, " +
                "max(case when p.param_id = 10000001 then p.value_s end)::text as c_melt, " +
                "max(case when p.param_id = 10000004 then p.value_n end) as c_count " +
                "from mts.unit_tasks t join mts.units_relations r on t.unit_id = r.unit_id_child " +
                "join mts.passport p on p.unit_id = r.unit_id_parent and p.project_id = 1 " +
                "left join mts.passport pw on pw.unit_id = t.unit_id and pw.project_id = 1 and pw.param_id = 10000014 " +
                "left join mts.passport pn on pn.unit_id = t.unit_id and pn.project_id = 1 and pn.param_id = 10000016 " +
                "where t.node_id = 10000 and t.project_id = 1 and t.date_close between ''{0}''::timestamp and ''{1}''::timestamp " +
                "group by r.unit_id_parent,t.unit_id,t.pos,t.date_reg,t.date_close,pw.value_s,pn.value_s,pw.param_date'::text) " +
                "queue_10000(now_prod timestamp with time zone,id_posad numeric,id_coil numeric,pos numeric,date_close timestamp with time zone, " +
                "coil_weight numeric,param_date timestamp without time zone,coil_num numeric,melt text,count numeric)), " +
                "dt as (select r.unit_id_parent id_posad, l.unit_id as unit_dt,ingot_id, t.pos as pos, tm_beg, tm_end, " +
                "pw.value_s::numeric as billet_weight, pw.param_date as billet_date, pt.value_s::numeric coil_weight_dt, " +
                "pt.param_date coil_date_param, coalesce(pp.value_s::numeric,0) as id_prod, p.value_s  melt, pc.value_n count, " +
                "mes.id_coil, pr.value_n as id_parent_cut from mts.locations l join mts.units u on u.id = l.unit_id " +
                "left join mts.units_relations rd on rd.unit_id_child = l.unit_id " +
                "left join mts.unit_tasks t on t.unit_id = rd.unit_id_parent and t.node_id = 20100 and t.project_id = 1 " +
                "left join mts.units_relations r on t.unit_id = r.unit_id_child " +
                "left join mts.passport pp on pp.unit_id = r.unit_id_parent and pp.project_id = 1 and pp.param_id = 1 " +
                "left join mts.passport p on p.unit_id = r.unit_id_parent and p.project_id = 1 and p.param_id = 10000001 " +
                "left join mts.passport pc on pc.unit_id = r.unit_id_parent and pc.project_id = 1 and pc.param_id = 10000004 " +
                "left join mts.passport pw on pw.unit_id = l.unit_id and pw.project_id = 1 and pw.param_id = 1000 " +
                "join mts.passport pt on pt.unit_id = l.unit_id and pt.project_id = 1 and pt.param_id = 1002 " +
                "left join mts.passport pr on pr.unit_id = l.unit_id and pr.param_id=10000 " +
                "join mes on mes.date_close between pt.param_date + interval '10 second' and pt.param_date + interval '30 second' " +
                "where l.node_id = 15100), pr as (select dt.unit_dt, pp.value_s||'-'||t.pos||'-1' eu_child, pp.value_s||'-'||t.pos eu_parent, " +
                "pw.value_s::numeric billet_weight_parent from dt left join mts.passport pw on pw.unit_id = dt.id_parent_cut " +
                "join mts.units_relations r on r.unit_id_child = dt.id_parent_cut " +
                "join mts.unit_tasks t on t.unit_id = r.unit_id_parent and t.node_id = 20100 " +
                "join mts.units_relations rp on rp.unit_id_child = r.unit_id_parent " +
                "join mts.passport pp on pp.unit_id =  rp.unit_id_parent and pp.param_id = 10000001 " +
                "where dt.id_parent_cut is not null) SELECT " +
                "mes.id_posad, mes.id_coil, mes.melt||'-'||mes.pos as eu_mes, mes.coil_weight, date_close, " +
                "dt.unit_dt,dt.ingot_id, coalesce(dt.melt||'-'||dt.pos,pr.eu_child) as eu_dt, dt.coil_weight_dt, dt.coil_date_param, " +
                "tm_beg, tm_end, coalesce(dt.billet_weight,pr.billet_weight_parent) as billet_weight, dt.billet_date, " +
                "coalesce(dt.melt||'-'||dt.pos,pr.eu_parent) as eu_dt_compare, " +
                "case when pr.unit_dt is not null then 1 else 0 end sig_cut " +
                "FROM mes left join dt on dt.id_coil = mes.id_coil left join pr on dt.unit_dt = pr.unit_dt ORDER BY mes.date_close;";

            // Список удаленных ЕУ
            _deleted =
                "WITH t0 as (select distinct l.unit_id, " +
                "first_value(l.id) over (partition by l.unit_id order by l.tm_end desc, l.id desc) as id_end " +
                "from mts.locations l where exists (select * from mts.locations l1 " +
                "where l1.tm_beg between '{0}'::timestamp and '{1}'::timestamp " +
                "and l1.unit_id = l.unit_id) " +
                "and not exists (select * from mts.locations l0 where l0.tm_end is null and l0.unit_id = l.unit_id)) " +
                "SELECT distinct l.node_id, n.node_code, p.value_s||'-'||t.pos melt, " +
                "l.unit_id, ingot_id, l.tm_beg, l.tm_end " +
                "FROM mts.locations l " +
                "join mts.units u on u.id = l.unit_id " +
                "join t0 on t0.id_end = l.id " +
                "join mts.nodes n on n.id = l.node_id " +
                "left join mts.units_relations r on r.unit_id_child = u.id " +
                "left join mts.unit_tasks t on t.unit_id = r.unit_id_parent and t.node_id = 20100 " +
                "left join mts.units_relations rp on rp.unit_id_child = r.unit_id_parent " +
                "left join mts.passport p on p.unit_id = rp.unit_id_parent and p.param_id = 10000001 " +
                "WHERE l.tm_end between '{0}'::timestamp and '{1}'::timestamp " +
                "and l.node_id <> 15100 and tm_end < now() - interval '10 minute' " +
                "ORDER BY l.node_id, l.tm_beg;";

            _rejections =
                "WITH t0 as (select distinct l.unit_id, " +
                "first_value(l.id) over (partition by l.unit_id order by l.tm_end desc, l.id desc) as id_end " +
                "from mts.locations l where exists (select * from mts.locations l1 " +
                "where l1.tm_beg between '{0}'::timestamp and '{1}'::timestamp " +
                "and l1.unit_id = l.unit_id) and not exists (select * from mts.locations l0 where l0.tm_end is null and l0.unit_id = l.unit_id)) " +
                "SELECT distinct p.value_s melt, count(l.unit_id) cnt, min(l.tm_beg) tm_beg, max(l.tm_end) tm_end " +
                "FROM mts.locations l join mts.units u on u.id = l.unit_id " +
                "join t0 on t0.id_end = l.id join mts.nodes n on n.id = l.node_id " +
                "left join mts.units_relations r on r.unit_id_child = u.id " +
                "left join mts.unit_tasks t on t.unit_id = r.unit_id_parent and t.node_id = 20100 " +
                "left join mts.units_relations rp on rp.unit_id_child = r.unit_id_parent " +
                "left join mts.passport p on p.unit_id = rp.unit_id_parent and p.param_id = 10000001 " +
                "WHERE l.tm_end between '{0}'::timestamp and '{1}'::timestamp " +
                "and l.node_id between 50160 and 50520 " + 
                "and tm_end < now() - interval '10 minute' GROUP BY melt ORDER BY melt, tm_beg;";
        }

        /// <summary>
        /// Получить текст запроса на получение выборки списка плавок, посаженных в печь за период
        /// </summary>
        /// <param name="start">Начало периода</param>
        /// <param name="end">Конец периода</param>
        /// <returns>Текст запроса</returns>
        public string GetMeltsInOwenRequest(DateTime start, DateTime end)
        {
            return string.Format(_meltsInOwenRequets, start.ToString("O"), end.ToString("O"));
        }

        /// <summary>
        /// Получить текста на выборку возвратов по номеру плавки 
        /// </summary>
        /// <param name="meltNumber">Номер плавки</param>
        /// <returns>Запрос на выборку списка возвратов</returns>
        public string GetReturnsByMelt(string meltNumber)
        {
            return string.Format(_returnsByMelt, meltNumber);
        }

        /// <summary>
        /// Получить запрос на получение количества возвратов по идентификатору посада
        /// </summary>
        /// <param name="meltId">Идентификатор посада</param>
        /// <returns>Запрос на получение количества возвратов</returns>
        public string GetCountReturnsByMeltId(string meltId)
        {
            return string.Format(_countReturnByMeltId, meltId);
        }

        /// <summary>
        /// Получить запрос на получение списка возвратов за период
        /// </summary>
        /// <param name="begin">Начало периода</param>
        /// <param name="end">Конец периода</param>
        /// <returns>Список возвратов за период</returns>
        public string GetReturnsByPeriod(DateTime begin, DateTime end)
        {
            return string.Format(_returnsByPeriod, begin.ToString("O"), end.ToString("O"));
        }

        /// <summary>
        /// Получить запрос на проверку соотвествия данных на проде и на тесте
        /// </summary>
        /// <param name="begin">Начало периода</param>
        /// <param name="end">Конец периода</param>
        /// <returns>Запрос на проверку соотвествия данных на проде и на тесте</returns>
        public string GetCheckDt(DateTime begin, DateTime end)
        {
            return string.Format(_chechDt, begin.ToString("O"), end.ToString("O"));
        }

        /// <summary>
        /// Получить текст запроса на выборку списка удаленных ЕУ за период
        /// </summary>
        /// <param name="begin">Начало периода</param>
        /// <param name="end">Конец периода</param>
        /// <returns>Текст запроса на выборку списка удаленных ЕУ за период</returns>
        public string GetDeletedByPeriod(DateTime begin, DateTime end)
        {
            return string.Format(_deleted, begin.ToString("O"), end.ToString("O"));
        }

        /// <summary>
        /// Получить запрос на список бурежек с группировкой по номеру плавки за период
        /// </summary>
        /// <param name="begin">Начало периода</param>
        /// <param name="end">Конец периода</param>
        /// <returns>Запрос на список бурежек с группировкой по номеру плавки за период</returns>
        public string GetRejectionsByPeriod(DateTime begin, DateTime end)
        {
            return string.Format(_rejections, begin.ToString("O"), end.ToString("O"));
        }
    }
}