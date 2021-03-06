﻿using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using NLog;
using Npgsql;

namespace OvenReports.Data
{
    public class DbConnection
    {
        private readonly string _connectionString;
        private readonly string _connectionStringTest;
        private readonly Logger _logger;
        private readonly QueryRequests _requests;

        /// <summary>
        /// Конструктор создания подключения к базе данных
        /// </summary>
        public DbConnection(bool test=false)
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
            
            // "DBConnection": {
            //     "Host": "10.23.196.58",
            //     "Port": "5432",
            //     "Database": "mtsbase",
            //     "UserName": "mts",
            //     "Password": "dfaf@we jkjcld!",
            //     "sslmode": "Prefer",
            //     "Trust Server Certificate": "true",
            //     "Reconnect": "5000"
            // }


            // Читаем параметры подключения к СУБД PostgreSQL
            _logger = LogManager.GetCurrentClassLogger();
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            // Настройки для подключения к базе данных на проде
            string host = config.GetSection("DBConnection:Host").Value;
            int port = int.Parse(config.GetSection("DBConnection:Port").Value);
            string database = config.GetSection("DBConnection:Database").Value;
            string user = config.GetSection("DBConnection:UserName").Value;
            string password = config.GetSection("DBConnection:Password").Value;
            int timeout = int.Parse(config.GetSection("DBConnection:Timeout").Value);
            string sslMode = config.GetSection("DBConnection:SslMode").Value;
            string trustServerCert = config.GetSection("DBConnection:TrustServerCertificate").Value;
            
            // Настроки для подключения к базе данных на тесте
            string hostT = config.GetSection("DBTest:Host").Value;
            int portT = int.Parse(config.GetSection("DBTest:Port").Value);
            string databaseT = config.GetSection("DBTest:Database").Value;
            string userT = config.GetSection("DBTest:UserName").Value;
            string passwordT = config.GetSection("DBTest:Password").Value;
            int timeoutT = int.Parse(config.GetSection("DBTest:Timeout").Value);
            string sslModeT = config.GetSection("DBTest:SslMode").Value;
            string trustServerCertT = config.GetSection("DBTest:TrustServerCertificate").Value;

            // Строка подключения для базы данных на проде
            _connectionString =
                $"Server={host};Username={user};Database={database};Port={port};Password={password};" +
                $"SSL Mode={sslMode};Trust Server Certificate={trustServerCert};CommandTimeout={timeout}";
            
            // Строка подключения для базы данных на тесте
            _connectionStringTest =
                $"Server={hostT};Username={userT};Database={databaseT};Port={portT};Password={passwordT};" +
                $"SSL Mode={sslModeT};Trust Server Certificate={trustServerCertT};CommandTimeout={timeoutT}";

            _requests = new QueryRequests();
        }


        /// <summary>
        /// Получить данные плавки по номеру
        /// </summary>
        /// <param name="meltNumber">Номер плавки</param>
        /// <param name="diameter">Диаметр прокатываемого профиля</param>
        /// <returns>Данные плавки</returns>
        public List<LandingData> GetMeltByNumber(string meltNumber, double diameter = 0)
        {
            List<LandingData> result = new List<LandingData>();
            DataTable dataTable = new DataTable();
            string diam = diameter.ToString("F1").Replace(",", ".");
            string query = diameter > 0
                ? $"select * from public.f_get_all_queues('{meltNumber}') where c_diameter={diam} order by c_date_reg;"
                : $"select * from public.f_get_all_queues('{meltNumber}') order by c_date_reg;";

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
                                string val = dataTable.Rows[i][0].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.LandingId = int.Parse(val);

                                val = dataTable.Rows[i][1].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                item.LandingDate = DateTime.Parse(val);

                                val = dataTable.Rows[i][2].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.MeltNumber = val;

                                val = dataTable.Rows[i][3].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.SteelMark = val;

                                val = dataTable.Rows[i][4].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0x0";
                                item.IngotProfile = val;

                                val = dataTable.Rows[i][5].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.IngotsCount = int.Parse(val);

                                val = dataTable.Rows[i][6].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.WeightAll = int.Parse(val);

                                val = dataTable.Rows[i][7].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.WeightOne = int.Parse(val);

                                val = dataTable.Rows[i][8].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.IngotLength = int.Parse(val);

                                val = dataTable.Rows[i][9].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.Standart = val;

                                val = dataTable.Rows[i][10].ToString()?.Trim().Replace(".", ",");
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.Diameter = double.Parse(val);

                                val = dataTable.Rows[i][11].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.Customer = val;

                                val = dataTable.Rows[i][12].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.Shift = val;

                                val = dataTable.Rows[i][13].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.IngotClass = val;

                                val = dataTable.Rows[i][14].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.ProductCode = int.Parse(val);

                                val = dataTable.Rows[i][15].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.ProductProfile = val;

                                val = dataTable.Rows[i][16].ToString()?.Trim();
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
                    $"Не удалось получить плавки №{meltNumber} [{ex.Message}]");
            }

            return result;
        }

        /// <summary>
        /// Получить сводную таблицу по плавкам за период
        /// </summary>
        /// <param name="startPeriod">Начало периода</param>
        /// <param name="finishPeriod">Конец периода</param>
        /// <param name="order">Тип сортировки результата</param>
        /// <returns>Сводная таблица по плавкам</returns>
        public List<MeltsList> GetMeltsListSummary(DateTime startPeriod, DateTime finishPeriod, OrderTypes order=OrderTypes.OrderByMeltNumber)
        {
            List<MeltsList> result = new List<MeltsList>();
            DataTable dataTable = new DataTable();
            string query =
                $"select * from public.f_get_coils_period_summary('{startPeriod:O}','{finishPeriod:O}') "; 
            
            // Определяем тип сортировки результата
            switch (order)
            {
                case OrderTypes.OrderByMeltNumber:
                {
                    // По номеру плавки
                    query += "order by c_melt, c_start_time;";
                    break;
                }
                case OrderTypes.OrderByPeriodStart:
                {
                    // По дате начала проката
                    query += "order by c_start_time;";
                    break;
                }
                case OrderTypes.OrderByPeriodFinish:
                {
                    // По дате окончания проката
                    query += "order by c_finish_time;";
                    break;
                }
            }

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
                                string val = dataTable.Rows[i][0].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.MeltNumber = val;
                                
                                val = dataTable.Rows[i][1].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.ProductProfile = val;
                                
                                val = dataTable.Rows[i][2].ToString()?.Trim().Replace(".", ",");
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.Diameter = double.Parse(val);
                                
                                val = dataTable.Rows[i][3].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                item.PeriodStart = DateTime.Parse(val);

                                val = dataTable.Rows[i][4].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                item.PeriodFinish = DateTime.Parse(val);
                                
                                val = dataTable.Rows[i][5].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.SteelMark = val;
                                
                                val = dataTable.Rows[i][6].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.IngotProfile = val;
                                
                                val = dataTable.Rows[i][7].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.IngotsCount = int.Parse(val);
                                
                                val = dataTable.Rows[i][8].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.IngotLength = int.Parse(val);
                                
                                val = dataTable.Rows[i][9].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.Standart = val;
                                
                                val = dataTable.Rows[i][10].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.ProductCode = int.Parse(val);
                                
                                val = dataTable.Rows[i][11].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.Customer = val;
                                
                                val = dataTable.Rows[i][12].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.CoilsCount = int.Parse(val);
                                
                                val = dataTable.Rows[i][13].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.TotalWeight = int.Parse(val);
                                
                                val = dataTable.Rows[i][14].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.MeltId = int.Parse(val);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(
                                    $"Не удалось прочитать данные по плавкам за период с [{startPeriod:G}] " +
                                    $"по [{finishPeriod:G}] ({ex.Message})");
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
        /// Получить данные по бунтам за период
        /// </summary>
        /// <param name="startPeriod">Начало периода</param>
        /// <param name="finishPeriod">Конец периода</param>
        /// <returns>Данные по бунтам</returns>
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
                                string val = dataTable.Rows[i][0].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.PosadUid = int.Parse(val);

                                val = dataTable.Rows[i][1].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.MeltNumber = val;

                                val = dataTable.Rows[i][2].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.ProductionProfile = val;

                                val = dataTable.Rows[i][3].ToString()?.Trim().Replace(".", ",");
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.Diameter = double.Parse(val);

                                val = dataTable.Rows[i][4].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.ShiftNumber = val;

                                val = dataTable.Rows[i][5].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.Specification = val;

                                val = dataTable.Rows[i][6].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.Lot = int.Parse(val);

                                val = dataTable.Rows[i][7].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.CoilNumber = int.Parse(val);

                                val = dataTable.Rows[i][8].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.WeightFact = int.Parse(val);

                                val = dataTable.Rows[i][9].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                item.DateWeight = DateTime.Parse(val);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(
                                    $"Не удалось прочитать данные по плавкам за период с [{startPeriod:G}] " +
                                    $"по [{finishPeriod:G}] ({ex.Message})");
                            }

                            result.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Не удалось получить данные по плавкам за период с [{startPeriod:G}] " +
                    $"по [{finishPeriod:G}] ({ex.Message})");
            }

            return result;
        }

        /// <summary>
        /// Получить данные по бунтам за период с группировкой по часам
        /// </summary>
        /// <param name="startPeriod">Начало периода</param>
        /// <param name="finishPeriod">Конец периода</param>
        /// <returns>Данные по бунтам</returns>
        public List<MeltsForPeriod> GetHourlyCoilsByPeriod(DateTime startPeriod, DateTime finishPeriod)
        {
            List<MeltsForPeriod> result = new List<MeltsForPeriod>();
            DataTable dataTable = new DataTable();
            string query =
                $"select * from public.f_get_coils_period_hourly('{startPeriod:s}', '{finishPeriod:s}');";

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
                                string val = dataTable.Rows[i][0].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                item.WeightingData = DateTime.Parse(val);

                                val = dataTable.Rows[i][1].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.WeightingHourStart = int.Parse(val);
                                item.WeightingHourFinish = item.WeightingHourStart + 1;

                                val = dataTable.Rows[i][2].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.CoilsCount = int.Parse(val);

                                val = dataTable.Rows[i][3].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.WeightFact = int.Parse(val);
                                
                                val = dataTable.Rows[i][4].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.ShiftNumber = int.Parse(val);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(
                                    $"Не удалось прочитать данные по плавкам за период с [{startPeriod:G}] " +
                                    $"по [{finishPeriod:G}] ({ex.Message})");
                            }

                            result.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Не удалось получить данные по плавкам за период с [{startPeriod:G}] " +
                    $"по [{finishPeriod:G}] ({ex.Message})");
            }

            return result;
        }

        /// <summary>
        /// Получить данные бунтов по номеру плавки
        /// </summary>
        /// <param name="melt">Номер плавки</param>
        /// <param name="diameter">Диаметр прокатываемого профиля</param>
        /// <param name="last">Только последний бунт</param>
        /// <returns>Данные бунтов</returns>
        public List<CoilData> GetCoilsByMelt(string melt, double diameter, bool last=true)
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
                                string val = dataTable.Rows[i][0].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.PosadUid = int.Parse(val);

                                val = dataTable.Rows[i][1].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.MeltNumber = val;

                                val = dataTable.Rows[i][9].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.ProductionProfile = val;

                                val = dataTable.Rows[i][10].ToString()?.Trim().Replace(".", ",");
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.Diameter = double.Parse(val);

                                val = dataTable.Rows[i][15].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.CoilUid = int.Parse(val);

                                val = dataTable.Rows[i][16].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.CoilPos = int.Parse(val);

                                val = dataTable.Rows[i][17].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.CoilNumber = int.Parse(val);

                                val = dataTable.Rows[i][18].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.WeightFact = int.Parse(val);

                                val = dataTable.Rows[i][19].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.ShiftNumber = val;

                                val = dataTable.Rows[i][20].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = " ";
                                item.Specification = val;

                                val = dataTable.Rows[i][21].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.Lot = int.Parse(val);

                                val = dataTable.Rows[i][22].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                item.DateReg = DateTime.Parse(val);

                                val = dataTable.Rows[i][23].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
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
        /// Запрос на выборку плавок, заготовки которых были взвешаны за период
        /// </summary>
        /// <param name="periodStart">Начало периода</param>
        /// <param name="periodEnd">Конец периода</param>
        /// <param name="meltNo">Номер плавки</param>
        /// <returns>Список заготовок, которые были взвешены за период</returns>
        public List<IngotsWeights> GetMeltsWeight(DateTime periodStart, DateTime periodEnd, string meltNo)
        {
            string query = "with l as (select unit_id, min(tm_beg) tm_beg, max(tm_end) tm_end " +
                           "from mts.locations l where l.node_id = 20100 ";
            
            if(string.IsNullOrEmpty(meltNo))
                query += $"and tm_beg between '{periodStart:O}'::timestamp and '{periodEnd:O}'::timestamp ";

            query += "group by unit_id) " +
                     "select p1.value_s as melt, " +
                     "p18.value_s as profile, " +
                     "p10.value_s as diameter, " +
                     "min(l.tm_beg) start_weight, max(l.tm_end) stop_weight, " +
                     "p2.value_s as steel_grade, " +
                     "count(t.unit_id) billet_count, " +
                     "p3.value_s as section, " +
                     "sum(p4.value_n) as billet_weight, " +
                     "p11.value_s as ingot_length, " +
                     "p12.value_s as standart, " +
                     "p13.value_s as prod_code, " +
                     "p14.value_s as customer " +
                     "from l join mts.units_relations r on r.unit_id_child = l.unit_id " +
                     "join mts.unit_tasks t on t.unit_id = r.unit_id_parent and t.node_id = 20100 " +
                     "join mts.units_relations rp on rp.unit_id_child = r.unit_id_parent " +
                     "join mts.passport p1 on p1.unit_id = rp.unit_id_parent and p1.param_id=10000001 " +
                     "left join mts.passport p2 on p2.unit_id = rp.unit_id_parent and p2.param_id=10000002 " +
                     "left join mts.passport p3 on p3.unit_id = rp.unit_id_parent and p3.param_id=10000003 " +
                     "left join mts.passport p18 on p18.unit_id = rp.unit_id_parent and p18.param_id=10000018 " +
                     "left join mts.passport p10 on p10.unit_id = rp.unit_id_parent and p10.param_id=10000010 " +
                     "left join mts.passport p11 on p11.unit_id = rp.unit_id_parent and p11.param_id=10000007 " +
                     "left join mts.passport p12 on p12.unit_id = rp.unit_id_parent and p12.param_id=10000009 " +
                     "left join mts.passport p13 on p13.unit_id = rp.unit_id_parent and p13.param_id=10000015 " +
                     "left join mts.passport p14 on p14.unit_id = rp.unit_id_parent and p14.param_id=10000011 " +
                     "join mts.passport p4 on p4.unit_id = l.unit_id and p4.param_id=1000 ";
            
            if (!string.IsNullOrEmpty(meltNo))
                query += $"where p1.value_s = '{meltNo}' ";

            query +=
                "group by p1.value_s, p18.value_s, p10.value_s, p2.value_s, p3.value_s, p11.value_s, p12.value_s, p13.value_s, p14.value_s order by min(tm_beg);";

            List<IngotsWeights> result = new List<IngotsWeights>();
            DataTable dataTable = new DataTable();
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionStringTest))
                {
                    connection.Open();
                    new NpgsqlDataAdapter(new NpgsqlCommand(query, connection)).Fill(dataTable);
                    connection.Close();
                    if (dataTable.Rows.Count > 0)
                    {
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            IngotsWeights item = new IngotsWeights();
                            try
                            {
                                string val = dataTable.Rows[i][0].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.Melt = val;

                                val = dataTable.Rows[i][1].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "";
                                item.Profile = val;
                                
                                val = dataTable.Rows[i][2].ToString()?.Trim().Replace(".", ",");
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.Diameter = double.Parse(val);
                                
                                val = dataTable.Rows[i][3].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                item.TimeBegin = DateTime.Parse(val);
                                
                                val = dataTable.Rows[i][4].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                item.TimeEnd = DateTime.Parse(val);
                                
                                val = dataTable.Rows[i][5].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "";
                                item.SteelGrade = val;
                                
                                val = dataTable.Rows[i][6].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.CoilsCount = int.Parse(val);
                                
                                val = dataTable.Rows[i][7].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "";
                                item.IngotProfile = val;
                                
                                val = dataTable.Rows[i][8].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.WeightTotal = int.Parse(val);
                                
                                val = dataTable.Rows[i][9].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.IngotLength = int.Parse(val);
                                
                                val = dataTable.Rows[i][10].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "";
                                item.Standart = val;
                                
                                val = dataTable.Rows[i][11].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.ProductCode = int.Parse(val);
                                
                                val = dataTable.Rows[i][12].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "";
                                item.Customer = val;
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(
                                    "Не удалось прочитать данные по плавкам за период " +
                                    $"с {periodStart:G} по {periodEnd:G} [{ex.Message}]");
                            }
                            
                            result.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    "Не удалось получить данные по плавкам за период " +
                    $"с {periodStart:G} по {periodEnd:G} [{ex.Message}]");
            }

            return result;
        }

        /// <summary>
        /// Получить данные о весе заготовки по номеру плавки
        /// </summary>
        /// <returns>Данные о весе заготовок</returns>
        public List<IngotsWeights> GetIngotsWeights(string meltNo)
        {
            string query =
                "WITH t3 as (select r.unit_id_child as unit_id,r1.unit_id_parent as id_posad,p.param_id,p.value_s, t.pos " +
                "from mts.unit_tasks t join mts.units_relations r on r.unit_id_parent = t.unit_id " +
                "join mts.units_relations r1 on r1.unit_id_child = r.unit_id_parent " +
                $"join mts.passport pm on pm.unit_id = r1.unit_id_parent and pm.param_id=10000001 and value_s ='{meltNo}' " +
                "join mts.passport p on p.unit_id = r1.unit_id_parent where t.node_id = 20100), " +
                "t0 as (select distinct l.unit_id, " +
                "first_value(l.id) over (partition by l.unit_id order by l.tm_beg, l.id) as id_beg " +
                "from mts.locations l where l.unit_id in (select unit_id from t3) and node_id = 20100), " +
                "t1 as (select t0.unit_id, p.param_id,p.value_s from mts.passport p join t0 on p.unit_id = t0.unit_id) " +
                "SELECT l.unit_id, l.tm_beg,l.tm_end, " +
                "max(case when t1.param_id = 1000 then t1.value_s end) billet_weight, t3.pos, " +
                "max(case when t3.param_id = 10000001 then t3.value_s end) melt, " +
                "max(case when t3.param_id = 10000003 then t3.value_s end) ingot_profile, " +
                "max(case when t3.param_id = 10000002 then t3.value_s end) steel_grade, " +
                "max(case when t3.param_id = 10000018 then t3.value_s end) profile, " +
                "max(case when t3.param_id = 10000010 then t3.value_s end) diameter, " +
                "max(case when t3.param_id = 10000004 then t3.value_s end) count, " +
                "max(case when t3.param_id = 10000005 then t3.value_s end) weight_all " +
                "FROM mts.locations l join t0 on t0.id_beg = l.id " +
                "left join t1 on t1.unit_id = t0.unit_id " +
                "join t3 on t3.unit_id = t0.unit_id " +
                "GROUP BY l.unit_id,l.tm_beg,l.tm_end, t3.pos " +
                "ORDER BY l.tm_beg;";
            
            List<IngotsWeights> result = new List<IngotsWeights>();
            DataTable dataTable = new DataTable();
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionStringTest))
                {
                    connection.Open();
                    new NpgsqlDataAdapter(new NpgsqlCommand(query, connection)).Fill(dataTable);
                    connection.Close();
                    if (dataTable.Rows.Count > 0)
                    {
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            IngotsWeights item = new IngotsWeights();
                            try
                            {
                                string val = dataTable.Rows[i][0].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.UnitId = int.Parse(val);
                                
                                val = dataTable.Rows[i][1].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                item.TimeBegin = DateTime.Parse(val);
                                
                                val = dataTable.Rows[i][2].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                item.TimeEnd = DateTime.Parse(val);
                                
                                val = dataTable.Rows[i][3].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.BilletWeight = int.Parse(val);
                                
                                val = dataTable.Rows[i][4].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.Position = int.Parse(val);
                                
                                val = dataTable.Rows[i][5].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.Melt = val;
                                
                                val = dataTable.Rows[i][6].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "";
                                item.IngotProfile = val;
                                
                                val = dataTable.Rows[i][7].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "";
                                item.SteelGrade = val;
                                
                                val = dataTable.Rows[i][8].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "";
                                item.Profile = val;
                                
                                val = dataTable.Rows[i][9].ToString()?.Trim().Replace(".", ",");
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.Diameter = double.Parse(val);
                                
                                val = dataTable.Rows[i][10].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.CoilsCount = int.Parse(val);
                                
                                val = dataTable.Rows[i][11].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.WeightTotal = int.Parse(val);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(
                                    $"Не удалось прочитать данные по весу заготовок для плавки №{meltNo} [{ex.Message}]");
                            }
                            
                            result.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Не удалось получить данные по весу заготовок для плавки №{meltNo} [{ex.Message}]");
            }

            return result;
        }

        /// <summary>
        /// Получить сводные данные по суткам за период
        /// </summary>
        /// <param name="startPeriod">Начало периода</param>
        /// <param name="finishPeriod">Конец периода</param>
        /// <returns>Сводные данные за период</returns>
        public List<DailyReport> GetDailyReport(DateTime startPeriod, DateTime finishPeriod)
        {
            List<DailyReport> result = new List<DailyReport>();
            DataTable dataTable = new DataTable();
            string query = $"select * from public.f_get_report_by_days('{startPeriod:O}', '{finishPeriod:O}');";
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
                            DailyReport item = new DailyReport();
                            try
                            {
                                string val = dataTable.Rows[i][0].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                item.Date = DateTime.Parse(val);
                                
                                val = dataTable.Rows[i][1].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                item.PeriodStart = DateTime.Parse(val);
                                
                                val = dataTable.Rows[i][2].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                item.PeriodEnd = DateTime.Parse(val);
                                
                                val = dataTable.Rows[i][3].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.CoilsCount = int.Parse(val);
                                
                                val = dataTable.Rows[i][4].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.CoilsWeight = int.Parse(val);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(
                                    $"Не удалось прочитать сводные данные по суткам за период с {startPeriod:G} по {finishPeriod:G} [{ex.Message}]");
                            }
                            
                            result.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Не удалось получить сводные данные по суткам за период с {startPeriod:G} по {finishPeriod:G} [{ex.Message}]");
            }

            return result;
        }


        /// <summary>
        /// Получить список плавок в печи за период
        /// </summary>
        /// <param name="start">Начало периода</param>
        /// <param name="end">Конец периода</param>
        /// <returns>Список плавок в печи</returns>
        public List<ReportMeltsInOwen> GetOwenReport(DateTime start, DateTime end)
        {
            List<ReportMeltsInOwen> result = new List<ReportMeltsInOwen>();
            DataTable dataTable = new DataTable();
            string request = _requests.GetMeltsInOwenRequest(start, end);
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    new NpgsqlDataAdapter(new NpgsqlCommand(request, connection)).Fill(dataTable);
                    connection.Close();
                    if (dataTable.Rows.Count > 0)
                    {
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            ReportMeltsInOwen item = new ReportMeltsInOwen();
                            try
                            {
                                // Номер плавки
                                string val = dataTable.Rows[i][0].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.Melt = val;

                                // Марка стали
                                val = dataTable.Rows[i][1].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "";
                                item.SteelMark = val;

                                // Сечение заготовки
                                val = dataTable.Rows[i][2].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0x0";
                                item.Section = val;

                                // Длина заготовки
                                val = dataTable.Rows[i][3].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.IngotLength = int.Parse(val);

                                // Профиль проката
                                val = dataTable.Rows[i][4].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "";
                                item.IngotProfile = val;
                                
                                // Диаметр
                                val = dataTable.Rows[i][5].ToString()?.Trim().Replace(".", ",");
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.Diameter = double.Parse(val);
                                
                                // Стандарт
                                val = dataTable.Rows[i][6].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "";
                                item.Standart = val;
                                
                                // Заказчик
                                val = dataTable.Rows[i][7].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "";
                                item.Customer = val;

                                // Код продукции
                                val = dataTable.Rows[i][8].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.ProductCode = int.Parse(val);

                                // Количество заготовок
                                val = dataTable.Rows[i][9].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.IngotsCount = int.Parse(val);

                                // Время входа в печь 
                                val = dataTable.Rows[i][10].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                item.TimeStart = DateTime.Parse(val);

                                // Время выхода из печи 
                                val = dataTable.Rows[i][11].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                item.TimeEnd = DateTime.Parse(val);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(
                                    $"Не удалось прочитать данные по плавкам в печи за период с {start:G} по {end:G} [{ex.Message}]");
                            }

                            result.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Не удалось получить данные по плавкам в печи за период с {start:G} по {end:G} [{ex.Message}]");
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
                                string val = dataTable.Rows[i][0].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.ShiftNumber = int.Parse(val);

                                val = dataTable.Rows[i][1].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                item.PeriodStart = DateTime.Parse(val);

                                val = dataTable.Rows[i][2].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                item.PeriodEnd = DateTime.Parse(val);

                                val = dataTable.Rows[i][3].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.CoilsCount = int.Parse(val);

                                val = dataTable.Rows[i][4].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.CoilsWeight = int.Parse(val);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(
                                    $"Не удалось прочитать сводные данные по сменам за период с {startPeriod:G} по {finishPeriod:G} [{ex.Message}]");
                            }

                            result.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Не удалось получить сводные данные по сменам за период с {startPeriod:G} по {finishPeriod:G} [{ex.Message}]");
            }

            return result;
        }
        
        /// <summary>
        /// Получить список простоев за период
        /// </summary>
        /// <param name="startTime">Начало периода</param>
        /// <param name="finishTime">Конец периода</param>
        /// <returns></returns>
        public List<DownTime> GetDowntimes(DateTime startTime, DateTime finishTime)
        {
            List<DownTime> result = new List<DownTime>();
            DataTable dataTable = new DataTable();
            string query = $"select * from public.f_get_downtimes('{startTime:O}', '{finishTime:O}');";
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
                            DownTime item = new DownTime();
                            try
                            {
                                string val = dataTable.Rows[i][0].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                item.TimeStart = DateTime.Parse(val);

                                val = dataTable.Rows[i][1].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                item.TimeFinish = DateTime.Parse(val);

                                val = dataTable.Rows[i][2].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "";
                                item.Comment = val;
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(
                                    $"Не удалось прочитать данные по простоям за период с {startTime:G} по {finishTime:G} [{ex.Message}]");
                            }
                            
                            result.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Не удалось получить данные по простоям за период с {startTime:G} по {finishTime:G} [{ex.Message}]");
            }

            return result;
        }

        /// <summary>
        /// Получение данных о соответствии прода и теста
        /// </summary>
        /// <param name="begin">Начало периода</param>
        /// <param name="end">Конец периода</param>
        /// <returns></returns>
        public List<CheckDtData> GetCheckDt(DateTime begin, DateTime end)
        {
            List<CheckDtData> result = new List<CheckDtData>();
            string query = _requests.GetCheckDt(begin, end);
            DataTable dataTable = new DataTable();
            
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionStringTest))
                {
                    connection.Open();
                    new NpgsqlDataAdapter(new NpgsqlCommand(query, connection)).Fill(dataTable);
                    connection.Close();
                    if (dataTable.Rows.Count > 0)
                    {
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            CheckDtData row = new CheckDtData();
                            try
                            {
                                string val = dataTable.Rows[i][0].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                row.LandingId = int.Parse(val);
                                
                                val = dataTable.Rows[i][1].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "";
                                row.CoilId = val;
                                
                                val = dataTable.Rows[i][2].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "";
                                row.IngotMes = val;
                                
                                val = dataTable.Rows[i][3].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                row.CoilWeightMes = int.Parse(val);
                                
                                val = dataTable.Rows[i][4].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                row.DateClose = DateTime.Parse(val);
                                
                                val = dataTable.Rows[i][5].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "";
                                row.IngotDt = val;
                                
                                val = dataTable.Rows[i][6].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                row.UnitDt = int.Parse(val);
                                
                                val = dataTable.Rows[i][7].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "";
                                row.IngotId = val;
                                
                                val = dataTable.Rows[i][8].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                row.CoilWeightDt = int.Parse(val);
                                
                                val = dataTable.Rows[i][9].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                row.CoilDateParam = DateTime.Parse(val);
                                
                                val = dataTable.Rows[i][10].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                row.TimeBegin = DateTime.Parse(val);
                                
                                val = dataTable.Rows[i][11].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                row.TimeEnd = DateTime.Parse(val);
                                
                                val = dataTable.Rows[i][12].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                row.BilletWeight = int.Parse(val);
                                
                                val = dataTable.Rows[i][13].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                row.BilletDate = DateTime.Parse(val);
                                
                                val = dataTable.Rows[i][14].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "";
                                row.IngotCompare = val;
                                
                                val = dataTable.Rows[i][15].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                row.Cutting = int.Parse(val) == 1;
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(
                                    $"Не удалось прочитать данные о соответствии прода и теста за период с {begin:G} по {end:G} [{ex.Message}]");
                            }
                            
                            result.Add(row);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Не удалось получить данные о соответствии прода и теста за период с {begin:G} по {end:G} [{ex.Message}]");
            }

            return result;
        }

        /// <summary>
        /// Получить список удаленных ЕУ за период
        /// </summary>
        /// <param name="begin">Начало периода</param>
        /// <param name="end">Конец периода</param>
        /// <returns>Список удаленных ЕУ за период</returns>
        public List<DeletedIngots> GetDeletedIngotsByPeriod(DateTime begin, DateTime end)
        {
            string query = _requests.GetDeletedByPeriod(begin, end);
            List<DeletedIngots> result = new List<DeletedIngots>();
            DataTable dataTable = new DataTable();

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionStringTest))
                {
                    connection.Open();
                    new NpgsqlDataAdapter(new NpgsqlCommand(query, connection)).Fill(dataTable);
                    connection.Close();
                    if (dataTable.Rows.Count > 0)
                    {
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            DeletedIngots row = new DeletedIngots();
                            try
                            {
                                string val = dataTable.Rows[i][0].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                row.NodeId = int.Parse(val);
                                
                                val = dataTable.Rows[i][1].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "";
                                row.NodeCode = val;
                                
                                val = dataTable.Rows[i][2].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "";
                                row.MeltNumber = val;
                                
                                val = dataTable.Rows[i][3].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                row.UnitId = int.Parse(val);
                                
                                val = dataTable.Rows[i][4].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                row.IngotId = int.Parse(val);
                                
                                val = dataTable.Rows[i][5].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                row.TimeBegin = DateTime.Parse(val);
                                
                                val = dataTable.Rows[i][6].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                row.TimeEnd = DateTime.Parse(val);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(
                                    $"Не удалось прочитать данные об удаленных ЕУ за период с {begin:G} по {end:G} [{ex.Message}]");
                            }
                            
                            result.Add(row);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Не удалось получить данные об удаленных ЕУ за период с {begin:G} по {end:G} [{ex.Message}]");
            }

            return result;
        }

        /// <summary>
        /// Получить список бурежек с группировкой по номеру плавки за период
        /// </summary>
        /// <param name="begin">Начало периода</param>
        /// <param name="end">Конец периода</param>
        /// <returns>Список бурежек за период</returns>
        public List<RejectionsData> GetRejectionsByPeriod(DateTime begin, DateTime end)
        {
            List<RejectionsData> result = new List<RejectionsData>();
            DataTable dataTable = new DataTable();
            string query = _requests.GetRejectionsByPeriod(begin, end);
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionStringTest))
                {
                    connection.Open();
                    new NpgsqlDataAdapter(new NpgsqlCommand(query, connection)).Fill(dataTable);
                    connection.Close();
                    if (dataTable.Rows.Count > 0)
                    {
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            RejectionsData item = new RejectionsData();
                            try
                            {
                                string val = dataTable.Rows[i][0].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "";
                                item.Melt = val;
                                
                                val = dataTable.Rows[i][1].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.IngotsCount = int.Parse(val);
                                
                                val = dataTable.Rows[i][2].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                item.TimeBegin = DateTime.Parse(val);
                                
                                val = dataTable.Rows[i][3].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                item.TimeEnd = DateTime.Parse(val);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(
                                    $"Не удалось прочитать список бурежек за период с [{begin:G}] по [{end:G}] [{ex.Message}]");
                            }
                            
                            result.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Не удалось получить список бурежек за период с [{begin:G}] по [{end:G}] [{ex.Message}]");
            }

            return result;
        }

        /// <summary>
        /// Получить количество возвратов по идентификатору посада
        /// </summary>
        /// <param name="meltId">Идентификатор посада</param>
        /// <returns>Количество возвратов</returns>
        public int GetReturnsCountByMeltId(string meltId)
        {
            int result = 0;
            string query = _requests.GetCountReturnsByMeltId(meltId);
            DataTable dataTable = new DataTable();
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionStringTest))
                {
                    connection.Open();
                    new NpgsqlDataAdapter(new NpgsqlCommand(query, connection)).Fill(dataTable);
                    connection.Close();
                    if (dataTable.Rows.Count > 0)
                    {
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            try
                            {
                                string val = dataTable.Rows[i][4].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                result = int.Parse(val);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(
                                    $"Не удалось прочитать количество возвратов по иденту плавки {meltId} [{ex.Message}]");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Не удалось получить количество возвратов по иденту плавки {meltId} [{ex.Message}]");
            }

            return result;
        }

        /// <summary>
        /// Получить список возвратов по готовому запросу
        /// </summary>
        /// <param name="query">Запрос</param>
        /// <returns>Список возвратов</returns>
        public List<ReturningData> GetReturns(string query)
        {
            List<ReturningData> result = new List<ReturningData>();
            DataTable dataTable = new DataTable();
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionStringTest))
                {
                    connection.Open();
                    new NpgsqlDataAdapter(new NpgsqlCommand(query, connection)).Fill(dataTable);
                    connection.Close();
                    if (dataTable.Rows.Count > 0)
                    {
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            ReturningData item = new ReturningData();
                            try
                            {
                                string val = dataTable.Rows[i][0].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.Melt = val;

                                val = dataTable.Rows[i][1].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                item.TimeBegin = DateTime.Parse(val);

                                val = dataTable.Rows[i][2].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                item.TimeEnd = DateTime.Parse(val);

                                val = dataTable.Rows[i][3].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.IngotNumber = int.Parse(val);

                                val = dataTable.Rows[i][4].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.IngotsCount = int.Parse(val);

                                val = dataTable.Rows[i][5].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = DateTime.MinValue.ToString("G");
                                item.TimeCreateLanding = DateTime.Parse(val);

                                val = dataTable.Rows[i][6].ToString()?.Trim();
                                if (string.IsNullOrEmpty(val))
                                    val = "0";
                                item.IngotWeight = int.Parse(val);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(
                                    $"Не удалось прочитать список возвратов по готовому запросу [{ex.Message}]");
                            }

                            result.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Не удалось получить список возвратов по готовому запросу [{ex.Message}]");
            }

            return result;
        }

    }
}