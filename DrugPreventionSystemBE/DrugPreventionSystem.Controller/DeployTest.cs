using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("API Working!");
    }
}
//https://deploy-webapi-gvashjepf9hnc8bp.australiaeast-01.azurewebsites.net/api/test
