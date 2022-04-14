using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Test_Example.Models;

namespace Test_Example.Controllers
{
    [Route("api/FireTrucks")]
    [ApiController]
    public class FireTrucksController : ControllerBase
    {
        [HttpGet("{idFireTruck}")]
        public IActionResult GetFireTrucks(int idFireTruck)
        {
            var fireTruckActionDTO = new HashSet<FireTruckActionDTO>();
            using (var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=s22247;Integrated Security=True"))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "select ft.IdFireTruck, ft.OperationalNumber, ft.SpecialEquipment, " +
                                  "a.StartTime, a.EndTime, fta.AssignmentDate, ffNumber.Firefighters_Number " +
                                  "from FireTruck_Action fta, FireTruck ft, Action a, (" +
                                                                                        "select IdAction, COUNT(IdAction) as Firefighters_Number " +
                                                                                        "from Firefighter_Action " +
                                                                                        "group by IdAction) ffNumber " +
                                  "where ffNumber.IdAction = a.IdAction and fta.IdAction = a.IdAction and ft.IdFireTruck = @IdFireTruck";
                com.Parameters.AddWithValue("IdFireTruck", idFireTruck);

                con.Open();
                var dr = com.ExecuteReader();
                while (dr.Read())
                {
                    fireTruckActionDTO.Add(new FireTruckActionDTO
                    {
                        FireTruck = new FireTruck
                        {
                            IdFireTruck = int.Parse(dr["IdFireTruck"].ToString()),
                            OperationalNumber = dr["OperationalNumber"].ToString(),
                            SpecialEquipment = dr["SpecialEquipment"].ToString().Equals("False") ? 0 : 1
                        },
                        Actions = new List<ActionFiremenDTO>
                        {
                            new ActionFiremenDTO
                            {
                                StartTime = DateTime.Parse(dr["StartTime"].ToString()),
                                EndTime = DateTime.Parse(dr["EndTime"].ToString()),
                                NumberOfFireMen = int.Parse(dr["Firefighters_Number"].ToString())
                            }
                        },
                        AssignmentDate = DateTime.Parse(dr["AssignmentDate"].ToString())
                        
                    });
                    return Ok(fireTruckActionDTO);
                }
            }
            return StatusCode(StatusCodes.Status404NotFound);
        }
    }
}
