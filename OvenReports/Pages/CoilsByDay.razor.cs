using System;
using System.Collections.Generic;
using System.Diagnostics;
using OvenReports.Data;
using NLog;

namespace OvenReports.Pages
{
    public partial class CoilsByDay
    {
        private struct MeltInfo
        {
            public DateTime StartPeriod;
            public DateTime FinishPeriod;
        }
        
        private readonly MeltsForPeriod _meltsPeriod = new MeltsForPeriod();
        private List<MeltsForPeriod> _meltsList = new List<MeltsForPeriod>();
        private readonly DBConnection _db = new DBConnection();
        private List<CoilData> _selectedMelt = new List<CoilData>();
        private string _showReport = "none";
        private MeltInfo _meltInfo;
        private Logger _logger;
        
        protected override void OnInitialized()
        {
            _logger = LogManager.GetCurrentClassLogger();
            Initialize();
        }

        private void Initialize()
        {
            // _meltsPeriod.PeriodFinish = DateTime.Now; // GetCurrentTime();
            // _meltsPeriod.PeriodStart = _shift.GetShiftStart(_meltsPeriod.PeriodFinish);
        }

        /// <summary>
        /// Получить текущее время с точностью до минут
        /// </summary>
        /// <returns></returns>
        private DateTime GetCurrentTime()
        {
            DateTime now = DateTime.Now;
            // DateTime now = DateTime.Parse("01-01-2020 21:37:21");
            string currentTime = $"{now.Day}-{now.Month}-{now.Year} {now.Hour}:00:00";

            DateTime result = DateTime.Parse(currentTime);
            return result;
        }

        private List<MeltsForPeriod> GetMelts(DateTime start, DateTime finish)
        {
            List<MeltsForPeriod> result = new List<MeltsForPeriod>();
            List<MeltsForPeriod> sorted = new List<MeltsForPeriod>();
            List<MeltsForPeriod> meltsList = new List<MeltsForPeriod>();
            try
            {
                meltsList = _db.GetHourlyCoilsByPeriod(start, finish);
            }
            catch (Exception ex)
            {
                _logger.Error($"Не удалось получить список плавок за период с [{_meltsPeriod.PeriodStart}] по [{_meltsPeriod.PeriodFinish}] [{ex.Message}]");
            }
            
            // Заполнение недостающих часов
            DateTime prevDate = new DateTime();
            int prevHour = 0;
            
            foreach (MeltsForPeriod hour in meltsList)
            {
                if (hour.WeightingData != prevDate)
                {
                    // Новый день
                    
                    // Если предыдущий час не равен 23, значит предыдущий день не закончен
                    if (prevHour<23 && prevDate!=DateTime.MinValue)
                    {
                        for (int i = prevHour+1; i <= 23; i++)
                        {
                            MeltsForPeriod tmp = new MeltsForPeriod
                            {
                                WeightingData = prevDate,
                                WeightingHourStart = i,
                                WeightingHourFinish = i + 1
                            };
                            sorted.Add(tmp);
                        }
                    }
                        
                    prevHour = 0;
                    prevDate = hour.WeightingData;
                    int diff = hour.WeightingHourStart - prevHour;
                    if (diff > 1)
                    {
                        // есть пропуск в часах
                        for (int i = 0; i<diff; i++)
                        {
                            MeltsForPeriod tmp = new MeltsForPeriod
                            {
                                WeightingData = hour.WeightingData,
                                WeightingHourStart = i,
                                WeightingHourFinish = i + 1
                            };
                            sorted.Add(tmp);
                        }
                        
                        sorted.Add(hour);
                        prevHour = hour.WeightingHourStart;
                    }
                    else
                    {
                        // час на своем месте
                        prevHour = hour.WeightingHourStart;
                        sorted.Add(hour);
                    }
                }
                else
                {
                    // Новый час того же дня
                    int diff = hour.WeightingHourStart - prevHour;
                    if (diff > 1)
                    {
                        // есть пропуск в часах
                        for (int i = prevHour+1; i < hour.WeightingHourStart; i++)
                        {
                            MeltsForPeriod tmp = new MeltsForPeriod
                            {
                                WeightingData = hour.WeightingData,
                                WeightingHourStart = i,
                                WeightingHourFinish = i + 1
                            };
                            sorted.Add(tmp);
                        }

                        sorted.Add(hour);
                        prevHour = hour.WeightingHourStart;
                    }
                    else
                    {
                        // час на своем месте
                        prevHour = hour.WeightingHourStart;
                        sorted.Add(hour);
                    }
                }
            }
                
            // Если конечный период не текущая дата, то заполняем часы до 24
            DateTime now = DateTime.Now;
            // DateTime lastDate = sorted[^1].WeightingData;
            // int lastHours = sorted[^1].WeightingData.Hour;
            
            if (now.Date != prevDate.Date)
            {
                if (prevHour < 24)
                {
                    for (int i = prevHour+1; i < 24; i++)
                    {
                        MeltsForPeriod tmp = new MeltsForPeriod
                        {
                            WeightingData = prevDate,
                            WeightingHourStart = i,
                            WeightingHourFinish = i + 1
                        };
                        sorted.Add(tmp);
                    }
                }
            }

            int totalCoilsCount = 0;
            int totalWeightFact = 0;
            DateTime weightDate = new DateTime();
            bool lastHour = false;
            
            foreach (MeltsForPeriod melt in sorted)
            {
                if (weightDate == DateTime.MinValue)
                {
                    // Это первая строка
                    weightDate = melt.WeightingData;
                    melt.TotalCoilsCount = melt.CoilsCount;
                    melt.TotalWeightFact = melt.WeightFact;
                    
                    totalCoilsCount += melt.CoilsCount;
                    totalWeightFact += melt.WeightFact;
                }
                else
                {
                    if (melt.WeightingData != weightDate)
                    {
                        totalCoilsCount = melt.CoilsCount;
                        totalWeightFact = melt.WeightFact;
                        
                        melt.TotalCoilsCount = melt.CoilsCount;
                        melt.TotalWeightFact = melt.WeightFact;
                        
                        weightDate = melt.WeightingData;
                        lastHour = true;
                    }
                    else
                    {
                        totalCoilsCount += melt.CoilsCount;
                        totalWeightFact += melt.WeightFact;
                        
                        melt.TotalCoilsCount = totalCoilsCount;
                        melt.TotalWeightFact = totalWeightFact;
                    }
                }
                if(lastHour)
                    result.Add(new MeltsForPeriod());
                lastHour = false;

                result.Add(melt);
            }
            
            return result;
        }
        

        private void GetMeltsList()
        {
            string start =
                $"{_meltsPeriod.PeriodStart.Day}-{_meltsPeriod.PeriodStart.Month}-{_meltsPeriod.PeriodStart.Year} 00:00:00.000";
            string finish =
                $"{_meltsPeriod.PeriodFinish.Day}-{_meltsPeriod.PeriodFinish.Month}-{_meltsPeriod.PeriodFinish.Year} 23:59:59.999";
            
            _meltsList = GetMelts(DateTime.Parse(start), DateTime.Parse(finish));
            _showReport = "block";
            StateHasChanged();
        }

        private void PrepareCoils(DateTime date, int hour)
        {
            _selectedMelt = new List<CoilData>();
            string startTime = $"{date.Day}-{date.Month}-{date.Year} {hour}:00:00";
            
            DateTime start = DateTime.Parse(startTime);
            DateTime finish = start.AddHours(1);

            _meltInfo.StartPeriod = start;
            _meltInfo.FinishPeriod = finish;
            string melt = "";
            double diam = 0;
            
            List<CoilData> coils = _db.GetHourlyCoilsByPeriodDetail(start, finish);
            foreach (CoilData coil in coils)
            {
                if (melt != coil.MeltNumber || Math.Abs(diam - coil.Diameter) > 0.1)
                {
                    if (melt != "" && diam != 0)
                    {
                        _selectedMelt.Add(new CoilData());
                    }
                    melt = coil.MeltNumber;
                    diam = coil.Diameter;
                }
                _selectedMelt.Add(coil);
            }
        }

        private void GetCurrentDay()
        {
            DateTime now = DateTime.Now;
            string todayStart = $"{now.Day}-{now.Month}-{now.Year} 00:00:00.000";
            string todayFinish = $"{now.Day}-{now.Month}-{now.Year} 23:59:59.999";
            
            _meltsList = GetMelts(DateTime.Parse(todayStart), DateTime.Parse(todayFinish));
            
            _showReport = "block";
            StateHasChanged();
        }

        private void GetPrevDay()
        {
            DateTime yesterday = DateTime.Now.AddDays(-1);
            string yesterdayStart = $"{yesterday.Day}-{yesterday.Month}-{yesterday.Year} 00:00:00.000";
            string yesterdayFinish = $"{yesterday.Day}-{yesterday.Month}-{yesterday.Year} 23:59:59.999";
            
            _meltsList = GetMelts(DateTime.Parse(yesterdayStart), DateTime.Parse(yesterdayFinish));
            
            _showReport = "block";
            StateHasChanged();
        }
    }
}