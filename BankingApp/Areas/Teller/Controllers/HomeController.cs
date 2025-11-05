using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Areas.Teller.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
