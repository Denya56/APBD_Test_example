using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;
using System.Data.SqlClient;
using Test_Example.Models;

namespace Test_Example.Controllers
{
    [Route("api/Actions")]
    [ApiController]
    public class Actions : ControllerBase
    {
        [HttpPut("{idAction}/end-time")]
        public async Task<IActionResult> PutActionEndTime(ActionDTO action)
        {
            using var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=s22247;Integrated Security=True");
            using var com = new SqlCommand("select count(IdAction) as countAction from Action where IdAction = @IdAction", con);
            com.Parameters.AddWithValue("IdAction", action.IdAction);

            await con.OpenAsync();
            DbTransaction tran = await con.BeginTransactionAsync();
            com.Transaction = (SqlTransaction)tran;

            try
            {
                // checking if action exists
                using (var dr = await com.ExecuteReaderAsync())
                {
                    while(await dr.ReadAsync())
                    {
                        if (dr["countAction"].ToString().Equals("0"))
                            return NotFound();
                    }
                }

                // checking if action is finished or new EndTime is earlier than current StartTime
                com.Parameters.Clear();
                com.CommandText = "select StartTime, EndTime from Action where IdAction = @IdAction";
                com.Parameters.AddWithValue("IdAction", action.IdAction);

                using (var dr = await com.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        if (DateTime.Parse(dr["EndTime"].ToString()) < DateTime.Now || 
                            action.EndTime < DateTime.Parse(dr["StartTime"].ToString()))
                            return BadRequest();
                    }
                }

                // updating EndTime
                com.Parameters.Clear();
                com.CommandText = "update Action set EndTime = @EndTime where IdAction = @IdAction";
                com.Parameters.AddWithValue("EndTime", action.EndTime);
                com.Parameters.AddWithValue("IdAction", action.IdAction);
                await com.ExecuteReaderAsync();
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
            }
            return Ok();
        }
    }
}
