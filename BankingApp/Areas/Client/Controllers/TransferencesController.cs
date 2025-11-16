using AutoMapper;
using BankingApp.Core.Application.Dtos.Transaction.Transference;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.ViewModels.Transferences;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Areas.Client.Controllers
{

    [Area("Client")]
    
    [Authorize(Roles = "CLIENT")]
    public class TransferencesController : Controller
    {
        private readonly IUserAccountManagementService _userAccountManagement;
        private readonly IMapper _mapper;

        public TransferencesController(IUserAccountManagementService userAccountManagement, IMapper mapper)
        {
            _userAccountManagement = userAccountManagement;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(string? message = null)
        {
            var username = User.Identity!.Name ?? "";
            var availableAccounts = await _userAccountManagement.GetCurrentUserActiveAccounts(username);

            var vm = new TransferenceOperationViewModel
            {
                AvailableAccounts = availableAccounts,
                Message = message
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(TransferenceOperationViewModel vm)
        {
            var username = User.Identity!.Name ?? "";
            vm.AvailableAccounts = await _userAccountManagement.GetCurrentUserActiveAccounts(username);

            if (!ModelState.IsValid)
            {
                vm.HasError = true;
                return View("Index", vm);
            }

            if (vm.AccountNumberFrom == vm.AccountNumberTo)
            {
                vm.HasError = true;
                vm.Error = "La cuenta de origen debe ser diferente a la cuenta de destino";
                return View("Index", vm);
            }

            var hasEnoughFunds = await _userAccountManagement.AccountHasEnoughFounds(vm.AccountNumberFrom ?? "", vm.Amount);
            if (!hasEnoughFunds)
            {
                vm.HasError = true;
                vm.Error = $"La cuenta {vm.AccountNumberFrom} no tiene fondos suficientes para realizar la transferencia";
                return View("Index", vm);
            }

            var request = _mapper.Map<TransferenceRequestDto>(vm);
            var response = await _userAccountManagement.TransferAmountToAccount(request);

            if (!response.IsSuccessful)
            {
                vm.HasError = true;
                vm.Error = response.Message;
                return View("Index", vm);
            }

            return RedirectToAction(nameof(Index), new { message = response.Message });
        }
    }
}
