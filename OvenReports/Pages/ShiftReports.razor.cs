using System;
using System.Collections.Generic;
using OvenReports.Data;
using NLog;

namespace OvenReports.Pages
{
    public partial class ShiftReports
    {
        private struct MeltInfo
        {
            public DateTime StartDate;
            public DateTime FinishDate;
        }
        
        private readonly MeltsForPeriod _meltsPeriod = new MeltsForPeriod();
        private List<ShiftReport> _reportList = new List<ShiftReport>();
        private readonly DBConnection _db = new DBConnection();
        private readonly Shift _shift = new Shift();
        private List<MeltsForPeriod> _meltsList = new List<MeltsForPeriod>();
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

        private void GetReportByPeriod()
        {
            _reportList = new List<ShiftReport>();

            DateTime rangeStart = _meltsPeriod.PeriodStart.AddDays(-1);
            DateTime rangeFinish = _meltsPeriod.PeriodFinish;
            string timeStart = $"{rangeStart.Day}-{rangeStart.Month}-{rangeStart.Year} 20:00:00.000";
            string timeFinish = $"{rangeFinish.Day}-{rangeFinish.Month}-{rangeFinish.Year} 20:00:00.000";
            DateTime periodStart = DateTime.Parse(timeStart);
            DateTime periodFinish = DateTime.Parse(timeFinish);

            GetShiftsReport(periodStart, periodFinish);

            _showReport = "block";
            StateHasChanged();
        }

        /// <summary>
        /// Формирование отчета за текущую смены
        /// </summary>
        private void GetReportByCurrentShift()
        {
            ShiftData current = _shift.GetCurrentShift();
            _meltInfo.StartDate = current.StartTime;
            _meltInfo.FinishDate = current.FinishTime;
            _reportList = new List<ShiftReport>();
            
            GetShiftsReport(current.StartTime, current.FinishTime);
            
            // try
            // {
            //     _reportList = _db.GetShiftReport(current.StartTime, current.FinishTime);
            // }
            // catch (Exception ex)
            // {
            //     _logger.Error($"Не удалось получить список плавок за текущую смену №{current.Number} с [{current.StartTime}] по [{current.FinishTime}] [{ex.Message}]");
            // }
            
            _showReport = "block";
            StateHasChanged();
        }

        /// <summary>
        /// Формирование отчета за предыдущую смену
        /// </summary>
        private void GetReportByPreviousShift()
        {
            ShiftData previous = _shift.GetPreviousShift();
            _meltInfo.StartDate = previous.StartTime;
            _meltInfo.FinishDate = previous.FinishTime;
            _reportList = new List<ShiftReport>();
            
            GetShiftsReport(previous.StartTime, previous.FinishTime);
            
            // try
            // {
            //     _reportList = _db.GetShiftReport(previous.StartTime, previous.FinishTime);
            // }
            // catch (Exception ex)
            // {
            //     _logger.Error($"Не удалось получить список плавок за период с [{previous.StartTime}] по [{previous.FinishTime}] [{ex.Message}]");
            // }
            
            _showReport = "block";
            StateHasChanged();
        }

        private void GetShiftsReport(DateTime periodStart, DateTime periodFinish)
        {
            List<ShiftReport> result;
            
            try
            {
                result = _db.GetShiftReport(periodStart, periodFinish);
            }
            catch (Exception ex)
            {
                result = new List<ShiftReport>();
                _logger.Error(
                    $"Не удалось получить список плавок за период с [{_meltsPeriod.PeriodStart}] по [{_meltsPeriod.PeriodFinish}] [{ex.Message}]");
            }

            // Заполнение корректного времени начала и окончания смен
            int shiftNumber = (int) (periodStart - DateTime.Parse("01-01-2020 08:00:00")).TotalHours / 12 - 1;
            int maxShiftNumber = (int) (periodFinish - DateTime.Parse("01-01-2020 08:00:00")).TotalHours / 12 - 1;

            foreach (ShiftReport item in result)
            {
                int diff = item.ShiftNumber - shiftNumber;
                ShiftData shiftData;
                ShiftReport shift;

                if (diff > 1)
                {
                    // Для всех пропущенных смен
                    DateTime shiftTime = item.PeriodStart.AddHours(-12 * (diff - 1));
                    for (int i = 1; i < diff; i++)
                    {
                        shiftData = _shift.GetShiftByDate(shiftTime);
                        shift = new ShiftReport
                        {
                            ShiftNumber = item.ShiftNumber - diff + i,
                            PeriodStart = shiftData.StartTime,
                            PeriodEnd = shiftData.FinishTime,
                            CoilsCount = 0,
                            CoilsWeight = 0
                        };

                        _reportList.Add(shift);
                        shiftTime = shiftTime.AddHours(12);
                    }
                }

                shiftData = _shift.GetShiftByDate(item.PeriodStart);
                shift = new ShiftReport
                {
                    ShiftNumber = item.ShiftNumber,
                    PeriodStart = shiftData.StartTime,
                    PeriodEnd = shiftData.FinishTime,
                    CoilsCount = item.CoilsCount,
                    CoilsWeight = item.CoilsWeight
                };

                _reportList.Add(shift);
                shiftNumber = item.ShiftNumber;
            }

            int difference = maxShiftNumber - shiftNumber;
            if (difference > 0)
            {
                DateTime shiftTime = periodFinish.AddHours(-12 * (difference - 1));
                for (int i = 1; i < difference; i++)
                {
                    ShiftData shiftData = _shift.GetShiftByDate(shiftTime);
                    ShiftReport shift = new ShiftReport
                    {
                        ShiftNumber = shiftNumber - difference + i,
                        PeriodStart = shiftData.StartTime,
                        PeriodEnd = shiftData.FinishTime,
                        CoilsCount = 0,
                        CoilsWeight = 0
                    };

                    _reportList.Add(shift);
                    shiftTime = shiftTime.AddHours(12);
                    shiftNumber = shift.ShiftNumber;
                }
            }

            int totalCoils = 0;
            int totalWeight = 0;
            foreach (ShiftReport item in _reportList)
            {
                totalCoils += item.CoilsCount;
                totalWeight += item.CoilsWeight;
            
                item.TotalCoils = totalCoils;
                item.TotalWeight = totalWeight;
            }
        }

        /// <summary>
        /// Подготовка данных по часам за выбранную смену 
        /// </summary>
        /// <param name="startPeriod">Начало смены</param>
        /// <param name="finishPeriod">Окончание смены</param>
        private void PrepareCoils(DateTime startPeriod, DateTime finishPeriod)
        {
            _meltInfo.StartDate = startPeriod;
            _meltInfo.FinishDate = finishPeriod;
            _meltsList = GetMelts(startPeriod, finishPeriod);
            _showReport = "block";
            StateHasChanged();
        }

        /// <summary>
        /// Рассчет данных по часам для выбранной смены
        /// </summary>
        /// <param name="start">Начало смены</param>
        /// <param name="finish">Окончание смены</param>
        /// <returns></returns>
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
                _logger.Error(
                    $"Не удалось получить список плавок за период с [{_meltsPeriod.PeriodStart}] по [{_meltsPeriod.PeriodFinish}] [{ex.Message}]");
            }

            // Заполнение недостающих часов
            if (start.Hour == 8)
            {
                // Дневная смена
                int startHour = 7;
                DateTime date = new DateTime();
                foreach (MeltsForPeriod item in meltsList)
                {
                    int diff = item.WeightingHourStart - startHour;
                    if (diff > 1)
                    {
                        for (int i = startHour+1; i < item.WeightingHourStart; i++)
                        {
                            MeltsForPeriod tmp = new MeltsForPeriod
                            {
                                WeightingData = DateTime.Parse(
                                    $"{item.WeightingData.Day}-{item.WeightingData.Month}-{item.WeightingData.Year} 08:00"),
                                WeightingHourStart = i,
                                WeightingHourFinish = i + 1
                            };
                            sorted.Add(tmp);
                        }
                    }
                    sorted.Add(item);
                    startHour = item.WeightingHourStart;
                    date = item.WeightingData;
                }

                if (startHour < 19 && date.Date != DateTime.Now.Date)
                {
                    for (int i = startHour+1; i <= 19; i++)
                    {
                        MeltsForPeriod tmp = new MeltsForPeriod
                        {
                            WeightingData = DateTime.Parse(
                                $"{date.Day}-{date.Month}-{date.Year} 08:00"),
                            WeightingHourStart = i,
                            WeightingHourFinish = i + 1
                        };
                        sorted.Add(tmp);
                    }
                }
            }
            else
            {
                // Ночная смена
                int startHour = 19;
                DateTime date = new DateTime();
                
                foreach (MeltsForPeriod item in meltsList)
                {
                    if (item.WeightingHourStart >= 20)
                    {
                        // Первая половина смены
                        int diff = item.WeightingHourStart - startHour;
                        if (diff > 1)
                        {
                            for (int i = startHour + 1; i < item.WeightingHourStart; i++)
                            {
                                MeltsForPeriod tmp = new MeltsForPeriod
                                {
                                    WeightingData = DateTime.Parse(
                                        $"{item.WeightingData.Day}-{item.WeightingData.Month}-{item.WeightingData.Year} 20:00"),
                                    WeightingHourStart = i,
                                    WeightingHourFinish = i + 1 == 24 ? 0 : i + 1
                                };
                                sorted.Add(tmp);
                            }
                        }

                        sorted.Add(item);
                        startHour = item.WeightingHourStart;
                        date = item.WeightingData;
                    }
                    else
                    {
                        // Вторая половина смены
                        if (startHour >= 19)
                        {
                            for (int i = startHour+1; i < 24; i++)
                            {
                                DateTime yesterday = item.WeightingData.AddDays(-1); 
                                MeltsForPeriod tmp = new MeltsForPeriod
                                {
                                    WeightingData = DateTime.Parse(
                                        $"{yesterday.Day}-{yesterday.Month}-{yesterday.Year} 20:00"),
                                    WeightingHourStart = i,
                                    WeightingHourFinish = i + 1 == 24 ? 0 : i + 1
                                };
                                sorted.Add(tmp);
                            }

                            startHour = -1;
                        }
                        else
                        {
                            if (startHour > 7)
                            {
                                startHour = -1;
                            }
                        }

                        int diff = item.WeightingHourStart - startHour;
                        if (diff > 1)
                        {
                            for (int i = startHour + 1; i < item.WeightingHourStart; i++)
                            {
                                MeltsForPeriod tmp = new MeltsForPeriod
                                {
                                    WeightingData = DateTime.Parse(
                                        $"{item.WeightingData.Day}-{item.WeightingData.Month}-{item.WeightingData.Year} 20:00"),
                                    WeightingHourStart = i,
                                    WeightingHourFinish = i + 1 == 24 ? 0 : i + 1
                                };
                                sorted.Add(tmp);
                            }
                        }

                        sorted.Add(item);

                        startHour = item.WeightingHourStart;
                        date = item.WeightingData;
                    }
                }

                if (startHour < 7)
                {
                    for (int i = startHour + 1; i <= 7; i++)
                    {
                        MeltsForPeriod tmp = new MeltsForPeriod
                        {
                            WeightingData = DateTime.Parse(
                                $"{date.Day}-{date.Month}-{date.Year} 20:00"),
                            WeightingHourStart = i,
                            WeightingHourFinish = i + 1 == 24 ? 0 : i + 1
                        };
                        sorted.Add(tmp);
                    }
                }
            }

            int totalCoilsCount = 0;
            int totalWeightFact = 0;

            foreach (MeltsForPeriod melt in sorted)
            {
                totalCoilsCount += melt.CoilsCount;
                totalWeightFact += melt.WeightFact;

                melt.TotalCoilsCount = totalCoilsCount;
                melt.TotalWeightFact = totalWeightFact;

                if (melt.WeightingHourFinish == 24)
                    melt.WeightingHourFinish = 0;
                
                result.Add(melt);
            }

            return result;
        }
    }
}