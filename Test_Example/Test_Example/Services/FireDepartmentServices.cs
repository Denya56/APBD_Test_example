using System.Data.Common;
using System.Data.SqlClient;
using Test_Example.DTOs;
using Test_Example.Exceptions;
using Test_Example.Models;

namespace Test_Example.Services
{
    public class FireDepartmentServices : IFireDepartmentServices
    {
        private readonly IConfiguration _configuration;
        public FireDepartmentServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> CheckIfFiretruckExistsAsync(int idFireTruck, SqlCommand com)
        {
            com.Parameters.Clear();
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
        public async Task<IEnumerable<ActionFiremenDTO>> GetActionsFiremenDTOAsync(SqlCommand com, int idFireTruck)
        {
            var ActionFiremenDTO = new List<ActionFiremenDTO>();

            com.Parameters.Clear();
            com.CommandText = "select a.IdAction, a.StartTime, a.EndTime, fta.AssignmentDate, ffNumber.Firefighters_Number " +
                              "from FireTruck_Action fta, Action a, (" +
                                                                     "select IdAction, COUNT(IdAction) as Firefighters_Number " +
                                                                     "from Firefighter_Action " +
                                                                     "group by IdAction) ffNumber " +
                              "where ffNumber.IdAction = a.IdAction and fta.IdAction = a.IdAction and fta.IdFireTruck = @IdFireTruck";
            com.Parameters.AddWithValue("IdFireTruck", idFireTruck);
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
        public async Task<FireTruckActionDTO> GetFireTruckActionDTOAsync(int idFireTruck)
        {
            var FireTruckActionDTO = new FireTruckActionDTO();

            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultDbCon"));
            using var com = new SqlCommand("", con);

            await con.OpenAsync();
            DbTransaction tran = await con.BeginTransactionAsync();
            com.Transaction = (SqlTransaction)tran;

            try
            {
                if (!await CheckIfFiretruckExistsAsync(idFireTruck, com))
                    throw new NotFoundException();

                FireTruckActionDTO = new FireTruckActionDTO
                {
                    FireTruck = await GetFireTruckAsync(com),
                    Actions = (List<ActionFiremenDTO>)await GetActionsFiremenDTOAsync(com, idFireTruck)
                };

                await tran.CommitAsync();
            }
            catch (SqlException)
            {
                await tran.RollbackAsync();
            }

            return FireTruckActionDTO;

        }
        public async Task<bool> CheckIfActionExistsAsync(Models.Action action, SqlCommand com)
        {
            com.Parameters.Clear();
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

        public async Task UpdateActionEndTimeAsync(Models.Action action, SqlCommand com)
        {
            com.Parameters.Clear();
            com.CommandText = "update Action set EndTime = @EndTime where IdAction = @IdAction";
            com.Parameters.AddWithValue("EndTime", action.EndTime);
            com.Parameters.AddWithValue("IdAction", action.IdAction);
            await com.ExecuteReaderAsync();
        }

        public async Task PutActionEndTime(Models.Action action)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultDbCon"));
            using var com = new SqlCommand("", con);

            await con.OpenAsync();
            DbTransaction tran = await con.BeginTransactionAsync();
            com.Transaction = (SqlTransaction)tran;

            try
            {
                if (!await CheckIfActionExistsAsync(action, com))
                    throw new NotFoundException();

                if (!await CheckIfActionUpdatePossibleAsync(action, com))
                    throw new BadRequestException();

                await UpdateActionEndTimeAsync(action, com);

                await tran.CommitAsync();
            }
            catch (SqlException)
            {
                await tran.RollbackAsync();
            }
        }
    }
}