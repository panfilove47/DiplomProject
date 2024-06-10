namespace Trains.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public Station Station { get; set; }
        public Train Train { get; set; }

    }
}
