using Microsoft.AspNetCore.Mvc;
using Test_Example.DTOs;
using Test_Example.Exceptions;
using Test_Example.Services;

namespace Test_Example.Controllers
{
    [Route("api/FireTrucks")]
    [ApiController]
    public class FireTrucksController : ControllerBase
    {
        private IFireDepartmentServices _fireDepartmentServices;

        public FireTrucksController(IConfiguration configuration, FireDepartmentServices fireDepartmentServices)
        {
            _fireDepartmentServices = fireDepartmentServices;
        }

        [HttpGet("{idFireTruck}")]
        public async Task<IActionResult> GetFireTruck(int idFireTruck)
        {
            var FireTruckActionDTO = new FireTruckActionDTO();
            try
            {

                FireTruckActionDTO = await _fireDepartmentServices.GetFireTruckActionDTOAsync(idFireTruck);
            }
            catch(NotFoundException)
            {
                return NotFound("Firetruck not found");
            }
            
            return Ok(FireTruckActionDTO);
        }
    }
}
