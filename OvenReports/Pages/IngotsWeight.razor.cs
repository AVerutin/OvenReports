using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing.Tree;
using OvenReports.Data;
using NLog;

namespace OvenReports.Pages
{
    /// <summary>
    /// Модель для представления данных с веб-формы
    /// </summary>
    public class MeltsWeight
    {
        public string MeltNumber { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        public MeltsWeight()
        {
            MeltNumber = default;
            PeriodEnd = DateTime.Now;
            PeriodStart = GetStartOfMonth(PeriodEnd);
        }

        private DateTime GetStartOfMonth(DateTime date)
        {
            DateTime result = new DateTime(date.Year, date.Month, 1);

            return result;
        }

        private DateTime GetLastDayOfMonth(DateTime date)
        {
            DateTime startDate = GetStartOfMonth(date); 
            DateTime finishDate = startDate.AddMonths(1).AddDays(-1);

            return finishDate;
        }
    }
    
    /// <summary>
    /// Отчет по весу заготовок
    /// </summary>
    public partial class IngotsWeight
    {
        private List<IngotsWeights> _ingotsList;
        private List<IngotsWeights> _meltsList;
        private MeltsWeight _meltsWeight = new MeltsWeight();
        private string _download;
        private string _allowDownload;
        private string _webRootPath;
        private bool _loaded;
        
        private readonly DbConnection _db = new DbConnection(true);
        private string _showReport = "none;";
        private Logger _logger;
        private string _loading = "hidden;";
        
        //TODO: Добавить на все страницы вывод строки в таблицу "Данные по плавке не найдены"
        
        protected override void OnInitialized()
        {
            Initialize();
        }

        private void Initialize()
        {
            _ingotsList = new List<IngotsWeights>();
            _meltsList = new List<IngotsWeights>();
            
            _allowDownload = "none;";
            _download = "#";
            
            _logger = LogManager.GetCurrentClassLogger();
            _webRootPath = ReportingService.WebHostEnvironment.WebRootPath;
            _webRootPath = Path.Combine(_webRootPath, "files");
            RemoveOldFiles(_webRootPath);
            _loaded = false;
        }
        
        private void _setLoading(bool visible)
        {
            _loading = visible ? "visible;" : "hidden;";
        }

        /// <summary>
        /// Получение подробных данных о заготовках по выбранной плавке
        /// </summary>
        private async void GetIngots(string meltNo)
        {
            _setLoading(true);
            _ingotsList = new List<IngotsWeights>();
            await Task.Delay(100);
            
            _meltsWeight.MeltNumber = meltNo;
            _ingotsList.Clear();
            _loaded = false;
            if (!string.IsNullOrEmpty(meltNo))
            {
                _ingotsList = _db.GetIngotsWeights(meltNo);
                if (_ingotsList.Count > 0)
                {
                    string[] rndFileName = Path.GetRandomFileName().Split(".");
                    string writeFileName = rndFileName[0] + ".csv";
                    string writePath = Path.Combine(_webRootPath, writeFileName);
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(writePath, false, Encoding.UTF8))
                        {
                            sw.WriteLine(
                                "№ плавки;Профиль;Диаметр профиля;Марка стали;Сечение заготовки;№ заготовки;Вес заготовки;Время взвешивания");
                            foreach (IngotsWeights item in _ingotsList)
                            {
                                sw.WriteLine($"{item.Melt.Trim()};" +
                                             $"{item.Profile};" +
                                             $"{item.Diameter:F1};" +
                                             $"{item.SteelGrade};" +
                                             $"{item.IngotProfile};" +
                                             $"{item.Position};" +
                                             $"{item.BilletWeight};" +
                                             $"{item.TimeBegin:dd.MM.yyyy HH:mm:ss}");
                            }
                        }

                        // using (StreamWriter sw = new StreamWriter(writePath, true, System.Text.Encoding.UTF8))
                        // {
                        //     sw.WriteLine("Дозапись");
                        //     sw.Write(4.5);
                        // }
                        _download = "files/" + writeFileName;
                        _allowDownload = "inline;";
                        _loaded = true;
                        _setLoading(false);
                        StateHasChanged();

                        _logger.Info($"Запись выполнена. Сформирован файл [{writeFileName}]");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Не удалось записать в файл: {ex.Message}");
                        _setLoading(false);
                    }
                }
            }
            else
            {
                _ingotsList = new List<IngotsWeights>();
            }
            _setLoading(false);
        }

        /// <summary>
        /// Удаление старых файлов
        /// </summary>
        private void RemoveOldFiles(string pathToFolder)
        {
            IEnumerable<string> allFiles = Directory.EnumerateFiles(pathToFolder, "*.csv");
            foreach (string filename in allFiles)
            {
                try
                {
                    File.Delete(filename);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Не удалось удалить файл {filename} - [{ex.Message}]");
                }
            }
        }
        
        /// <summary>
        /// Получить данные о заготовках по номеру плавки
        /// </summary>
        private async void GetMelt()
        {
            _setLoading(true);
            _meltsList = new List<IngotsWeights>();
            await Task.Delay(100);
            
            _meltsList = _db.GetMeltsWeight(DateTime.MinValue, DateTime.MinValue, _meltsWeight.MeltNumber);
            
            _showReport = "block;";
            _setLoading(false);
            StateHasChanged();
        }

        /// <summary>
        /// Получить список плавок, заготовки которых взвешивались в рамках указанного периода времени
        /// </summary>
        private async void GetMelts()
        {
            _setLoading(true);
            _meltsList = new List<IngotsWeights>();
            await Task.Delay(100);
            
            DateTime start = DateTime.Parse(_meltsWeight.PeriodStart.ToString("d") + " 00:00:00");
            DateTime finish = DateTime.Parse(_meltsWeight.PeriodEnd.ToString("d") + " 23:59:59");
            _meltsList = _db.GetMeltsWeight(start, finish, "");
            
            _showReport = "block;";
            _setLoading(false);
            StateHasChanged();
        }
    }
}
