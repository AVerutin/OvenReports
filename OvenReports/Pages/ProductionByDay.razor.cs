using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OvenReports.Data;

namespace OvenReports.Pages
{
    public partial class ProductionByDay
    {
        private DateTime _timeBegin;
        private DateTime _timeEnd;
        
        private static List<LandingData> _landed = new List<LandingData>();
        private readonly Reports _reports = new Reports();
        private string _showReport = "none";
        private string _loading = "hidden;";
        
        protected override void OnInitialized()
        {
            Initialize();
        }

        private void Initialize()
        {
            _timeEnd = DateTime.Now;
            _timeBegin =  new DateTime(_timeEnd.Year, _timeEnd.Month, 1);
        }

        private void _setLoading(bool visible)
        {
            _loading = visible ? "visible;" : "hidden;";
        }

        /// <summary>
        /// Отчет за выбранный период
        /// </summary>
        private async void GetMelts()
        {
            _setLoading(true);
            _landed = new List<LandingData>();
            await Task.Delay(100);

            DateTime start = _timeBegin;
            DateTime finish = _timeEnd;
            
            string dateStart = $"{start.Day}-{start.Month}-{start.Year} 00:00:00.000";
            start = DateTime.Parse(dateStart);
            string dateFinish = $"{finish.Day}-{finish.Month}-{finish.Year} 23:59:59.999";
            finish = DateTime.Parse(dateFinish);

            _landed = _reports.GetReturnsByPeriod(start, finish);
            _showReport = "block";
            _setLoading(false);
            StateHasChanged();
        }

        /// <summary>
        /// Отчет за текущие сутки
        /// </summary>
        private async void GetCurrentDay()
        {
            
        }

        /// <summary>
        /// Отчет за предыдущие сутки
        /// </summary>
        private async void GetPrevDay()
        {
            
        }

        /// <summary>
        /// Отчет за текущую смену
        /// </summary>
        private async void GetCurrentShift()
        {
            
        }

        /// <summary>
        /// Отчет за предыдущую смену 
        /// </summary>
        private async void GetPrevShift()
        {
            
        }

        /// <summary>
        /// Получить список бунтов для выбранной плавки
        /// </summary>
        /// <param name="meltNumber">Номер плавки</param>
        /// <param name="diameter">Диаметр прокатываемого профиля</param>
        private void GetPrepareCoils(string meltNumber, double diameter)
        {
            // Получать список бунтов по запросу из БД
            // _selectedMelt = _db.GetCoilsByMelt(meltNumber, diameter, false);
            // _meltInfo.MeltNumber = _selectedMelt[0].MeltNumber;
            // _meltInfo.Diameter = _selectedMelt[0].Diameter;
        }
    }
}

/******   Вывод списка заготовок из веб-части класса  ********
 
<div class="modal fade bd-example-modal-lg" tabindex="-1" aria-labelledby="myLargeModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="exampleModalLabel">Список бунтов плавки №@_meltNumber</h5>
                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
            </div>
            <div class="modal-body">
                <div id="viewMelt">
                    @* <div class="h3">Плавка №@_currentMelt[0].MeltNumber диаметр @_currentMelt[0].Diameter от @_currentMelt[0].DateReg</div> *@
                    <table>
                        <tr>
                            <td>
                                <table>
                                    <tr>
                                        <th scope="col" style="vertical-align: middle; width: 100px;">Плавка<br/>№</th>
                                        <th scope="col" style="vertical-align: middle; width: 100px;">Профиль</th>
                                        <th scope="col" style="vertical-align: middle; width: 100px;">Диаметр<br/>профиля</th>
                                        <th scope="col" style="vertical-align: middle; width: 100px;">№<br/>бунта</th>
                                        <th scope="col" style="vertical-align: middle; width: 155px;">Вес<br/>бунта</th>
                                        <th scope="col" style="vertical-align: middle; width: 250px;">Дата<br/>взвешивания</th>
                                        <th scope="col" style="vertical-align: middle; width: 15px;">&nbsp;</th>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <div style="height:630px; overflow:auto;">
                                    <table class="table table-hover">
                                        <tbody>
                                        @foreach (CoilData item in _selectedMelt)
                                        {
                                            if (!string.IsNullOrEmpty(item.MeltNumber))
                                            {
                                                <tr>
                                                    <th style="width: 100px;">@item.MeltNumber</th>
                                                    <td style="width: 100px;">@item.ProductionProfile</td>
                                                    <td style="width: 100px;">@item.Diameter</td>
                                                    <td style="width: 100px;">@item.CoilNumber</td>
                                                    <td style="width: 155px;">@item.WeightFact</td>
                                                    <td style="width: 250px;">@item.DateWeight</td>
                                                </tr>
                                            }
                                            else
                                            {
                                                <tr>
                                                    <td style="width: 100px;">&nbsp;</td>
                                                    <td style="width: 100px;">&nbsp;</td>
                                                    <td style="width: 100px;">&nbsp;</td>
                                                    <td style="width: 155px;">&nbsp;</td>
                                                    <td style="width: 250px;">&nbsp;</td>
                                                </tr>
                                            }
                                        }
                                        </tbody>
                                    </table>
                                </div>
                            </td>
                        </tr>
                    </table>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal">Закрыть</button>
                </div>
            </div>
        </div>
    </div>
</div>
*/