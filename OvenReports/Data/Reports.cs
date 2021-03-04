using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using NLog;

namespace OvenReports.Data
{
    public class Reports
    {
        private readonly Logger _logger;
        private readonly DbConnection _db;
        private readonly Shift _shift;
        private readonly QueryRequests _requests;
        
        public Reports()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _db = new DbConnection();
            _shift = new Shift();
            _requests = new QueryRequests();
        }

        /// <summary>
        /// Получить данные о плавке по ее номеру
        /// </summary>
        /// <param name="meltNumber">Номер плавки</param>
        /// <returns>Данные о плавке</returns>
        public List<LandingData> GetMelt(string meltNumber)
        {
            List<LandingData> result = new List<LandingData>();
            List<LandingData> meltsList = new List<LandingData>();
                
            try
            {
                result = _db.GetMeltByNumber(meltNumber);
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Не удалось получить плавки №{meltNumber} [{ex.Message}]");
            }

            foreach (LandingData melt in result)
            {
                if(melt.Weighted>0)
                    meltsList.Add(melt);
            }

            foreach (LandingData melt in result)
            {
                if(melt.Weighted>0)
                {
                    try
                    {
                        melt.CoilList = _db.GetCoilsByMelt(melt.MeltNumber, melt.Diameter, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(
                            $"Не удалось получить список бунтов для плавки №{melt.MeltNumber}, диаметр {melt.Diameter} [{ex.Message}]");
                    }

                    melt.FirstWeighting = melt.CoilList[0].DateWeight;
                    melt.LastWeighting = melt.CoilList[^1].DateWeight;

                    foreach (CoilData coil in melt.CoilList)
                    {
                        melt.WeightReal += coil.WeightFact;
                    }
                }
            }

            return meltsList;
        }

        /// <summary>
        /// Получить список плавок за период
        /// </summary>
        /// <param name="start">Начало периода</param>
        /// <param name="finish">Конец периода</param>
        /// <param name="orderType">Тип сортировки результата</param>
        /// <returns>Список плавок за период</returns>
        public List<LandingData> GetMelts(DateTime start, DateTime finish, OrderTypes orderType)
        {
            List<LandingData> meltsList = new List<LandingData>();
            List<MeltsList> melts = new List<MeltsList>();
            
            try
            {
                melts = _db.GetMeltsListSummary(start, finish, orderType);
                
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Не удалось получить список плавок за период с [{start}] по [{finish}] [{ex.Message}]");
            }

            foreach (MeltsList melt in melts)
            {
                LandingData item = new LandingData
                {
                    LandingId = melt.MeltId,
                    MeltNumber = melt.MeltNumber,
                    ProductProfile = melt.ProductProfile,
                    Diameter = melt.Diameter,
                    FirstWeighting = melt.PeriodStart,
                    LastWeighting = melt.PeriodFinish,
                    SteelMark = melt.SteelMark,
                    IngotProfile = melt.IngotProfile,
                    IngotsCount = melt.IngotsCount,
                    IngotLength = melt.IngotLength,
                    Standart = melt.Standart,
                    ProductCode = melt.ProductCode,
                    Customer = melt.Customer,
                    Weighted = melt.CoilsCount,
                    WeightReal = melt.TotalWeight
                };

                meltsList.Add(item);
            }

            return meltsList;
        }

        /// <summary>
        /// Получить список простоев за период в разреде суток
        /// </summary>
        /// <param name="startPeriod">Начало периода</param>
        /// <param name="finishPeriod">Конец периода</param>
        /// <returns>Список простоев</returns>
        public List<DownTime> GetDowntimesByDay(DateTime startPeriod, DateTime finishPeriod)
        {
            string dateStart = $"{startPeriod.Day}-{startPeriod.Month}-{startPeriod.Year} 00:00:00.000";
            DateTime start = DateTime.Parse(dateStart);
            string dateFinish = $"{finishPeriod.Day}-{finishPeriod.Month}-{finishPeriod.Year} 23:59:59.999";
            DateTime finish = DateTime.Parse(dateFinish);

            List<DownTime> dbResult = _db.GetDowntimes(start, finish);
            List<DownTime> result = new List<DownTime>();
            
            // Добавляем разделители между сутками
            DateTime prevDate = DateTime.MinValue;
            foreach (DownTime item in dbResult)
            {
                if (prevDate == DateTime.MinValue)
                {
                    prevDate = item.TimeStart;
                    result.Add(item);
                }
                else
                {
                    if (item.TimeStart.Date != prevDate.Date)
                    {
                        result.Add(new DownTime());
                    }
                    
                    //TODO: Добавить разделение простоев по границам суток

                    prevDate = item.TimeStart;
                    result.Add(item);
                }
            }

            // Расчитываем итоговые значения по суткам
            DateTime lastDowntime = DateTime.MinValue;
            TimeSpan totalDuration = new TimeSpan();
            
            foreach (DownTime item in result)
            {
                if (item.TimeStart == DateTime.MinValue)
                {
                    lastDowntime = DateTime.MinValue;
                    totalDuration = new TimeSpan();
                    continue;
                }
                
                if (lastDowntime == DateTime.MinValue)
                {
                    lastDowntime = item.TimeStart;
                }

                if (item.TimeFinish != DateTime.MinValue)
                {
                    item.TimeDuration = item.TimeFinish.Subtract(item.TimeStart);
                    totalDuration += item.TimeDuration;
                }
                else
                {
                    item.TimeDuration = DateTime.Now.Subtract(item.TimeStart);
                    totalDuration += item.TimeDuration;
                }
                
                item.DurationTotal = totalDuration;
            }

            return result;
        }

        /// <summary>
        /// Получить список простоев за период в разрезе смен
        /// </summary>
        /// <param name="startPeriod">Начало периода</param>
        /// <param name="finishPeriod">Конец периода</param>
        /// <returns></returns>
        public List<DownTime> GetDowntimesByShift(DateTime startPeriod, DateTime finishPeriod)
        {
            List<DownTime> dbResult = _db.GetDowntimes(startPeriod, finishPeriod);
            List<DownTime> result = new List<DownTime>();
            
            // Добавляем разделители между сменами
            ShiftData prevShift = new ShiftData();

            foreach (DownTime item in dbResult)
            {
                // Если нет времени начала предыдущей смены, получим время начала текущей смены 
                if (prevShift.StartTime == DateTime.MinValue)
                {
                    prevShift = _shift.GetShiftByDate(item.TimeStart);
                    result.Add(item);
                }
                else
                {
                    if (Shift.GetShiftStart(item.TimeStart) != prevShift.StartTime)
                    {
                        // Простой начался не в предыдущей смене, добавим разделитель смен
                        result.Add(new DownTime());
                    }
                    
                    //TODO: Добавить разделение простоев по границам смен
                    // Можно добавить разделение простоя по границам конца смены
                    // Добавить к текущей смене время до окончания смены
                    // После добавления разделителя добавить оставшееся время простоя
                    
                    // Возможна ситуация, когда простой начался в одной смене, а закончился через несколько смен
                    // В таком случае нужно найти разницу между номерами смен
                    // Если разница больше 1, добавить пустую смену с простоем равным продолжительности простоя
                    
                    
                    // Добавить простой в результирующую таблицу
                    prevShift.StartTime = Shift.GetShiftStart(item.TimeStart);
                    result.Add(item);
                }
            }
            
            // Расчитываем итоговые значения за смену
            DateTime lastDowntime = DateTime.MinValue;
            TimeSpan totalDuration = new TimeSpan();
            
            foreach (DownTime item in result)
            {
                if (item.TimeStart == DateTime.MinValue)
                {
                    lastDowntime = DateTime.MinValue;
                    totalDuration = new TimeSpan();
                    continue;
                }
                
                if (lastDowntime == DateTime.MinValue)
                {
                    lastDowntime = item.TimeStart;
                }
            
                if (item.TimeFinish != DateTime.MinValue)
                {
                    item.TimeDuration = item.TimeFinish.Subtract(item.TimeStart);
                    totalDuration += item.TimeDuration;
                }
                else
                {
                    item.TimeDuration = DateTime.Now.Subtract(item.TimeStart);
                    totalDuration += item.TimeDuration;
                }
                
                item.DurationTotal = totalDuration;
            }

            return result;
        }

        /// <summary>
        /// Получить список плавок за период по часам за сутки
        /// </summary>
        /// <param name="startPeriod">Начало периода</param>
        /// <param name="finishPeriod">Конец периода</param>
        /// <returns>Список плавок</returns>
        public List<MeltsForPeriod> GetMeltsByDay(DateTime startPeriod, DateTime finishPeriod)
        {
            List<MeltsForPeriod> result = new List<MeltsForPeriod>();
            List<MeltsForPeriod> sorted = new List<MeltsForPeriod>();
            List<MeltsForPeriod> meltsList = new List<MeltsForPeriod>();
            
            try
            {
                meltsList = _db.GetHourlyCoilsByPeriod(startPeriod, finishPeriod);
            }
            catch (Exception ex)
            {
                _logger.Error($"Не удалось получить список плавок за период с [{startPeriod}] по [{finishPeriod}] [{ex.Message}]");
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
            // Иначе только до текущего часа
            DateTime now = DateTime.Now;
            int finishHour = now.Date != prevDate.Date ? 23 : now.Hour;

            if (prevHour < finishHour)
            {
                for (int i = prevHour + 1; i <= finishHour; i++)
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

            // Расчет итоговых столбцов
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

        /// <summary>
        /// Получить список плавок за период по часам за смену
        /// </summary>
        /// <param name="start">Начало периода</param>
        /// <param name="finish">Конец периода</param>
        /// <returns>Список плавок</returns>
        public List<MeltsForPeriod> GetMeltsByShift(DateTime start, DateTime finish)
        {
            List<MeltsForPeriod> result = new List<MeltsForPeriod>();
            List<MeltsForPeriod> sorted = new List<MeltsForPeriod>();
            List<MeltsForPeriod> meltsList = new List<MeltsForPeriod>();
            DateTime date = new DateTime();

            try
            {
                meltsList = _db.GetHourlyCoilsByPeriod(start, finish);
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Не удалось получить список плавок за период с [{start}] по [{finish}] [{ex.Message}]");
            }

            if(meltsList.Count>0)
            {
                // Заполнение недостающих часов
                int shiftNumber = 0;
                int startHour;
                if (start.Hour == 8)
                {
                    // Дневная смена
                    startHour = 7;
                    date = new DateTime();
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
                        
                        startHour = item.WeightingHourStart;
                        shiftNumber = item.ShiftNumber;
                        date = item.WeightingData;
                        sorted.Add(item);
                    }

                    DateTime today = DateTime.Now;
                    int finishHour = date.Date == today.Date ? today.Hour : 19;
                    
                    if (startHour < finishHour)
                    {
                        for (int i = startHour + 1; i <= finishHour; i++)
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
                    startHour = 19;
                    date = new DateTime();

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

                            // Заполнить недостающие часы
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


        /// <summary>
        ///  Получить список плавок, посаженных в печь за период 
        /// </summary>
        /// <param name="start">Начало периода</param>
        /// <param name="end">Конец периода</param>
        /// <returns>Список плавок, посаженных в печь</returns>
        public List<ReportMeltsInOwen> GetMeltsInOwen(DateTime start, DateTime end)
        {
            List<ReportMeltsInOwen> result = _db.GetOwenReport(start, end);
            return result;
        }

        /// <summary>
        /// Отчет за сутки годного
        /// </summary>
        /// <param name="start">Начало периода</param>
        /// <param name="finish">Конец периода</param>
        /// <returns>Отчет за сутки годного</returns>
        public List<MeltsForPeriod> GetDailyReport(DateTime start, DateTime finish)
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
                _logger.Error($"Не удалось получить список плавок за период с [{start}] по [{finish}] [{ex.Message}]");
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

            // Расчет итогов для количества годного и веса
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

        /// <summary>
        /// Расчет данных по часам для выбранной смены
        /// </summary>
        /// <param name="start">Начало периода</param>
        /// <param name="finish">Конец периода</param>
        /// <returns>Отчет за смену годного</returns>
        public List<MeltsForPeriod> GetShiftReport(DateTime start, DateTime finish)
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
                    $"Не удалось получить список плавок за период с [{start}] по [{finish}] [{ex.Message}]");
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

        /// <summary>
        /// Получить отчет за период в разрезе смен
        /// </summary>
        /// <param name="periodStart">Начало периода</param>
        /// <param name="periodFinish">Конец периода</param>
        /// <returns></returns>
        public List<ShiftReport> GetShiftsReport(DateTime periodStart, DateTime periodFinish)
        {
            List<ShiftReport> result;
            List<ShiftReport> report = new List<ShiftReport>();
            
            try
            {
                result = _db.GetShiftReport(periodStart, periodFinish);
            }
            catch (Exception ex)
            {
                result = new List<ShiftReport>();
                _logger.Error(
                    $"Не удалось получить список плавок за период с [{periodStart}] по [{periodFinish}] [{ex.Message}]");
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

                        report.Add(shift);
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

                report.Add(shift);
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

                    report.Add(shift);
                    shiftTime = shiftTime.AddHours(12);
                    shiftNumber = shift.ShiftNumber;
                }
            }

            int totalCoils = 0;
            int totalWeight = 0;
            foreach (ShiftReport item in report)
            {
                totalCoils += item.CoilsCount;
                totalWeight += item.CoilsWeight;
            
                item.TotalCoils = totalCoils;
                item.TotalWeight = totalWeight;
            }

            return report;
        }

        /// <summary>
        /// Получить сменный рапорт производства
        /// </summary>
        /// <param name="periodStart">Начало периода</param>
        /// <param name="periodFinish">Конец периода</param>
        /// <returns>Сменный отчет по производству</returns>
        private List<ReportByShift> GetReportByShift(DateTime periodStart, DateTime periodFinish)
        {
            List<ReportByShift> result = new List<ReportByShift>();

            return result;
        }

        /// <summary>
        /// Получить список возвратов по номеру плавки
        /// </summary>
        /// <param name="melt">Номер плавки</param>
        /// <returns>Список возвратов</returns>
        public List<ReturningData> GetReturnsByMelt(string melt)
        {
            string query = _requests.GetReturnsByMelt(melt);
            List<ReturningData> result = _db.GetReturns(query);
            return result;
        }

        /// <summary>
        /// Получить список возвратов за период
        /// </summary>
        /// <param name="start">Начало периода</param>
        /// <param name="end">Конец периода</param>
        /// <returns>Список возвратов</returns>
        public /*async*/ List<LandingData> GetReturnsByPeriod(DateTime start, DateTime end)
        {
            /* ======= */
            /* 1. № плавки */
            /* 2. Прокатываемый профиль */
            /* 3. Диаметр */
            /* 4. Количество заготвок в плавке */
            /* 5. Взвешено заготовок */
            /* 6. Заготовок в печи */
            /* 7. Заготовок выдано из печи в стан */
            /* 8. Возвраты */
            /* 9. Заготовок прокатано */
            /* 10. Количество брака */
            /* 11. Взвешено бунтов (годного) */
            /* ======= */

            List<LandingData> meltsList = GetMelts(start, end, OrderTypes.OrderByPeriodStart);
            foreach (LandingData melt in meltsList)
            {
                /* Получить количество возвратов */
                int returns = _db.GetReturnsCountByMeltId(melt.LandingId.ToString());
                melt.IngotsReturned = returns;
                // await Task.Delay(100);
            }

            return meltsList;
        }

        /// <summary>
        /// Получить отчет проверки ДТ
        /// </summary>
        /// <param name="begin">Начало периода</param>
        /// <param name="end">Конец периода</param>
        /// <returns>Отчет проверки дт</returns>
        public List<CheckDtData> GetCheckDt(DateTime begin, DateTime end)
        {
            List<CheckDtData> result = _db.GetCheckDt(begin, end);

            return result;
        }

        /// <summary>
        /// Получить список бурежек за период
        /// </summary>
        /// <param name="begin">Начало периода</param>
        /// <param name="end">Конец периода</param>
        /// <returns></returns>
        public List<RejectionsData> GetRejections(DateTime begin, DateTime end)
        {
            List<RejectionsData> result = _db.GetRejectionsByPeriod(begin, end);

            return result;
        }

        /// <summary>
        /// Получить отчет по производству за период по сменам
        /// </summary>
        /// <param name="begin">Начало периода</param>
        /// <param name="end">Конец периода</param>
        /// <returns></returns>
        public List<LandingData> GetShiftProductionReportByPeriod(DateTime begin, DateTime end)
        {
            // Готовим результат для возврата из функцмм;
            List<LandingData> result = new List<LandingData>();
            
            // Получить список возвратов за период;
            List<LandingData> returns = GetReturnsByPeriod(begin, end);
            
            // Получить список бурежек за период;
            List<RejectionsData> rejected = GetRejections(begin, end);
            
            // Обновление данных по возвратам и бурежкам в результирующем наборе
            DateTime shiftBegin = DateTime.MinValue;    // Время начала смены для добавления разделителей смен
            DateTime shiftEnd = DateTime.MinValue;      // Время конца смены  для добавления разделителей смен
            Shift shift = new Shift();                  // Класс для работы со сменами

            // Обходим полученный список из базы данных
            foreach (LandingData melt in returns)
            {
                // Определяем время начала смены и время конца смены
                // Проходим по полученному из БД списку плавок
                // Если время начала смены не определено, ставим его равным времени начала смены, в которую каталась текущая плавка
                //      Ставим время конца смены равным времени конца смены, в которую каталась текущая плавка
                // Иначе, если время начала смены, в которую каталась текукщая плавка, не равна времени начала смены
                //      Добавляем пустую строку и устанавливаем время начала и конца смены равным времени начала и конца смены, в которую каталась плавка

                // Определяем смену, в которую был прокат
                ShiftData shiftData = shift.GetShiftByDate(melt.FirstWeighting);
                
                // Проверяем, установлено ли время начала смены
                if (shiftBegin == DateTime.MinValue)
                {
                    // Если нет, установим его
                    shiftBegin = shiftData.StartTime;
                    shiftEnd = shiftData.FinishTime;
                }
                else if (melt.FirstWeighting > shiftEnd)
                {
                    // Если да, и смена проката плавки была позже, добавим разделитель периодов
                    result.Add(new LandingData());
                    shiftBegin = shiftData.StartTime;
                    shiftEnd = shiftData.FinishTime;
                }
                
                // Для каждой плавки ищем соответствие номера плавки в списке бурежек
                foreach (RejectionsData rejects in rejected)
                {
                    if (melt.MeltNumber == rejects.Melt)
                    {
                        melt.IngotsRejected = rejects.IngotsCount;
                    }
                }
                
                result.Add(melt);
            }

            return result;
        }

        /// <summary>
        /// Получить отчет по производству за период по суткам
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<LandingData> GetDailyProductionReportByPeriod(DateTime begin, DateTime end)
        {
            // Готовим результат для возврата из функцмм;
            List<LandingData> result = new List<LandingData>();
            
            // Получить список возвратов за период;
            List<LandingData> returns = GetReturnsByPeriod(begin, end);
            
            // Получить список бурежек за период;
            List<RejectionsData> rejected = GetRejections(begin, end);
            
            // Обновление данных по возвратам и бурежкам в результирующем наборе
            DateTime prevDate = DateTime.MinValue;
            foreach (LandingData melt in returns)
            {
                if (prevDate == DateTime.MinValue)
                {
                    // Если неизвестна дата проката предыдущей плавки, ставим дату текущей плавки
                    prevDate = melt.FirstWeighting.Date;
                }
                else if (prevDate.Date != melt.FirstWeighting.Date)
                {
                    // Если дата текущей плавки не равна дате предыдущей плавки, то добавим разделитель периодов
                    result.Add(new LandingData());
                        
                    // Установим дату проката на дату проката текущей плавки
                    prevDate = melt.FirstWeighting.Date;
                }
                
                // Для каждой плавки ищем соответствие номера плавки в списке бурежек
                foreach (RejectionsData rejects in rejected)
                {
                    if (melt.MeltNumber == rejects.Melt)
                    {
                        melt.IngotsRejected = rejects.IngotsCount;
                    }
                }
                
                result.Add(melt);
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
            List<DeletedIngots> result = _db.GetDeletedIngotsByPeriod(begin, end);

            return result;
        }

    }
}