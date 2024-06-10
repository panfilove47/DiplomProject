using ChartJs.Blazor.Common;
using ChartJs.Blazor.Common.Axes;
using ChartJs.Blazor.Common.Axes.Ticks;
using ChartJs.Blazor.LineChart;
using ChartJs.Blazor.PieChart;
using ChartJs.Blazor.Util;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
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

        private LineConfig _config;

        protected override async void OnInitialized()
        {
            train = await trainRepository.GetTrainById(Id);

            movements = await trainRepository.GetMovementsById(Id);
            AddDiagramm();
            base.OnInitialized();
        }

        protected override async Task OnParametersSetAsync()
        {
            train = await trainRepository.GetTrainById(Id);

            movements = await trainRepository.GetMovementsById(Id);

        }


        public void AddDiagramm()
        {
            _config = new LineConfig();

            _config.Options = new LineOptions
            {
                Responsive = true,
                Title = new OptionsTitle
                {
                    Display = true,
                    Text = "Анализ движения поездов"
                },
                //Scales = new Scales
                //{
                //    XAxes = new List<CartesianAxis>
                //{
                //    new LinearCartesianAxis
                //    {
                //        ScaleLabel = new ScaleLabel
                //        {
                //            Display = true,
                //            LabelString = "Поезда"
                //        },
                //        Ticks = new LinearCartesianTicks
                //        {
                //            AutoSkip = false
                //        }
                //    }
                //},
                //    YAxes = new List<CartesianAxis>
                //{
                //    new LinearCartesianAxis
                //    {
                //        ScaleLabel = new ScaleLabel
                //        {
                //            Display = true,
                //            LabelString = "Время (часы)"
                //        },
                //        Ticks = new LinearCartesianTicks
                //        {
                //            Min = DateTime.Now.Date.ToOADate(), // Минимальное значение (например, начало текущего дня)
                //            Max = DateTime.Now.Date.AddDays(1).ToOADate()
                //        }
                //    }
                //}
                //}
            };

            foreach (var item in movements.Select(m => m.To))
            {
                _config.Data.Labels.Add(item);
            }
            //_config.Data.Labels.(movements.Select(m => m.Name));

            var scheduledArrivals = new LineDataset<double>
            {
                Label = "Запланированное время прибытия",
                BackgroundColor = ColorUtil.ColorHexString(0, 0, 255),
                BorderColor = ColorUtil.ColorHexString(0, 0, 255),
                Fill = false
            };

            var actualArrivals = new LineDataset<double>
            {
                Label = "Фактическое время прибытия",
                BackgroundColor = ColorUtil.ColorHexString(153, 0, 0),
                BorderColor = ColorUtil.ColorHexString(153, 0, 0),
                Fill = false
            };

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
                scheduledArrivals.Add(movement.ScheduleArrivalTime.ToOADate());
                actualArrivals.Add(movement.RealArrivalTime.ToOADate() * 2);
                scheduledDepartures.Add(movement.ScheduleDepartureTime.ToOADate());
                actualDepartures.Add(movement.RealDepartureTime.ToOADate() * 2);
            }

            _config.Data.Datasets.Add(scheduledArrivals);
            _config.Data.Datasets.Add(actualArrivals);
            _config.Data.Datasets.Add(scheduledDepartures);
            _config.Data.Datasets.Add(actualDepartures);
        }

    }
}
