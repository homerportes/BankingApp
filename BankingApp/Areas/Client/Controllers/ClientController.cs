using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Areas.Client.Controllers
{
    [Authorize(Roles = "Client")]

    public class ClientController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Home() { return View(); } // Product list
        public IActionResult Beneficiaries() { return View(); }
        public IActionResult Transactions() { return View(); }
        public IActionResult CashAdvances() { return View(); }
        public IActionResult AccountTransfers() { return View(); }
    }
}
