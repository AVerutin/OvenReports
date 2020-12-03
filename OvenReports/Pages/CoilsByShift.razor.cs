using System;
using System.Collections.Generic;
using OvenReports.Data;
using NLog;

namespace OvenReports.Pages
{
    public partial class CoilsByShift
    {
        private struct MeltInfo
        {
            public string MeltNumber;
            public double Diameter;
            public DateTime StartPeriod;
            public DateTime FinishPeriod;
        }
        
        private MeltsForPeriod _meltsPeriod = new MeltsForPeriod();
        private List<MeltsForPeriod> _meltsList = new List<MeltsForPeriod>();
        private List<CoilData> _selectedMelt = new List<CoilData>();
        private readonly DBConnection _db = new DBConnection();
        private string _showReport = "none";
        private MeltInfo _meltInfo;
        private Logger _logger;
        
        
        // private List<ShiftReport> _reportList = new List<ShiftReport>();
        // private Shift _shift = new Shift();
        // private List<CoilData> _selectedMelt = new List<CoilData>();
        
        protected override void OnInitialized()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info("Запущен отчет по бунтам");
            Initialize();
        }

        private void Initialize()
        {
            // _meltsPeriod.PeriodFinish = DateTime.Now; // GetCurrentTime();
            // _meltsPeriod.PeriodStart = _shift.GetShiftStart(_meltsPeriod.PeriodFinish);
        }

        private void GetReportByPeriod()
        {
            DateTime rangeStart = _meltsPeriod.PeriodStart.AddDays(-1);
            DateTime rangeFinish = _meltsPeriod.PeriodFinish;
            string timeStart = $"{rangeStart.Day}-{rangeStart.Month}-{rangeStart.Year} 20:00:00.000";
            string timeFinish = $"{rangeFinish.Day}-{rangeFinish.Month}-{rangeFinish.Year} 20:00:00.000";
            DateTime periodStart = DateTime.Parse(timeStart);
            DateTime periodFinish = DateTime.Parse(timeFinish);
            
            Shift shift = new Shift();
            ShiftData shiftData = shift.GetShiftByDate(periodStart);
            _meltsList = new List<MeltsForPeriod>();

            while (shiftData.FinishTime <= periodFinish)
            {
                List<MeltsForPeriod> melts = GetMelts(shiftData.StartTime, shiftData.FinishTime);

                foreach (MeltsForPeriod melt in melts)
                {
                    _meltsList.Add(melt);
                }

                if (shiftData.FinishTime != periodFinish && melts.Count > 0)
                    _meltsList.Add(new MeltsForPeriod());

                shiftData = shift.GetShiftByDate(shiftData.StartTime.AddHours(12));
            }

            _showReport = "block";
            StateHasChanged();
        }

        private void GetReportCurrentShift()
        {
            Shift shift = new Shift();
            ShiftData currentShift = shift.GetCurrentShift();

            _meltsList = GetMelts(currentShift.StartTime, currentShift.FinishTime);
            _showReport = "block";
            StateHasChanged();
        }

        private void GetReportByPreviousShift()
        {
            Shift shift = new Shift();
            ShiftData previousShift = shift.GetPreviousShift();

            _meltsList = GetMelts(previousShift.StartTime, previousShift.FinishTime);
            _showReport = "block";
            StateHasChanged();
        }

        private void PrepareCoils(DateTime date, int hour)
        {
            _selectedMelt = new List<CoilData>();
            string startTime = $"{date.Day}-{date.Month}-{date.Year} {hour}:00:00.000";
            // string finishTime = $"{date.Day}-{date.Month}-{date.Year} {hour}:59:59.999";
            
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

            if(meltsList.Count>0)
            {
                // Заполнение недостающих часов
                int shiftNumber = 0;
                if (start.Hour == 8)
                {
                    // Дневная смена
                    int startHour = 7;
                    DateTime date = new DateTime();
                    foreach (MeltsForPeriod item in meltsList)
                    {
                        if (shiftNumber == 0)
                            shiftNumber = item.ShiftNumber;

                        int diff = item.WeightingHourStart - startHour;
                        if (diff > 1)
                        {
                            for (int i = startHour + 1; i < item.WeightingHourStart; i++)
                            {
                                MeltsForPeriod tmp = new MeltsForPeriod
                                {
                                    WeightingData = DateTime.Parse(
                                        $"{item.WeightingData.Day}-{item.WeightingData.Month}-{item.WeightingData.Year} 08:00"),
                                    WeightingHourStart = i,
                                    WeightingHourFinish = i + 1,
                                    ShiftNumber = shiftNumber == 0 ? item.ShiftNumber : shiftNumber
                                };
                                sorted.Add(tmp);
                            }
                        }

                        sorted.Add(item);
                        startHour = item.WeightingHourStart;
                        shiftNumber = item.ShiftNumber;
                        date = item.WeightingData;
                    }

                    if (startHour < 19 && date.Date != DateTime.Now.Date)
                    {
                        for (int i = startHour + 1; i <= 19; i++)
                        {
                            MeltsForPeriod tmp = new MeltsForPeriod
                            {
                                WeightingData = DateTime.Parse(
                                    $"{date.Day}-{date.Month}-{date.Year} 08:00"),
                                WeightingHourStart = i,
                                WeightingHourFinish = i + 1,
                                ShiftNumber = shiftNumber
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
                        if (shiftNumber == 0)
                            shiftNumber = item.ShiftNumber;

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
                                        WeightingHourFinish = i + 1 == 24 ? 0 : i + 1,
                                        ShiftNumber = shiftNumber == 0 ? item.ShiftNumber : shiftNumber
                                    };
                                    sorted.Add(tmp);
                                }
                            }

                            sorted.Add(item);
                            startHour = item.WeightingHourStart;
                            shiftNumber = item.ShiftNumber;
                            date = item.WeightingData;
                        }
                        else
                        {
                            // Вторая половина смены
                            if (startHour >= 19)
                            {
                                for (int i = startHour + 1; i < 24; i++)
                                {
                                    DateTime yesterday = item.WeightingData.AddDays(-1);
                                    MeltsForPeriod tmp = new MeltsForPeriod
                                    {
                                        WeightingData = DateTime.Parse(
                                            $"{yesterday.Day}-{yesterday.Month}-{yesterday.Year} 20:00"),
                                        WeightingHourStart = i,
                                        WeightingHourFinish = i + 1 == 24 ? 0 : i + 1,
                                        ShiftNumber = shiftNumber
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
                                        WeightingHourFinish = i + 1 == 24 ? 0 : i + 1,
                                        ShiftNumber = shiftNumber == 0 ? item.ShiftNumber : shiftNumber
                                    };
                                    sorted.Add(tmp);
                                }
                            }

                            sorted.Add(item);

                            startHour = item.WeightingHourStart;
                            shiftNumber = item.ShiftNumber;
                            date = item.WeightingData;
                        }
                    }

                    if (startHour >= 20)
                    {
                        for (int i = startHour + 1; i < 24; i++)
                        {
                            MeltsForPeriod tmp = new MeltsForPeriod
                            {
                                WeightingData = DateTime.Parse(
                                    $"{date.Day}-{date.Month}-{date.Year} 20:00"),
                                WeightingHourStart = i,
                                WeightingHourFinish = i + 1 == 24 ? 0 : i + 1,
                                ShiftNumber = shiftNumber
                            };
                            sorted.Add(tmp);
                        }

                        startHour = -1;
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
                                WeightingHourFinish = i + 1 == 24 ? 0 : i + 1,
                                ShiftNumber = shiftNumber
                            };
                            sorted.Add(tmp);
                        }
                    }
                }

                int totalCoilsCount = 0;
                int totalWeightFact = 0;

                foreach (MeltsForPeriod melt in sorted)
                {
                    if (melt.ShiftNumber > 0)
                    {
                        totalCoilsCount += melt.CoilsCount;
                        totalWeightFact += melt.WeightFact;

                        melt.TotalCoilsCount = totalCoilsCount;
                        melt.TotalWeightFact = totalWeightFact;

                        if (melt.WeightingHourFinish == 24)
                            melt.WeightingHourFinish = 0;
                    }
                    else
                    {
                        totalCoilsCount = 0;
                        totalWeightFact = 0;
                    }

                    result.Add(melt);
                }
            }
            return result;
        }
    }
}