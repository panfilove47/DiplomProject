namespace Trains.Models.DTO
{
    public class TrainMovementsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string To { get; set; }
        public DateTime ScheduleDepartureTime { get; set; }
        public DateTime ScheduleArrivalTime { get; set; }
        public DateTime RealDepartureTime { get; set; }
        public DateTime RealArrivalTime { get; set;}
        public TimeSpan DifferenceDeparture {  get; set; }
        public TimeSpan DifferenceArrival { get; set; }
    }
}
