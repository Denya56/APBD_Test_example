using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;
using System.Data.SqlClient;
using Test_Example.Models;

namespace Test_Example.Controllers
{
    [Route("api/FireTrucks")]
    [ApiController]
    public class FireTrucksController : ControllerBase
    {
        [HttpGet("{idFireTruck}")]
        public async Task<IActionResult> GetFireTrucks(int idFireTruck)
        {
            var fireTruckActionDTO = new HashSet<FireTruckActionDTO>();
            var ft = new FireTruck();
            var afDTO = new List<ActionFiremenDTO>();

            using var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=s22247;Integrated Security=True");
            using var com = new SqlCommand("select count(idFireTruck) as countFT from FireTruck where IdFireTruck = @IdFireTruck", con);

            com.Parameters.AddWithValue("IdFireTruck", idFireTruck);

            await con.OpenAsync();
            DbTransaction tran = await con.BeginTransactionAsync();
            com.Transaction = (SqlTransaction)tran;

            try
            {
                // Checking if FireTruck with id = idFireTruck exists
                using (var dr = await com.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        if (dr["countFT"].ToString().Equals("0"))
                            return StatusCode(StatusCodes.Status404NotFound);
                    }
                }

                // Getting FireTruck with id = idFireTruck
                com.Parameters.Clear();
                com.CommandText = "select ft.IdFireTruck, ft.OperationalNumber, ft.SpecialEquipment " +
                                  "from FireTruck ft " +
                                  "where ft.IdFireTruck = @IdFireTruck";
                com.Parameters.AddWithValue("IdFireTruck", idFireTruck);

                using (var dr = await com.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        ft = new FireTruck
                        {
                            IdFireTruck = int.Parse(dr["IdFireTruck"].ToString()),
                            OperationalNumber = dr["OperationalNumber"].ToString(),
                            SpecialEquipment = (bool)dr["SpecialEquipment"]
                        };
                    }
                }

                // getting actions assigned to Firetruck with number of assigned firemen
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
                        afDTO.Add(new ActionFiremenDTO
                        {
                            StartTime = DateTime.Parse(dr["StartTime"].ToString()),
                            EndTime = DateTime.Parse(dr["EndTime"].ToString()),
                            NumberOfFireMen = int.Parse(dr["Firefighters_Number"].ToString()),
                            AssignmentDate = DateTime.Parse(dr["AssignmentDate"].ToString())
                        });
                    }
                }
                fireTruckActionDTO.Add(new FireTruckActionDTO
                {
                    FireTruck = ft,
                    Actions = afDTO
                });

                await tran.CommitAsync();
            }
            catch (SqlException ex)
            {
                await tran.RollbackAsync();
            }
            return Ok(fireTruckActionDTO);
        }
    }
}
