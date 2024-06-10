using ChartJs.Blazor.Common;
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

        private PieConfig _config;

        protected override void OnInitialized()
        {
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
            _config = new PieConfig();

            _config.Options = new PieOptions
            {
                Responsive = true,
                Title = new OptionsTitle
                {
                    Display = true,
                    Text = "Zalupa"
                }
            };

            foreach (var party in new[] {"Party1", "Party2", "Party3"})
            {
                _config.Data.Labels.Add(party);
            }

            var dataset = new PieDataset<int>(new[] { 35, 50, 15 })
            {
                BackgroundColor = new[]
                {
                    ColorUtil.ColorHexString(255, 0, 0),
                    ColorUtil.ColorHexString(0, 255, 0),
                    ColorUtil.ColorHexString(0, 0, 255)
                }
            };

            _config.Data.Datasets.Add(dataset);
        }

    }
}
