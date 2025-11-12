using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Areas.Client.Controllers
{

    [Authorize(Roles = "CLIENT")]
    [Area("Client")]
    public class TransactionToBeneficiaryController : Controller
    {
        public IActionResult Create()
        {
            return View();
        }
    }
}
