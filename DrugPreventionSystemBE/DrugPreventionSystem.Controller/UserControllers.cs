using DrugPreventionSystemBE.DrugPreventionSystem.ModelView;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;


namespace DrugPreventionSystemBE.DrugPreventionSystem.Controller
{
    [ApiController]
    [Route("/api/user")]
    public class UserControllers : ControllerBase
    {
        public readonly IConfiguration _configuration;
        public UserControllers(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("register")]
        public string register(UserRegisterRequest userRegisterRequest)
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection").ToString());
            SqlCommand cmd = new SqlCommand("sp_RegisterUser", con); // chưa làm xong
            return "";
        }

        private string GetAgeGroup(DateTime dob)
        {
            int age = DateTime.Now.Year - dob.Year;
            if (dob > DateTime.Now.AddYears(-age)) age--;

            if (age < 18) return "Student";
            else if (age < 25) return "CollegeStudent";
            else if (age < 60) return "Parent";
            else return "Senior";
        }
    }
}
