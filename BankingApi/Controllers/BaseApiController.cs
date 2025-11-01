using Microsoft.AspNetCore.Mvc;

namespace BankingApi.Controllers
{
    [Route("api/v{version::apiVersion}/[controller]")]
    [ApiController]
    public class BaseApiController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
