﻿using System;

namespace OvenReports.Data
{
    public class Shift
    {
        public DateTime ShiftStart { get; set; }
        public DateTime ShiftFinish { get; set; }
        public int ShiftNumber { get; set; }
        public int ShiftCount { get; set; }

        public Shift()
        {
            ShiftStart = DateTime.MinValue;
            ShiftFinish = DateTime.MinValue;
            ShiftNumber = 0;
            ShiftCount = 0;
        }

        private int GetShiftCount(DateTime date)
        {
            DateTime startDate = DateTime.Parse("2020-01-01 08:00:00");
        
            TimeSpan dateInterval = date - startDate;
            int shiftIndex = (int)(dateInterval.TotalHours / 12) % 8;

            return shiftIndex;
        }
        
        /// <summary>
        /// Получить номер бригады по дате и времени работы
        /// </summary>
        /// <param name="date">Дата и время работы</param>
        /// <returns>Номер бригады</returns>
        private int GetShiftNumber(DateTime date)
        {
            int[] shifts = {1, 4, 2, 1, 3, 2, 4, 3};
            DateTime startDate = DateTime.Parse("2020-01-01 08:00:00");
        
            TimeSpan dateInterval = date - startDate;
            int shiftIndex = (int)(dateInterval.TotalHours / 12) % 8;

            int shift = shifts[shiftIndex];

            return shift;
        }

        /// <summary>
        /// Получить номер текущей бригады
        /// </summary>
        /// <returns>Номер текущей бригады</returns>
        public int GetCurrentShiftNumber()
        {
            int[] shifts = {1, 4, 2, 1, 3, 2, 4, 3};
            DateTime startDate = DateTime.Parse("2020-01-01 08:00:00");
            DateTime date = DateTime.Now;
        
            TimeSpan dateInterval = date - startDate;
            int shiftIndex = (int)(dateInterval.TotalHours / 12) % 8;

            int shift = shifts[shiftIndex];

            return shift;
        }

        /// <summary>
        /// Получить номер предыдущей бригады
        /// </summary>
        /// <returns>Номер предыдущей бригады</returns>
        public int GetPreviousShiftNumber()
        {
            int prev = GetShiftNumber(DateTime.Now.AddHours(-12));
            ShiftNumber = prev;
            
            return prev;
        }

        /// <summary>
        /// Получить данные о бригаде по дате и времени работы
        /// </summary>
        /// <param name="date">Дата и время работы</param>
        /// <returns>Данные о бригаде</returns>
        public ShiftData GetShiftByDate(DateTime date)
        {
            ShiftNumber = GetShiftNumber(date);
            ShiftCount = GetShiftCount(date);
            ShiftStart = GetShiftStart(date);
            ShiftFinish = GetShiftFinish(date);

            ShiftData shiftData = new ShiftData
            {
                Count = ShiftCount, 
                Number = ShiftNumber, 
                StartTime = ShiftStart, 
                FinishTime = ShiftFinish
            };

            return shiftData;
        }

        /// <summary>
        /// Получить данные о текущей бригаде
        /// </summary>
        /// <returns>Данные о текущей бригаде</returns>
        public ShiftData GetCurrentShift()
        {
            DateTime date = DateTime.Now;
            ShiftNumber = GetShiftNumber(date);
            ShiftCount = GetShiftCount(date);
            ShiftStart = GetShiftStart(date);
            ShiftFinish = GetShiftFinish(date);

            ShiftData shiftData = new ShiftData
            {
                Count = ShiftCount, 
                Number = ShiftNumber, 
                StartTime = ShiftStart, 
                FinishTime = ShiftFinish
            };

            return shiftData;
        }

        /// <summary>
        /// Получить данные о предыдущей бригаде
        /// </summary>
        /// <returns>Данные о предыдущей бригаде</returns>
        public ShiftData GetPreviousShift()
        {
            ShiftData prev = GetShiftByDate(DateTime.Now.AddHours(-12));
            ShiftCount = prev.Count;
            ShiftNumber = prev.Number;
            ShiftStart = prev.StartTime;
            ShiftFinish = prev.FinishTime;
            
            return prev;
        }
        
        /// <summary>
        /// Получить время началы смены по дате и времени
        /// </summary>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        public static DateTime GetShiftStart(DateTime currentTime)
        {
            string startShift;
            
            // Текущий час между 8:00 и 20:00, смена началась сегодня в 8:00
            if (currentTime.Hour >= 8 && currentTime.Hour < 20)
            {
                startShift = $"{currentTime.Day}-{currentTime.Month}-{currentTime.Year} 08:00:00";
            }
            else
            {
                // Текущий час больше 20:00, смена началась сегодня в 20:00
                if(currentTime.Hour >= 20)
                {
                    startShift = $"{currentTime.Day}-{currentTime.Month}-{currentTime.Year} 20:00:00";
                }
                else
                {
                    // Текущий час меньше 8:00, смена началась вчера в 20:00
                    var endDate = currentTime.AddDays(-1);
                    startShift = $"{endDate.Day}-{endDate.Month}-{endDate.Year} 20:00:00";
                }
            }

            DateTime result = DateTime.Parse(startShift);
            return result;
        }

        public static DateTime GetShiftFinish(DateTime currentTime)
        {
            DateTime result;

            if (currentTime.Hour >= 8 && currentTime.Hour < 20)
            {
                // Текущий час между 8 и 20 часов, смена закончится в сегодня в 20:00
                string time = $"{currentTime.Day}-{currentTime.Month}-{currentTime.Year} 20:00:00";
                result = DateTime.Parse(time);
            }
            else
            {
                if (currentTime.Hour < 8)
                {
                    // Текущий час менее 8 часов, смена законится сегодня в 8:00
                    string time = $"{currentTime.Day}-{currentTime.Month}-{currentTime.Year} 08:00:00";
                    result = DateTime.Parse(time);
                }
                else
                {
                    // Текущий час больше 20 часов смена законится завтра в 8:00
                    DateTime finishTime = currentTime.AddDays(1);
                    
                    string time = $"{finishTime.Day}-{finishTime.Month}-{finishTime.Year} 08:00:00";
                    result = DateTime.Parse(time);
                }
            }

            return result;
        }
    }
}