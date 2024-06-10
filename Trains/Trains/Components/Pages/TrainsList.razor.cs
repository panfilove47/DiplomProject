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

        protected override async Task OnInitializedAsync()
        {
            Trains = await _trainRepository.GetTrains();
        }
    }
}
