using ChartJs.Blazor.BarChart;
using ChartJs.Blazor.Common;
using ChartJs.Blazor.Common.Axes;
using ChartJs.Blazor.Common.Enums;
using ChartJs.Blazor.Common.Time;
using ChartJs.Blazor.LineChart;
using ChartJs.Blazor.PieChart;
using ChartJs.Blazor.Util;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using System.ComponentModel.DataAnnotations;
using Trains.Interfaces;
using Trains.Models.DTO;

namespace Trains.Components.Pages
{
    public partial class Train
    {
        [Inject]
        public ITrainRepository trainRepository { get; set; }

        [Parameter]
        public int Id { get; set; }

        private TrainDto train;

        private IEnumerable<TrainMovementsDto> movements;

        public DateTime? From;

        public DateTime? To;

        private LineConfig _arrLine;

        private LineConfig _depLine;

        private LineConfig _diffLine;

        private BarConfig _arrBar;
        private BarConfig _depBar;
        private BarConfig _diffBar;
        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        public bool ShowLineDiagrammDepartures { get; set; } = false;

        public bool ShowLineDiagrammArrivals { get; set; } = false; 

        public bool ShowDifferenceDiagramm { get; set; } = false;

        public bool ShowBarDiagrammDepartures { get; set; } = false;

        public bool ShowBarDiagrammArrivals { get; set; } = false;

        public bool ShowBarDifferenceDiagramm { get; set; } = false;

        protected override async void OnInitialized()
        {
            await LoadAsync();
            base.OnInitialized();
        }

        protected override async Task OnParametersSetAsync()
        {
            train = await trainRepository.GetTrainById(Id);

            movements = await trainRepository.GetMovementsById(Id, From, To);

        }

        protected async Task LoadAsync(CancellationToken ct = default)
        {
            train = await trainRepository.GetTrainById(Id);

            movements = await trainRepository.GetMovementsById(Id, From, To);
        }

        public async void Search()
        {
            await LoadAsync();
        }


        public async Task GenerateExcelReport()
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Train Movements");

            var stationNames = new List<string>();
            var plannedArrivalTimes = new List<DateTime>();
            var actualArrivalTimes = new List<DateTime>();

            // Заголовки
            worksheet.Cells[1, 1].Value = "Номер поезда";
            worksheet.Cells[1, 2].Value = "Название";
            worksheet.Cells[1, 3].Value = "Станция";
            worksheet.Cells[1, 4].Value = "Запланированное время отправления";
            worksheet.Cells[1, 5].Value = "Запланированное время прибытия";
            worksheet.Cells[1, 6].Value = "Реальное время отправления";
            worksheet.Cells[1, 7].Value = "Реальное время прибытия";
            worksheet.Cells[1, 8].Value = "Разница во времени отправления";
            worksheet.Cells[1, 9].Value = "Разница во времени прибытия";

            // Данные
            int row = 2;
            foreach (var item in movements)
            {
                worksheet.Cells[row, 1].Value = item.Id;
                worksheet.Cells[row, 2].Value = item.Name;
                worksheet.Cells[row, 3].Value = item.To;
                worksheet.Cells[row, 4].Value = item.ScheduleDepartureTime;
                worksheet.Cells[row, 4].Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                worksheet.Cells[row, 5].Value = item.ScheduleArrivalTime;
                worksheet.Cells[row, 5].Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                worksheet.Cells[row, 6].Value = item.RealDepartureTime;
                worksheet.Cells[row, 6].Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                worksheet.Cells[row, 7].Value = item.RealArrivalTime;
                worksheet.Cells[row, 7].Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                worksheet.Cells[row, 8].Value = item.DifferenceDeparture.TotalMinutes;
                worksheet.Cells[row, 9].Value = item.DifferenceArrival.TotalMinutes;
                row++;

                stationNames.Add(item.To);
                plannedArrivalTimes.Add(item.ScheduleArrivalTime);
                actualArrivalTimes.Add(item.RealArrivalTime);
            }

            worksheet.Cells.AutoFitColumns();

            var chartDep = worksheet.Drawings.AddChart("DepartureLineChart", eChartType.Line);
            chartDep.SetPosition(stationNames.Count + 1, 0, 0, 0); // Установка положения графика на листе
            chartDep.SetSize(1600, 600); // Установка размеров графика
            var scheduledDep = chartDep.Series.Add(worksheet.Cells[$"D2:D{plannedArrivalTimes.Count + 1}"], worksheet.Cells[$"C2:C{stationNames.Count + 1}"]); // Добавление данных на график
            var realDep = chartDep.Series.Add(worksheet.Cells[$"F2:F{plannedArrivalTimes.Count + 1}"], worksheet.Cells[$"C2:C{stationNames.Count + 1}"]);
            scheduledDep.Header = "Запланированное время отправления";
            realDep.Header = "Реальное время отправления";
            chartDep.Title.Text = "График времени отправления по станциям"; // Установка заголовка графика
            chartDep.YAxis.Title.Text = "Время отправления"; // Установка подписи оси Y
            chartDep.XAxis.Title.Text = "Станция"; // Установка подписи оси X

            var chartArr = worksheet.Drawings.AddChart("ArrivalLineChart", eChartType.Line);
            chartArr.SetPosition(stationNames.Count + stationNames.Count + 10, 0, 0, 0); // Установка положения графика на листе
            chartArr.SetSize(1600, 600); // Установка размеров графика
            var scheduledArr = chartArr.Series.Add(worksheet.Cells[$"E2:E{plannedArrivalTimes.Count + 1}"], worksheet.Cells[$"C2:C{stationNames.Count + 1}"]); // Добавление данных на график
            var realArr = chartArr.Series.Add(worksheet.Cells[$"G2:G{plannedArrivalTimes.Count + 1}"], worksheet.Cells[$"C2:C{stationNames.Count + 1}"]);
            scheduledArr.Header = "Запланированное время прибытия";
            realArr.Header = "Реальное время прибытия";
            chartArr.Title.Text = "График времени прибытия по станциям"; // Установка заголовка графика
            chartArr.YAxis.Title.Text = "Время прибытия"; // Установка подписи оси Y
            chartArr.XAxis.Title.Text = "Станция"; // Установка подписи оси X


            var diff = worksheet.Drawings.AddChart("DifferenceChart", eChartType.Line);
            diff.SetPosition(stationNames.Count + stationNames.Count + stationNames.Count + 20, 0, 0, 0); // Установка положения графика на листе
            diff.SetSize(1600, 600); // Установка размеров графика
            var diffArr = diff.Series.Add(worksheet.Cells[$"H2:H{plannedArrivalTimes.Count + 1}"], worksheet.Cells[$"C2:C{stationNames.Count + 1}"]); // Добавление данных на график
            var diffDep = diff.Series.Add(worksheet.Cells[$"I2:I{plannedArrivalTimes.Count + 1}"], worksheet.Cells[$"C2:C{stationNames.Count + 1}"]);
            diffArr.Header = "Разница прибытия";
            diffDep.Header = "Разница отправления";
            diff.Title.Text = "Разница прибытия/отправления"; // Установка заголовка графика
            diff.YAxis.Title.Text = "Разница"; // Установка подписи оси Y
            diff.XAxis.Title.Text = "Станция"; // Установка подписи оси X



            // Генерация Excel файла в память
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            // Вызов JavaScript для скачивания файла
            var fileName = $"TrainMovements_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            await JSRuntime.InvokeVoidAsync("downloadFile", Convert.ToBase64String(stream.ToArray()), fileName);
        }

        public void AddLineDiagrammDepartures()
        {
            ShowLineDiagrammDepartures = ShowLineDiagrammDepartures ? false : true;

            _depLine = new LineConfig();

            _depLine.Options = new LineOptions
            {
                Responsive = true,
                Title = new OptionsTitle
                {
                    Display = true,
                    Text = "Зависимость отправления поезда"
                },
            };

            foreach (var item in movements)
            {
                _depLine.Data.Labels.Add(item.To);
            }

            var scheduledDepartures = new LineDataset<double>
            {
                Label = "Запланированное время отправления",
                BackgroundColor = ColorUtil.ColorHexString(204, 0, 255),
                BorderColor = ColorUtil.ColorHexString(204, 0, 255),
                Fill = false
            };

            var actualDepartures = new LineDataset<double>
            {
                Label = "Фактическое время отправления",
                BackgroundColor = ColorUtil.ColorHexString(0, 255, 0),
                BorderColor = ColorUtil.ColorHexString(0, 255, 0),
                Fill = false
            };

            foreach (var movement in movements)
            {
                scheduledDepartures.Add(movement.ScheduleDepartureTime.ToOADate());
                actualDepartures.Add(movement.RealDepartureTime.ToOADate());
            }

            _depLine.Data.Datasets.Add(scheduledDepartures);
            _depLine.Data.Datasets.Add(actualDepartures);
        }

        public void AddLineDiagrammArrivals()
        {
            ShowLineDiagrammArrivals = ShowLineDiagrammArrivals ? false : true;

            _arrLine = new LineConfig();

            _arrLine.Options = new LineOptions
            {
                Responsive = true,
                Title = new OptionsTitle
                {
                    Display = true,
                    Text = "Зависимость прибытия поезда"
                }
            };

            foreach (var item in movements)
            {
                _arrLine.Data.XLabels.Add(item.To);
            }

            var scheduledArrivals = new LineDataset<double>
            {
                Label = "Запланированное время прибытия",
                BackgroundColor = ColorUtil.ColorHexString(204, 0, 255),
                BorderColor = ColorUtil.ColorHexString(204, 0, 255),
                Fill = false
            };

            var actualArrivals = new LineDataset<double>
            {
                Label = "Фактическое время прибытия",
                BackgroundColor = ColorUtil.ColorHexString(0, 255, 0),
                BorderColor = ColorUtil.ColorHexString(0, 255, 0),
                Fill = false
            };

            foreach (var movement in movements)
            {
                scheduledArrivals.Add(movement.ScheduleArrivalTime.ToOADate());
                actualArrivals.Add(movement.RealArrivalTime.ToOADate());
            }

            _arrLine.Data.Datasets.Add(scheduledArrivals);
            _arrLine.Data.Datasets.Add(actualArrivals);
        }

        public void AddDifferenceDiagramm()
        {
            ShowDifferenceDiagramm = ShowDifferenceDiagramm ? false : true;
            _diffLine = new LineConfig();

            _diffLine.Options = new LineOptions
            {
                Responsive = true,
                Title = new OptionsTitle
                {
                    Display = true,
                    Text = "Разница"
                }
            };

            foreach (var item in movements)
            {
                _diffLine.Data.XLabels.Add(item.To);
            }

            var differenceArrival = new LineDataset<double>
            {
                Label = "Разница в прибытии",
                BackgroundColor = ColorUtil.ColorHexString(204, 0, 255),
                BorderColor = ColorUtil.ColorHexString(204, 0, 255),
                Fill = false
            };

            var differenceDepartures = new LineDataset<double>
            {
                Label = "Разница в отправлении",
                BackgroundColor = ColorUtil.ColorHexString(0, 255, 0),
                BorderColor = ColorUtil.ColorHexString(0, 255, 0),
                Fill = false
            };

            foreach (var movement in movements)
            {
                differenceArrival.Add(movement.DifferenceArrival.TotalMinutes);
                differenceDepartures.Add(movement.DifferenceDeparture.TotalMinutes);
            }

            _diffLine.Data.Datasets.Add(differenceArrival);
            _diffLine.Data.Datasets.Add(differenceDepartures);
        }

        public void AddBarDiagrammDepartures()
        {
            ShowBarDiagrammDepartures = ShowBarDiagrammDepartures ? false : true;

            _depBar = new BarConfig();

            _depBar.Options = new BarOptions
            {
                Responsive = true,
                Title = new OptionsTitle
                {
                    Display = true,
                    Text = "Зависимость отправления поезда"
                }
            };

            foreach (var item in movements)
            {
                _depBar.Data.Labels.Add(item.To);
            }

            var scheduledDepartures = new BarDataset<double>
            {
                Label = "Запланированное время отправления",
                BackgroundColor = ColorUtil.ColorHexString(204, 0, 255)
            };

            var actualDepartures = new BarDataset<double>
            {
                Label = "Фактическое время отправления",
                BackgroundColor = ColorUtil.ColorHexString(0, 255, 0)
            };

            foreach (var movement in movements)
            {
                scheduledDepartures.Add(movement.ScheduleDepartureTime.ToOADate());
                actualDepartures.Add(movement.RealDepartureTime.ToOADate());
            }

            _depBar.Data.Datasets.Add(scheduledDepartures);
            _depBar.Data.Datasets.Add(actualDepartures);
        }

        public void AddBarDiagrammArrivals()
        {
            ShowBarDiagrammArrivals = ShowBarDiagrammArrivals ? false : true;

            _arrBar = new BarConfig();

            _arrBar.Options = new BarOptions
            {
                Responsive = true,
                Title = new OptionsTitle
                {
                    Display = true,
                    Text = "Зависимость прибытия поезда"
                }
            };

            foreach (var item in movements)
            {
                _arrBar.Data.Labels.Add(item.To);
            }

            var scheduledArrivals = new BarDataset<double>
            {
                Label = "Запланированное время прибытия",
                BackgroundColor = ColorUtil.ColorHexString(204, 0, 255)
            };

            var actualArrivals = new BarDataset<double>
            {
                Label = "Фактическое время прибытия",
                BackgroundColor = ColorUtil.ColorHexString(0, 255, 0)
            };

            foreach (var movement in movements)
            {
                scheduledArrivals.Add(movement.ScheduleArrivalTime.ToOADate());
                actualArrivals.Add(movement.RealArrivalTime.ToOADate());
            }

            _arrBar.Data.Datasets.Add(scheduledArrivals);
            _arrBar.Data.Datasets.Add(actualArrivals);
        }

        public void AddBarDiagrammDifference()
        {
            ShowBarDifferenceDiagramm = ShowBarDifferenceDiagramm ? false : true;

            _diffBar = new BarConfig();

            _diffBar.Options = new BarOptions
            {
                Responsive = true,
                Title = new OptionsTitle
                {
                    Display = true,
                    Text = "Разница"
                }
            };

            foreach (var item in movements)
            {
                _diffBar.Data.Labels.Add(item.To);
            }

            var differenceArrival = new BarDataset<double>
            {
                Label = "Разница в прибытии",
                BackgroundColor = ColorUtil.ColorHexString(204, 0, 255)
            };

            var differenceDepartures = new BarDataset<double>
            {
                Label = "Разница в отправлении",
                BackgroundColor = ColorUtil.ColorHexString(0, 255, 0)
            };

            foreach (var movement in movements)
            {
                differenceArrival.Add(movement.DifferenceArrival.TotalMinutes);
                differenceDepartures.Add(movement.DifferenceDeparture.TotalMinutes);
            }

            _diffBar.Data.Datasets.Add(differenceArrival);
            _diffBar.Data.Datasets.Add(differenceDepartures);
        }


    }
}
