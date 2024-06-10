

using Trains.Models.DTO;

namespace Trains.Interfaces
{
    public interface ITrainRepository
    {
        Task<IEnumerable<TrainDto>> GetTrains();

        Task<TrainDto> GetTrainById(int id);

        Task<IEnumerable<TrainMovementsDto>> GetMovementsById(int id);
    }
}
