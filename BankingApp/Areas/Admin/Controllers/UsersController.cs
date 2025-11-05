using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Areas.Admin.Controllers
{
    [Authorize(Roles ="ADMIN") ]
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
