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

        public DateTime? From;

        public DateTime? To;

        private LineConfig _arr;

        private LineConfig _dep;

        private LineConfig _diff;

        public bool ShowCharts { get; set; } = false;

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

        public void AddDiagramm()
        {
            ShowCharts = true;
            _arr = new LineConfig();

            _arr.Options = new LineOptions
            {
                Responsive = true,
                Title = new OptionsTitle
                {
                    Display = true,
                    Text = "Зависимость прибытия поезда"
                }
            };

            _dep = new LineConfig();

            _dep.Options = new LineOptions
            {
                Responsive = true,
                Title = new OptionsTitle
                {
                    Display = true,
                    Text = "Зависимость отправленияв"
                }
            };

            _diff = new LineConfig();

            _diff.Options = new LineOptions
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
                _arr.Data.XLabels.Add(item.To);
                _dep.Data.XLabels.Add(item.To);
                _arr.Data.YLabels.Add(item.ScheduleArrivalTime.ToString("dd-MM-yyyy"));
                _dep.Data.YLabels.Add(item.ScheduleDepartureTime.ToString("dd-MM-yyyy"));

                _diff.Data.XLabels.Add(item.To);
            }
            //_config.Data.Labels.(movements.Select(m => m.Name));

            var differenceArrival = new LineDataset<double>
            {
                Label = "Разница в прибытии",
                BackgroundColor = ColorUtil.ColorHexString(255, 0, 255),
                BorderColor = ColorUtil.ColorHexString(255, 0, 255),
                Fill = false
            };

            var differenceDepartures = new LineDataset<double>
            {
                Label = "Разница в отправлении",
                BackgroundColor = ColorUtil.ColorHexString(0, 0, 255),
                BorderColor = ColorUtil.ColorHexString(0, 0, 255),
                Fill = false
            };

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
                actualArrivals.Add(movement.RealArrivalTime.ToOADate());
                scheduledDepartures.Add(movement.ScheduleDepartureTime.ToOADate());
                actualDepartures.Add(movement.RealDepartureTime.ToOADate());
                differenceArrival.Add(movement.DifferenceArrival.TotalMinutes);
                differenceDepartures.Add(movement.DifferenceDeparture.TotalMinutes);
            }

            _arr.Data.Datasets.Add(scheduledArrivals);
            _arr.Data.Datasets.Add(actualArrivals);
            _dep.Data.Datasets.Add(scheduledDepartures);
            _dep.Data.Datasets.Add(actualDepartures);

            _diff.Data.Datasets.Add(differenceArrival);
            _diff.Data.Datasets.Add(differenceDepartures);

        }

    }
}
