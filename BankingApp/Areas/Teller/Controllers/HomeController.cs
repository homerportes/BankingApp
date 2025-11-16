using BankingApp.Core.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankingApp.Areas.Teller.Controllers
{
    [Area("Teller")]
    [Authorize(Roles = "TELLER")]
    public class HomeController : Controller
    {
        private readonly ITellerService _tellerService;

        public HomeController(ITellerService tellerService)
        {
            _tellerService = tellerService;
        }

        public async Task<IActionResult> Index()
        {
            var tellerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var dashboardData = await _tellerService.GetTellerDashboardDataAsync(tellerId);
            return View(dashboardData);
        }
    }
}
