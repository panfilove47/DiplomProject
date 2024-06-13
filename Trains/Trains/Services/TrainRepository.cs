using Dapper;
using MySql.Data.MySqlClient;
using System.Data;
using Trains.Interfaces;
using Trains.Models;
using Trains.Models.DTO;


namespace Trains.Services
{
    public class TrainRepository : ITrainRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public TrainRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<TrainDto>> GetTrains()
        {
            using (IDbConnection db = new MySqlConnection(_connectionString))
            {
                var sql = @"select t.idtrain as Id,
                          t.Name as Name,
                          tt.Type as Type from train t join traintype tt on idtraintype = traintype_idtraintype";
                var result = await db.QueryAsync<TrainDto, TrainType, TrainDto>(
                    sql,
                    (train, traintype) =>
                    {
                        train.TrainType = traintype.Type;
                        return train;
                    },
                    splitOn: "Type");
                return result;
            }
        }

        public async Task<TrainDto> GetTrainById(int id)
        {
            using (IDbConnection db = new MySqlConnection(_connectionString))
            {
                var sql = @"select t.idtrain as Id,
                          t.Name as Name,
                          tt.Type as Type from train t join traintype tt on idtraintype = traintype_idtraintype where t.idTrain = @id";
                var result = await db.QueryAsync<TrainDto, TrainType, TrainDto>(
                sql,
                (train, traintype) =>
                {
                    train.TrainType = traintype.Type;
                    return train;
                },
                new { id },
                splitOn: "Type"
                );
                return result.FirstOrDefault();
            }
        }

        public async Task<IEnumerable<TrainMovementsDto>> GetMovementsById(int id, DateTime? From = null, DateTime? To = null)
        {
            string query = default;
            IEnumerable<TrainMovementsDto> result;
            using (IDbConnection db = new MySqlConnection(_connectionString))
            {
                if (From == null && To == null)
                {
                    query = @"select t.idtrain as 'Id', t.name as 'Name', concat(s.city, ""-"", s.name) as 'To', sc.DepartureTime as 'ScheduleDepartureTime', sc.ArrivalTime as 'ScheduleArrivalTime', tr.DepartureTime as 'RealDepartureTime', tr.ArrivalTime as 'RealArrivalTime' from train t
join  schedule sc on t.idtrain = sc.Train_idTrain
join trainroute tr on t.idtrain = tr.Train_idTrain
join Station s on s.idStation = sc.station_idStation and s.idStation = tr.station_idStation where t.Idtrain = @Id order by sc.DepartureTime";
                    result = await db.QueryAsync<TrainMovementsDto>(query, new { Id = id });
                }
                else if (To != null)
                {
                    query = @"select t.idtrain as 'Id', t.name as 'Name', concat(s.city, ""-"", s.name) as 'To', sc.DepartureTime as 'ScheduleDepartureTime', sc.ArrivalTime as 'ScheduleArrivalTime', tr.DepartureTime as 'RealDepartureTime', tr.ArrivalTime as 'RealArrivalTime' from train t
join  schedule sc on t.idtrain = sc.Train_idTrain
join trainroute tr on t.idtrain = tr.Train_idTrain
join Station s on s.idStation = sc.station_idStation and s.idStation = tr.station_idStation where t.Idtrain = @Id and sc.DepartureTime < @To order by sc.DepartureTime";
                    result = await db.QueryAsync<TrainMovementsDto>(query, new { Id = id, To });
                }
                else if (From != null)
                {
                    query = @"select t.idtrain as 'Id', t.name as 'Name', concat(s.city, ""-"", s.name) as 'To', sc.DepartureTime as 'ScheduleDepartureTime', sc.ArrivalTime as 'ScheduleArrivalTime', tr.DepartureTime as 'RealDepartureTime', tr.ArrivalTime as 'RealArrivalTime' from train t
join  schedule sc on t.idtrain = sc.Train_idTrain
join trainroute tr on t.idtrain = tr.Train_idTrain
join Station s on s.idStation = sc.station_idStation and s.idStation = tr.station_idStation where t.Idtrain = @Id and sc.DepartureTime > @From order by sc.DepartureTime";
                    result = await db.QueryAsync<TrainMovementsDto>(query, new { Id = id, From });
                } 
                else
                {
                    query = @"select t.idtrain as 'Id', t.name as 'Name', concat(s.city, ""-"", s.name) as 'To', sc.DepartureTime as 'ScheduleDepartureTime', sc.ArrivalTime as 'ScheduleArrivalTime', tr.DepartureTime as 'RealDepartureTime', tr.ArrivalTime as 'RealArrivalTime' from train t
join  schedule sc on t.idtrain = sc.Train_idTrain
join trainroute tr on t.idtrain = tr.Train_idTrain
join Station s on s.idStation = sc.station_idStation and s.idStation = tr.station_idStation where t.Idtrain = @Id and sc.DepartureTime between @From and @To order by sc.DepartureTime";
                    result = await db.QueryAsync<TrainMovementsDto>(query, new { Id = id, To, From });
                }

                //result = await db.QueryAsync<TrainMovementsDto>(query, new { Id = id, from = From });

                foreach (var movement in result)
                {
                    movement.DifferenceDeparture = movement.RealDepartureTime - movement.ScheduleDepartureTime;
                    movement.DifferenceArrival = movement.RealArrivalTime - movement.ScheduleArrivalTime;
                }

                return result;
            }
        }
    }
}
