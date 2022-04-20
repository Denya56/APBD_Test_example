using Microsoft.AspNetCore.Mvc;
using System.Data.Common;
using System.Data.SqlClient;
using Test_Example.DTOs;
using Test_Example.Services;

namespace Test_Example.Controllers
{
    [Route("api/FireTrucks")]
    [ApiController]
    public class FireTrucksController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private IFireDepartmentServices _fireDepartmentServices;

        public FireTrucksController(IConfiguration configuration, FireDepartmentServices fireDepartmentServices)
        {
            _configuration = configuration;
            _fireDepartmentServices = fireDepartmentServices;
        }

        [HttpGet("{idFireTruck}")]
        public async Task<IActionResult> GetFireTruck(int idFireTruck)
        {
            var FireTruckActionDTO = new HashSet<FireTruckActionDTO>();

            using var con = new SqlConnection(_configuration.GetConnectionString("DefailtDbCon"));
            using var com = new SqlCommand("", con);

            await con.OpenAsync();
            DbTransaction tran = await con.BeginTransactionAsync();
            com.Transaction = (SqlTransaction)tran;

            try
            {
                if (!await _fireDepartmentServices.CheckIfFiretruckExistsAsync(idFireTruck, com))
                    return NotFound("Firetruck not found");

                FireTruckActionDTO.Add(new FireTruckActionDTO
                {
                    FireTruck = await _fireDepartmentServices.GetFireTruckAsync(com),
                    Actions = await _fireDepartmentServices.GetActionsFiremenDTOAsync(com)
                });

                await tran.CommitAsync();
            }
            catch (SqlException)
            {
                await tran.RollbackAsync();
            }
            return Ok(FireTruckActionDTO);
        }
    }
}
