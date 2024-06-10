namespace Trains.Models
{
    public class TrainRoute
    {
        public int Id { get; set; }
        public Station Station { get; set; }
        public Train Train { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
    }
}
