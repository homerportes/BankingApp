using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Areas.Client.Controllers
{
    [Authorize(Roles = "CLIENT")]
    [Area("Client")]
    public class TransactionController : Controller
    {
        // no eliminar este controlador
        public IActionResult Index()
        {
            return View();
        }
    }
}
