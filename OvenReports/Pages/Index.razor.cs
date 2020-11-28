using System;
using System.Collections.Generic;
using OvenReports.Data;
using NLog;

namespace OvenReports.Pages
{
    public partial class Index
    {
        private struct MeltInfo
        {
            public string MeltNumber;
            public double Diameter;
        }
        
        private MeltsForPeriod _meltsPeriod = new MeltsForPeriod();
        private List<LandingData> _meltsList = new List<LandingData>();
        private List<MeltsList> melts = new List<MeltsList>();
        
        private DBConnection _db = new DBConnection();
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

        }

        private void GetMelt()
        {
            string meltNumber = _meltsPeriod.MeltNumber;
            if (!string.IsNullOrEmpty(meltNumber))
            {
                List<LandingData> melts_list = new List<LandingData>();
                _meltsList = new List<LandingData>();
                
                try
                {
                    //_meltsList = _db.GetMeltByNumber(meltNumber);
                    melts_list = _db.GetMeltByNumber(meltNumber);
                }
                catch (Exception ex)
                {
                    _logger.Error(
                        $"Не удалось получить плавки №{meltNumber} [{ex.Message}]");
                }

                foreach (LandingData melt in melts_list)
                {
                    if(melt.Weighted>0)
                        _meltsList.Add(melt);
                }

                foreach (LandingData melt in _meltsList)
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

                _showReport = "block";
                StateHasChanged();
            }            
        }

        private void GetMelts()
        {
            DateTime start = _meltsPeriod.PeriodStart;
            DateTime finish = _meltsPeriod.PeriodFinish;
            
            string dateStart = $"{start.Day}-{start.Month}-{start.Year} 00:00:00.000";
            start = DateTime.Parse(dateStart);
            string dateFinish = $"{finish.Day}-{finish.Month}-{finish.Year} 23:59:59.999";
            finish = DateTime.Parse(dateFinish);
            
            _meltsList = new List<LandingData>();
            melts = new List<MeltsList>();
            
            // List<CoilData> coils = new List<CoilData>();
            // try
            // {
            //      coils = _db.GetAllCoilsByPeriod(start, finish);
            // }
            // catch (Exception ex)
            // {
            //     _logger.Error(
            //         $"Не удалось получить список бунтов за период с [{start}] по [{finish}] [{ex.Message}]");
            // }
            //
            // LandingData landing = new LandingData();
            // List<LandingData> landings = new List<LandingData>();
            //
            // foreach (CoilData coil in coils)
            // {
            //     if (coil.MeltNumber != landing.MeltNumber && Math.Abs(coil.Diameter - landing.Diameter) > 0.1)
            //     {
            //         // Обнаружена новая плавка
            //         if (!string.IsNullOrEmpty(landing.MeltNumber) && landing.Diameter != 0)
            //         {
            //             landing.Weighted = landing.CoilList.Count;
            //             landing.FirstWeighting = landing.CoilList[0].DateWeight;
            //             landing.LastWeighting = landing.CoilList[^1].DateWeight;
            //             landings.Add(landing);
            //             landing = new LandingData();
            //         }
            //         
            //         landing.LandingId = coil.PosadUid;
            //         landing.MeltNumber = coil.MeltNumber;
            //         landing.SteelMark = coil.SteelMark;
            //         landing.IngotProfile = coil.IngotProfile;
            //         landing.IngotsCount = coil.IngotsCount;
            //         landing.WeightAll = coil.WeightAll;
            //         landing.WeightOne = coil.WeightOne;
            //         landing.IngotLength = coil.IngotLength;
            //         landing.Standart = coil.Standart;
            //         landing.IngotProfile = coil.IngotProfile;
            //         landing.Diameter = coil.Diameter;
            //         landing.Customer = coil.Customer;
            //         landing.Shift = coil.Shift;
            //         landing.IngotClass = coil.Class;
            //         landing.ProductCode = coil.ProductionCode;
            //
            //     }
            //     else
            //     {
            //         // Новый бунт текущей плавки
            //         landing.WeightReal += coil.WeightFact;
            //         landing.CoilList.Add(coil);
            //     }
            // }
            //
            // foreach (LandingData land in landings)
            // {
            //     if(land.LastWeighting>=start && land.LastWeighting<=finish)
            //         _meltsList.Add(land);
            // }

            // try
            // {
            //     // _meltsList = _db.GetMeltsListForPeriod(start, finish); 
            //     melts = _db.GetMeltsListByPeriod(start, finish);
            // }
            // catch (Exception ex)
            // {
            //     _logger.Error(
            //         $"Не удалось получить список плавок за период с [{start}] по [{finish}] [{ex.Message}]");
            // }
            //
            // foreach (MeltsList melt in melts)
            // {
            //     var meltsList = _db.GetMeltByNumber(melt.MeltNumber, melt.Diameter);
            //     foreach (LandingData mlt in meltsList)
            //     {
            //         if(mlt.Weighted>0)
            //         {
            //             _meltsList.Add(mlt);
            //         }
            //     }
            // }
            //
            // foreach (LandingData melt in _meltsList)
            // {
            //     try
            //     {
            //         melt.CoilList = _db.GetCoilsByMelt(melt.MeltNumber, melt.Diameter, false);
            //     }
            //     catch (Exception ex)
            //     {
            //         _logger.Error($"Не удалось получить список бунтов для плавки №{melt.MeltNumber}, диаметр {melt.Diameter} [{ex.Message}]");
            //     }
            //     
            //     foreach (CoilData coil in melt.CoilList)
            //     {
            //         melt.WeightReal += coil.WeightFact;
            //     }
            //     
            //     melt.Weighted = melt.CoilList.Count;
            //     melt.FirstWeighting = melt.CoilList[0].DateWeight;
            //     melt.LastWeighting = melt.CoilList[^1].DateWeight;
            // }

            try
            {
                melts = _db.GetMeltsListSummary(start, finish);
                
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Не удалось получить список плавок за период с [{start}] по [{finish}] [{ex.Message}]");
            }

            foreach (MeltsList melt in melts)
            {
                LandingData item = new LandingData();
                item.MeltNumber = melt.MeltNumber;
                item.ProductProfile = melt.ProductProfile;
                item.Diameter = melt.Diameter;
                item.FirstWeighting = melt.PeriodStart;
                item.LastWeighting = melt.PeriodFinish;
                item.SteelMark = melt.SteelMark;
                item.IngotProfile = melt.IngotProfile;
                item.IngotsCount = melt.IngotsCount;
                item.IngotLength = melt.IngotLength;
                item.Standart = melt.Standart;
                item.ProductCode = melt.ProductCode;
                item.Customer = melt.Customer;
                item.Weighted = melt.CoilsCount;
                item.WeightReal = melt.TotalWeight;
                
                _meltsList.Add(item);
            }
            
            _showReport = "block";
            StateHasChanged();
        }

        private void PrepareCoils(string meltNumber, double diameter)
        {
            // Получать список бунтов по запросу из БД
            _selectedMelt = _db.GetCoilsByMelt(meltNumber, diameter, false);
            _meltInfo.MeltNumber = _selectedMelt[0].MeltNumber;
            _meltInfo.Diameter = _selectedMelt[0].Diameter;

        }
    }
}