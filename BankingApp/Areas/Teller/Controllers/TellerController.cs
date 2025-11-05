using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Areas.Teller.Controllers
{
    [Authorize(Roles = "TELLER")]

    public class TellerController : Controller
    {
        public IActionResult Home() { return View(); }
        public IActionResult Deposit() { return View(); }
        public IActionResult Withdrawal() { return View(); ; }
        public IActionResult CreditCardPayment() { return View(); ; }
        public IActionResult LoanPayment() { return View(); ; }
        public IActionResult ThirdPartyTransactions() { return View(); }
    }
}
