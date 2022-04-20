using Microsoft.AspNetCore.Mvc;
using System.Data.Common;
using System.Data.SqlClient;
using Test_Example.Services;

namespace Test_Example.Controllers
{
    [Route("api/Actions")]
    [ApiController]
    public class ActionsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private IFireDepartmentServices _fireDepartmentServices;
        public ActionsController(IConfiguration configuration, FireDepartmentServices fireDepartmentServices)
        {
            _configuration = configuration;
            _fireDepartmentServices = fireDepartmentServices;
        }

        [HttpPut("{idAction}/end-time")]
        public async Task<IActionResult> PutActionEndTime(Models.Action action)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("DefaultDbCon"));
            using var com = new SqlCommand("", con);


            await con.OpenAsync();
            DbTransaction tran = await con.BeginTransactionAsync();
            com.Transaction = (SqlTransaction)tran;

            try
            {
                if(!await _fireDepartmentServices.CheckIfActionExistsAsync(action, com))
                    return NotFound("Action not found");

                if (!await _fireDepartmentServices.CheckIfActionUpdatePossibleAsync(action, com))
                    return BadRequest();

                _fireDepartmentServices.UpdateActionEndTimeAsync(action, com);

                await tran.CommitAsync();
            }
            catch (SqlException)
            {
                await tran.RollbackAsync();
            }
            return Ok();
        }
    }
}
