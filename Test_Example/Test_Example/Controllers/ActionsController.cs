using Microsoft.AspNetCore.Mvc;
using Test_Example.Exceptions;
using Test_Example.Services;

namespace Test_Example.Controllers
{
    [Route("api/Actions")]
    [ApiController]
    public class ActionsController : ControllerBase
    {
        private IFireDepartmentServices _fireDepartmentServices;

        public ActionsController(IFireDepartmentServices fireDepartmentServices)
        { 
            _fireDepartmentServices = fireDepartmentServices;
        }

        [HttpPut("{idAction}/end-time")]
        public async Task<IActionResult> PutActionEndTime(Models.Action action)
        {
            try
            {
                await _fireDepartmentServices.PutActionEndTime(action);
            }
            catch (NotFoundException)
            {
                return NotFound("Action not found");
            }
            catch(BadRequestException)
            {
                return BadRequest("EndTime cannot be earlier than StartTime " +
                    "and/or cannot modify action which already has finished");
            }
            return Ok();
        }
    }
}
