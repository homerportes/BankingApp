using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
