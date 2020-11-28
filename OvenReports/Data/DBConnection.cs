using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using NLog;
using Npgsql;

namespace OvenReports.Data
{
    public class DBConnection
    {
        private readonly string _connectionString;
        private readonly Logger _logger;

        /// <summary>
        /// Конструктор создания подключения к базе данных
        /// </summary>
        public DBConnection()
        {
            // Параметры подключения к БД для удаленного компьютера
            // "DBConnection": {
            //     "Host": "10.23.196.52",
            //     "Port": "5432",
            //     "Database": "mtsbase",
            //     "UserName": "mts",
            //     "Password": "dfaf@we jkjcld!",
            //     "sslmode": "Prefer",
            //     "Trust Server Certificate": "true",
            //     "Reconnect": "20000"
            // }

            // Параметры подключения к БД для локального компьютера
            // "DBConnection": {
            //     "Host": "192.168.56.104",
            //     "Port": "5432",
            //     "Database": "mtsbase",
            //     "UserName": "mts",
            //     "Password": "test$ope$_1",
            //     "sslmode": "Prefer",
            //     "Trust Server Certificate": "true",
            //     "Reconnect": "20000"
            // }


            // Читаем параметры подключения к СУБД PostgreSQL
            _logger = LogManager.GetCurrentClassLogger();
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            string host = config.GetSection("DBConnection:Host").Value;
            int port = int.Parse(config.GetSection("DBConnection:Port").Value);
            string database = config.GetSection("DBConnection:Database").Value;
            string user = config.GetSection("DBConnection:UserName").Value;
            string password = config.GetSection("DBConnection:Password").Value;

            _connectionString =
                $"Server={host};Username={user};Database={database};Port={port};Password={password}"; //";SSLMode=Prefer";
        }

        /// <summary>
        /// Получить список нарядов заготовок на посаде печи
        /// </summary>
        /// <returns>Список нарядов на посад в печь</returns>
        public List<LandingData> GetLandingOrder()
        {
            List<LandingData> result = new List<LandingData>();
            DataTable dataTable = new DataTable();

            string query = "select * from public.f_get_queue();";

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    new NpgsqlDataAdapter(new NpgsqlCommand(query, connection)).Fill(dataTable);
                    connection.Close();
                    if (dataTable.Rows.Count > 0)
                    {
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            LandingData item = new LandingData();
                            try
                            {
                                item.LandingId = int.Parse(dataTable.Rows[i][0].ToString() ?? "0");
                                item.MeltNumber = dataTable.Rows[i][1].ToString();
                                item.SteelMark = dataTable.Rows[i][2].ToString();
                                item.IngotProfile = dataTable.Rows[i][3].ToString();
                                item.IngotsCount = int.Parse(dataTable.Rows[i][4].ToString() ?? "0");
                                item.WeightAll = int.Parse(dataTable.Rows[i][5].ToString() ?? "0");
                                item.WeightOne = int.Parse(dataTable.Rows[i][6].ToString() ?? "0");
                                item.IngotLength = int.Parse(dataTable.Rows[i][7].ToString() ?? "0");
                                item.Standart = dataTable.Rows[i][8].ToString();

                                string diam = dataTable.Rows[i][9].ToString() ?? "0";
                                diam = diam.Replace(".", ",");
                                item.Diameter = double.Parse(diam);
                                item.Customer = dataTable.Rows[i][10].ToString();
                                item.Shift = dataTable.Rows[i][11].ToString();
                                item.IngotClass = dataTable.Rows[i][12].ToString();
                                item.ProductCode = int.Parse(dataTable.Rows[i][13].ToString() ?? "0");
                                item.Weighted = int.Parse(dataTable.Rows[i][14].ToString() ?? "0");
                            }
                            catch (Exception ex)
                            {
                                item.LandingId = 0;
                                item.MeltNumber = "";
                                item.SteelMark = "";
                                item.IngotProfile = "";
                                item.IngotsCount = 0;
                                item.WeightAll = 0;
                                item.WeightOne = 0;
                                item.IngotLength = 0;
                                item.Standart = "";
                                item.Diameter = 0;
                                item.Customer = "";
                                item.Shift = "";
                                item.IngotClass = "";
                                item.ProductCode = 0;
                                item.Weighted = 0;
                                _logger.Error(
                                    $"Ошибка при получении списка очереди заготовок на посаде печи [{ex.Message}]");
                            }

                            result.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка получения данных из БД: {ex.Message}");
            }

            return result;
        }

        public List<CoilData> GetCoilData(bool current = true, bool last = true)
        {
            List<CoilData> result = new List<CoilData>();

            if (current)
            {
                // Получить список бунтов для текущей плавки
                List<LandingData> landed = GetLandingOrder();
                foreach (LandingData item in landed)
                {
                    if (item.Weighted > 0)
                    {
                        result = GetCoilsByMelt(item.MeltNumber, item.Diameter, last);
                    }
                }
            }
            else
            {
                // Получить список бунтов для предыдущей плавки
                Dictionary<string, double> previous = GetPreviousMeltNumber();
                foreach (KeyValuePair<string, double> melt in previous)
                {
                    if (!string.IsNullOrEmpty(melt.Key) && melt.Value > 0)
                    {
                        result = GetCoilsByMelt(melt.Key, melt.Value, last);
                    }
                }

            }

            return result;
        }

        /// <summary>
        /// Получить список плавок за период
        /// </summary>
        /// <param name="dateStart">Начало периода</param>
        /// <param name="dateFinish">Конец периода</param>
        /// <returns>Список плавок</returns>
        public List<LandingData> GetMeltsListForPeriod(DateTime dateStart, DateTime dateFinish)
        {
            List<LandingData> result = new List<LandingData>();
            DataTable dataTable = new DataTable();
            string query =
                $"select * from public.f_get_queue_period('{dateStart:O}', '{dateFinish:O}') where c_count_weight>0 order by c_melt, c_date_reg;";
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    new NpgsqlDataAdapter(new NpgsqlCommand(query, connection)).Fill(dataTable);
                    connection.Close();
                    if (dataTable.Rows.Count > 0)
                    {
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            LandingData item = new LandingData();
                            try
                            {
                                string val = dataTable.Rows[i][0].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.LandingId = int.Parse(val);

                                val = dataTable.Rows[i][1].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "01-01-2020 00:00:00";
                                item.LandingDate = DateTime.Parse(val);

                                val = dataTable.Rows[i][2].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.MeltNumber = val;

                                val = dataTable.Rows[i][3].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.SteelMark = val;

                                val = dataTable.Rows[i][4].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0x0";
                                item.IngotProfile = val;

                                val = dataTable.Rows[i][5].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.IngotsCount = int.Parse(val);

                                val = dataTable.Rows[i][6].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.WeightAll = int.Parse(val);

                                val = dataTable.Rows[i][7].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.WeightOne = int.Parse(val);

                                val = dataTable.Rows[i][8].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.IngotLength = int.Parse(val);

                                val = dataTable.Rows[i][9].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.Standart = val;

                                val = dataTable.Rows[i][10].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.ProductProfile = val;

                                val = dataTable.Rows[i][11].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                val = val.Replace(".", ",");
                                item.Diameter = double.Parse(val);

                                val = dataTable.Rows[i][12].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.Customer = val;

                                val = dataTable.Rows[i][13].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.Shift = val;

                                val = dataTable.Rows[i][14].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.IngotClass = val;

                                val = dataTable.Rows[i][15].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.ProductCode = int.Parse(val);

                                val = dataTable.Rows[i][16].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.Weighted = int.Parse(val);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(
                                    $"Не удалось получить данные плавки [{ex.Message}]");
                            }

                            result.Add(item);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Не удалось получить список плавок за период с {dateStart.ToString("G")} по {dateFinish.ToString("G")} [{ex.Message}]");
            }

            return result;
        }

        public List<LandingData> GetMeltByNumber(string meltNumber, double diameter = 0)
        {
            List<LandingData> result = new List<LandingData>();
            DataTable dataTable = new DataTable();
            string diam = diameter.ToString("F1").Replace(",", ".");
            string query = diameter > 0
                ? $"select * from public.f_get_all_queues() where c_melt='{meltNumber}' and c_diameter={diam} order by c_date_reg;"
                : $"select * from public.f_get_all_queues() where c_melt='{meltNumber}' order by c_date_reg;";

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    new NpgsqlDataAdapter(new NpgsqlCommand(query, connection)).Fill(dataTable);
                    connection.Close();
                    if (dataTable.Rows.Count > 0)
                    {
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            LandingData item = new LandingData();
                            try
                            {
                                string val = dataTable.Rows[i][0].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.LandingId = int.Parse(val);

                                val = dataTable.Rows[i][1].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "01-01-2020 00:00:00";
                                item.LandingDate = DateTime.Parse(val);

                                val = dataTable.Rows[i][2].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.MeltNumber = val;

                                val = dataTable.Rows[i][3].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.SteelMark = val;

                                val = dataTable.Rows[i][4].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0x0";
                                item.IngotProfile = val;

                                val = dataTable.Rows[i][5].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.IngotsCount = int.Parse(val);

                                val = dataTable.Rows[i][6].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.WeightAll = int.Parse(val);

                                val = dataTable.Rows[i][7].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.WeightOne = int.Parse(val);

                                val = dataTable.Rows[i][8].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.IngotLength = int.Parse(val);

                                val = dataTable.Rows[i][9].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.Standart = val;

                                val = dataTable.Rows[i][10].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                val = val.Replace(".", ",");
                                item.Diameter = double.Parse(val);

                                val = dataTable.Rows[i][11].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.Customer = val;

                                val = dataTable.Rows[i][12].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.Shift = val;

                                val = dataTable.Rows[i][13].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.IngotClass = val;

                                val = dataTable.Rows[i][14].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.ProductCode = int.Parse(val);

                                val = dataTable.Rows[i][15].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.ProductProfile = val;

                                val = dataTable.Rows[i][16].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.Weighted = int.Parse(val);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(
                                    $"Не удалось получить плавки №{meltNumber}  [{ex.Message}]");
                            }

                            result.Add(item);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Не удалось получить плавки №{meltNumber}  [{ex.Message}]");
            }

            return result;
        }

        /// <summary>
        /// Получить номер предыдущей завершенной плавки
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, double> GetPreviousMeltNumber()
        {
            Dictionary<string, double> result = new Dictionary<string, double>();
            DataTable dataTable = new DataTable();

            string query = "call public.p_get_previos_melt(null, null);";
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    new NpgsqlDataAdapter(new NpgsqlCommand(query, connection)).Fill(dataTable);
                    connection.Close();
                    if (dataTable.Rows.Count > 0)
                    {
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            string key = "";
                            double value = 0;

                            try
                            {
                                string val = dataTable.Rows[i][0].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                key = val;

                                val = dataTable.Rows[i][1].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                val = val.Replace(".", ",");
                                value = double.Parse(val);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(
                                    $"Не удалось получить номер и диаметр предыдущей провешеной плавки [{ex.Message}]");
                            }

                            result.Add(key, value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Не удалось получить номер и диаметр предыдущей провешеной плавки [{ex.Message}]");
            }

            return result;
        }

        /// <summary>
        /// Получить список плавок, у которых бунты взвешивались в указанный период
        /// </summary>
        /// <param name="startPeriod">Начало периода</param>
        /// <param name="finishPeriod">Конец периода</param>
        /// <returns>Список плавок, у которых бунты взвешивались в указанный периода</returns>
        public List<MeltsList> GetMeltsListByPeriod(DateTime startPeriod, DateTime finishPeriod)
        {
            List<MeltsList> result = new List<MeltsList>();
            DataTable dataTable = new DataTable();
            string query =
                // $"select distinct c_melt_no, c_diameter from public.f_get_coils_period_hourly_detail('{startPeriod:O}', " +
                // $"'{finishPeriod:O}') order by c_melt_no;";

                $"select c_melt, c_diameter from public.f_get_coils_period('{startPeriod:O}'," +
                $"'{finishPeriod:O}') group by c_melt, c_diameter order by c_melt, c_diameter;";

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    new NpgsqlDataAdapter(new NpgsqlCommand(query, connection)).Fill(dataTable);
                    connection.Close();
                    if (dataTable.Rows.Count > 0)
                    {
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            string key = "";
                            double value = 0;
                            MeltsList melt = new MeltsList();

                            try
                            {
                                string val = dataTable.Rows[i][0].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                key = val;

                                val = dataTable.Rows[i][1].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                val = val.Replace(".", ",");
                                value = double.Parse(val);

                                melt.MeltNumber = key;
                                melt.Diameter = value;
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(
                                    $"Не удалось прочитать данные по плавкам за период с [{startPeriod:dd-MM-yyyy hh:mm:ss}] " +
                                    $"по [{finishPeriod:dd-MM-yyyy hh:mm:ss}] ({ex.Message})");
                            }

                            result.Add(melt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Не удалось получить данные по плавкам за период с [{startPeriod:dd-MM-yyyy hh:mm:ss}] " +
                    $"по [{finishPeriod:dd-MM-yyyy hh:mm:ss}] ({ex.Message})");
            }

            return result;
        }

        /// <summary>
        /// Получить список всех бунтов, которые взвешивались в указанный период
        /// </summary>
        /// <param name="startPeriod">Начало периода</param>
        /// <param name="finishPeriod">Конец периода</param>
        /// <returns>Список бунтов</returns>
        public List<CoilData> GetAllCoilsByPeriod(DateTime startPeriod, DateTime finishPeriod)
        {
            List<CoilData> result = new List<CoilData>();
            DataTable dataTable = new DataTable();
            string query =
                $"select * from public.f_get_coils_period('{startPeriod:O}', '{finishPeriod:O}');";
            // string query =
            //     $"select * from public.f_get_weighted_coils('{startPeriod:O}', '{finishPeriod:O}');";

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    new NpgsqlDataAdapter(new NpgsqlCommand(query, connection)).Fill(dataTable);
                    connection.Close();
                    if (dataTable.Rows.Count > 0)
                    {
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            CoilData item = new CoilData();
                            try
                            {
                                string val = dataTable.Rows[i][0].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.PosadUid = int.Parse(val);

                                val = dataTable.Rows[i][1].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.MeltNumber = val;

                                val = dataTable.Rows[i][2].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.CoilUid = int.Parse(val);

                                val = dataTable.Rows[i][3].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "01-01-2020 00:00:00";
                                item.DateReg = DateTime.Parse(val);

                                val = dataTable.Rows[i][4].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "01-01-2020 00:00:00";
                                item.DateWeight = DateTime.Parse(val);

                                val = dataTable.Rows[i][5].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.IngotsCount = int.Parse(val);

                                val = dataTable.Rows[i][6].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.CoilPos = int.Parse(val);

                                val = dataTable.Rows[i][7].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.WeightFact = int.Parse(val);

                                val = dataTable.Rows[i][8].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.CoilNumber = int.Parse(val);

                                val = dataTable.Rows[i][9].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.SteelMark = val;

                                val = dataTable.Rows[i][10].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.IngotProfile = val;

                                val = dataTable.Rows[i][11].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.WeightAll = int.Parse(val);

                                val = dataTable.Rows[i][12].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.WeightOne = int.Parse(val);

                                val = dataTable.Rows[i][13].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.IngotLength = int.Parse(val);

                                val = dataTable.Rows[i][14].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.Standart = val;

                                val = dataTable.Rows[i][15].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.ProductionProfile = val;

                                val = dataTable.Rows[i][16].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                val = val.Replace(".", ",");
                                item.Diameter = double.Parse(val);

                                val = dataTable.Rows[i][17].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.Customer = val;

                                val = dataTable.Rows[i][18].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.ShiftNumber = val;

                                val = dataTable.Rows[i][19].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.Class = val;

                                val = dataTable.Rows[i][20].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.ProductionCode = int.Parse(val);

                                val = dataTable.Rows[i][21].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.ShiftNumber = val;

                                val = dataTable.Rows[i][22].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.Specification = val;

                                val = dataTable.Rows[i][23].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.Lot = int.Parse(val);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(
                                    $"Не удалось прочитать данные по плавкам за период с [{startPeriod:dd-MM-yyyy hh:mm:ss}] " +
                                    $"по [{finishPeriod:dd-MM-yyyy hh:mm:ss}] ({ex.Message})");
                            }

                            result.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Не удалось получить данные по плавкам за период с [{startPeriod:dd-MM-yyyy hh:mm:ss}] " +
                    $"по [{finishPeriod:dd-MM-yyyy hh:mm:ss}] ({ex.Message})");
            }

            return result;
        }

        /// <summary>
        /// Получить сводную таблицу по плавкам за период
        /// </summary>
        /// <param name="startPeriod">Начало периода</param>
        /// <param name="finishPeriod">Конец периода</param>
        /// <returns>Сводная таблица по плавкам</returns>
        public List<MeltsList> GetMeltsListSummary(DateTime startPeriod, DateTime finishPeriod)
        {
            List<MeltsList> result = new List<MeltsList>();
            DataTable dataTable = new DataTable();
            string query =
                $"select * from public.f_get_coils_period_summary('{startPeriod:O}','{finishPeriod:O}');";

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    new NpgsqlDataAdapter(new NpgsqlCommand(query, connection)).Fill(dataTable);
                    connection.Close();
                    if (dataTable.Rows.Count > 0)
                    {
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            MeltsList item = new MeltsList();
                            try
                            {
                                string val = dataTable.Rows[i][0].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.MeltNumber = val;
                                
                                val = dataTable.Rows[i][1].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.ProductProfile = val;
                                
                                val = dataTable.Rows[i][2].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                val = val.Replace(".", ",");
                                item.Diameter = double.Parse(val);
                                
                                val = dataTable.Rows[i][3].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "01-01-2020 00:00:00";
                                item.PeriodStart = DateTime.Parse(val);

                                val = dataTable.Rows[i][4].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "01-01-2020 00:00:00";
                                item.PeriodFinish = DateTime.Parse(val);
                                
                                val = dataTable.Rows[i][5].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.SteelMark = val;
                                
                                val = dataTable.Rows[i][6].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.IngotProfile = val;
                                
                                val = dataTable.Rows[i][7].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.IngotsCount = int.Parse(val);
                                
                                val = dataTable.Rows[i][8].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.IngotLength = int.Parse(val);
                                
                                val = dataTable.Rows[i][9].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.Standart = val;
                                
                                val = dataTable.Rows[i][10].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.ProductCode = int.Parse(val);
                                
                                val = dataTable.Rows[i][11].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.Customer = val;
                                
                                val = dataTable.Rows[i][12].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.CoilsCount = int.Parse(val);
                                
                                val = dataTable.Rows[i][13].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.TotalWeight = int.Parse(val);
                                
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(
                                    $"Не удалось прочитать данные по плавкам за период с [{startPeriod:dd-MM-yyyy hh:mm:ss}] " +
                                    $"по [{finishPeriod:dd-MM-yyyy hh:mm:ss}] ({ex.Message})");
                            }
                            
                            result.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Не удалось получить данные по плавкам за период с [{startPeriod:dd-MM-yyyy hh:mm:ss}] " +
                    $"по [{finishPeriod:dd-MM-yyyy hh:mm:ss}] ({ex.Message})");
            }

            return result;
        }

        public List<CoilData> GetHourlyCoilsByPeriodDetail(DateTime startPeriod, DateTime finishPeriod)
        {
            List<CoilData> result = new List<CoilData>();
            DataTable dataTable = new DataTable();
            string query =
                $"select * from public.f_get_coils_period_hourly_detail('{startPeriod:O}', '{finishPeriod:O}') order by c_weighting_data;";

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    new NpgsqlDataAdapter(new NpgsqlCommand(query, connection)).Fill(dataTable);
                    connection.Close();
                    if (dataTable.Rows.Count > 0)
                    {
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            CoilData item = new CoilData();
                            try
                            {
                                string val = dataTable.Rows[i][0].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.PosadUid = int.Parse(val);

                                val = dataTable.Rows[i][1].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.MeltNumber = val;

                                val = dataTable.Rows[i][2].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.ProductionProfile = val;

                                val = dataTable.Rows[i][3].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                val = val.Replace(".", ",");
                                item.Diameter = double.Parse(val);

                                val = dataTable.Rows[i][4].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.ShiftNumber = val;

                                val = dataTable.Rows[i][5].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.Specification = val;

                                val = dataTable.Rows[i][6].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.Lot = int.Parse(val);

                                val = dataTable.Rows[i][7].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.CoilNumber = int.Parse(val);

                                val = dataTable.Rows[i][8].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.WeightFact = int.Parse(val);

                                val = dataTable.Rows[i][9].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "01-01-2020 00:00:00";
                                item.DateWeight = DateTime.Parse(val);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(
                                    $"Не удалось прочитать данные по плавкам за период с [{startPeriod:dd-MM-yyyy hh:mm:ss}] " +
                                    $"по [{finishPeriod:dd-MM-yyyy hh:mm:ss}] ({ex.Message})");
                            }

                            result.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Не удалось получить данные по плавкам за период с [{startPeriod:dd-MM-yyyy hh:mm:ss}] " +
                    $"по [{finishPeriod:dd-MM-yyyy hh:mm:ss}] ({ex.Message})");
            }

            return result;
        }

        public List<MeltsForPeriod> GetHourlyCoilsByPeriod(DateTime startPeriod, DateTime finishPeriod)
        {
            List<MeltsForPeriod> result = new List<MeltsForPeriod>();
            DataTable dataTable = new DataTable();
            string query =
                $"select * from f_get_coils_period_hourly('{startPeriod:O}', '{finishPeriod:O}');";

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    new NpgsqlDataAdapter(new NpgsqlCommand(query, connection)).Fill(dataTable);
                    connection.Close();
                    if (dataTable.Rows.Count > 0)
                    {
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            MeltsForPeriod item = new MeltsForPeriod();
                            try
                            {
                                string val = dataTable.Rows[i][0].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "01-01-2020 00:00:00";
                                item.WeightingData = DateTime.Parse(val);

                                val = dataTable.Rows[i][1].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.WeightingHourStart = int.Parse(val);
                                item.WeightingHourFinish = item.WeightingHourStart + 1;

                                val = dataTable.Rows[i][2].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.CoilsCount = int.Parse(val);

                                val = dataTable.Rows[i][3].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.WeightFact = int.Parse(val);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(
                                    $"Не удалось прочитать данные по плавкам за период с [{startPeriod:dd-MM-yyyy hh:mm:ss}] " +
                                    $"по [{finishPeriod:dd-MM-yyyy hh:mm:ss}] ({ex.Message})");
                            }

                            result.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Не удалось получить данные по плавкам за период с [{startPeriod:dd-MM-yyyy hh:mm:ss}] " +
                    $"по [{finishPeriod:dd-MM-yyyy hh:mm:ss}] ({ex.Message})");
            }

            return result;
        }

        public List<CoilData> GetCoilsByMelt(string melt, double diameter, bool last = true)
        {
            List<CoilData> result = new List<CoilData>();
            DataTable dataTable = new DataTable();

            string diam = diameter.ToString("F1").Replace(",", ".");

            string query = !last
                ? $"select * from public.f_get_queue_coils('{melt}', {diam}) order by c_date_weight;"
                : $"select * from public.f_get_queue_coils('{melt}', {diam}) order by c_date_weight desc limit 1;";

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    new NpgsqlDataAdapter(new NpgsqlCommand(query, connection)).Fill(dataTable);
                    connection.Close();
                    if (dataTable.Rows.Count > 0)
                    {

                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            CoilData item = new CoilData();
                            try
                            {
                                string val = dataTable.Rows[i][0].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.PosadUid = int.Parse(val);

                                val = dataTable.Rows[i][1].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.MeltNumber = val;

                                val = dataTable.Rows[i][9].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.ProductionProfile = val;

                                val = dataTable.Rows[i][10].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                val = val.Replace(".", ",");
                                item.Diameter = double.Parse(val);

                                val = dataTable.Rows[i][15].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.CoilUid = int.Parse(val);

                                val = dataTable.Rows[i][16].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.CoilPos = int.Parse(val);

                                val = dataTable.Rows[i][17].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.CoilNumber = int.Parse(val);

                                val = dataTable.Rows[i][18].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.WeightFact = int.Parse(val);

                                val = dataTable.Rows[i][19].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.ShiftNumber = val;

                                val = dataTable.Rows[i][20].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.Specification = val;

                                val = dataTable.Rows[i][21].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.Lot = int.Parse(val);

                                val = dataTable.Rows[i][22].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "01-01-2020 00:00:00";
                                item.DateReg = DateTime.Parse(val);

                                val = dataTable.Rows[i][23].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "01-01-2020 00:00:00";
                                item.DateWeight = DateTime.Parse(val);

                            }
                            catch (Exception ex)
                            {
                                _logger.Error(
                                    $"Не удалось получить список бунтов для плавки №{melt} с диаметром {diam} [{ex.Message}]");
                            }

                            result.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Не удалось получить данные для плавки №{melt} с диаметром {diam} [{ex.Message}]");
            }

            return result;
        }

        /// <summary>
        /// Получить сводные данные по сменам за период
        /// </summary>
        /// <param name="startPeriod">Начало периода</param>
        /// <param name="finishPeriod">Конец периода</param>
        /// <returns>Сводные данные по сменам за период</returns>
        public List<ShiftReport> GetShiftReport(DateTime startPeriod, DateTime finishPeriod)
        {
            List<ShiftReport> result = new List<ShiftReport>();
            DataTable dataTable = new DataTable();
            string query = $"select * from public.f_get_report_by_shift('{startPeriod:O}', '{finishPeriod:O}');";
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    new NpgsqlDataAdapter(new NpgsqlCommand(query, connection)).Fill(dataTable);
                    connection.Close();
                    if (dataTable.Rows.Count > 0)
                    {
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            ShiftReport item = new ShiftReport();
                            try
                            {
                                string val = dataTable.Rows[i][0].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "01-01-2020 00:00:00";
                                item.PeriodStart = DateTime.Parse(val);

                                val = dataTable.Rows[i][1].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "01-01-2020 00:00:00";
                                item.PeriodEnd = DateTime.Parse(val);

                                val = dataTable.Rows[i][2].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.CoilsCount = int.Parse(val);

                                val = dataTable.Rows[i][3].ToString();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.CoilsWeight = int.Parse(val);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(
                                    $"Не удалось прочитать сводные данные по сменам за период с {startPeriod} по {finishPeriod} [{ex.Message}]");
                            }

                            result.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Не удалось получить сводные данные по сменам за период с {startPeriod} по {finishPeriod} [{ex.Message}]");
            }

            return result;
        }
    }
}