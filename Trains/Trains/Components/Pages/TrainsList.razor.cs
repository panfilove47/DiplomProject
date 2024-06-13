using Microsoft.AspNetCore.Components;
using Trains.Interfaces;
using Trains.Models.DTO;


namespace Trains.Components.Pages
{
    public partial class TrainsList
    {
        public IEnumerable<TrainDto> Trains { get; set; }
        [Inject]
        public ITrainRepository _trainRepository {  get; set; }

        public int TrainId;

        protected override async Task OnInitializedAsync()
        {
            Trains = await _trainRepository.GetTrains();
        }

        public async Task LoadAsync(CancellationToken ct = default)
        {
            if (TrainId == 0)
            {
                Trains = await _trainRepository.GetTrains();
            }
            else
            {
                Trains = (await _trainRepository.GetTrains()).Where(_ => _.Id == TrainId);
            }
        }

        public void Search()
        {
            LoadAsync();
        }
    }
}
