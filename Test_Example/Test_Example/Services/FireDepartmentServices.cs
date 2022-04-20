using System.Data.SqlClient;
using Test_Example.DTOs;
using Test_Example.Models;

namespace Test_Example.Services
{
    public class FireDepartmentServices : IFireDepartmentServices
    {
        public async Task<bool> CheckIfFiretruckExistsAsync(int idFireTruck, SqlCommand com)
        {
            com.CommandText = "select * from FireTruck where IdFireTruck = @IdFireTruck";
            com.Parameters.AddWithValue("IdFireTruck", idFireTruck);

            using (var dr = await com.ExecuteReaderAsync())
            {
                if (await dr.ReadAsync())
                {
                    return true;
                }
            }
            return false;
        }
        public async Task<FireTruck> GetFireTruckAsync(SqlCommand com)
        {
            var Firetruck = new FireTruck();

            using (var dr = await com.ExecuteReaderAsync())
            {
                while (await dr.ReadAsync())
                {
                    Firetruck = new FireTruck
                    {
                        IdFireTruck = int.Parse(dr["IdFireTruck"].ToString()),
                        OperationalNumber = dr["OperationalNumber"].ToString(),
                        SpecialEquipment = (bool)dr["SpecialEquipment"]
                    };
                }
            }
            return Firetruck;
        }
        public async Task<List<ActionFiremenDTO>> GetActionsFiremenDTOAsync(SqlCommand com)
        {
            var ActionFiremenDTO = new List<ActionFiremenDTO>();

            com.Parameters.Clear();
            com.CommandText = "select a.IdAction, a.StartTime, a.EndTime, fta.AssignmentDate, ffNumber.Firefighters_Number " +
                              "from FireTruck_Action fta, Action a, (" +
                                                                     "select IdAction, COUNT(IdAction) as Firefighters_Number " +
                                                                     "from Firefighter_Action " +
                                                                     "group by IdAction) ffNumber " +
                              "where ffNumber.IdAction = a.IdAction and fta.IdAction = a.IdAction and fta.IdFireTruck = @IdFireTruck";
            using (var dr = await com.ExecuteReaderAsync())
            {

                while (await dr.ReadAsync())
                {
                    ActionFiremenDTO.Add(new ActionFiremenDTO
                    {
                        StartTime = DateTime.Parse(dr["StartTime"].ToString()),
                        EndTime = DateTime.Parse(dr["EndTime"].ToString()),
                        NumberOfFireMen = int.Parse(dr["Firefighters_Number"].ToString()),
                        AssignmentDate = DateTime.Parse(dr["AssignmentDate"].ToString())
                    });
                }
            }
            return ActionFiremenDTO;
        }
        public async Task<bool> CheckIfActionExistsAsync(Models.Action action, SqlCommand com)
        {
            com.CommandText = "select * from Action where IdAction = @IdAction";
            com.Parameters.AddWithValue("IdAction", action.IdAction);

            using (var dr = await com.ExecuteReaderAsync())
            {
                if (await dr.ReadAsync())
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> CheckIfActionUpdatePossibleAsync(Models.Action action, SqlCommand com)
        {
            com.Parameters.Clear();
            com.CommandText = "select StartTime, EndTime from Action where IdAction = @IdAction";
            com.Parameters.AddWithValue("IdAction", action.IdAction);

            using (var dr = await com.ExecuteReaderAsync())
            {
                while (await dr.ReadAsync())
                {
                    if (DateTime.Parse(dr["EndTime"].ToString()) < DateTime.Now ||
                        action.EndTime < DateTime.Parse(dr["StartTime"].ToString()))
                        return false;
                }
            }
            return true;
        }

        public async void UpdateActionEndTimeAsync(Models.Action action, SqlCommand com)
        {
            com.Parameters.Clear();
            com.CommandText = "update Action set EndTime = @EndTime where IdAction = @IdAction";
            com.Parameters.AddWithValue("EndTime", action.EndTime);
            com.Parameters.AddWithValue("IdAction", action.IdAction);
            await com.ExecuteReaderAsync();
        }
    }
}