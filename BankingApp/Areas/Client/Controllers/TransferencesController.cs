using AutoMapper;
using BankingApp.Core.Application.Dtos.Transaction.Transference;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.Services;
using BankingApp.Core.Application.ViewModels.Transferences;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Entities;
using BankingApp.Infraestructure.Persistence.Repositories;
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
        private readonly ITranferencesService _tranferencesService;

        public TransferencesController(IUserAccountManagementService userAccountManagement, IMapper mapper, ITranferencesService tranferencesService)

        {
            _userAccountManagement = userAccountManagement;
            _mapper = mapper;
            _tranferencesService = tranferencesService;


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
                ModelState.AddModelError(string.Empty, "La cuenta de origen debe ser diferente a la cuenta de destino");
                vm.HasError = true;
                return View("Index", vm);
            }

            var hasEnoughFunds = await _userAccountManagement.AccountHasEnoughFounds(vm.AccountNumberFrom ?? "", vm.Amount);
            if (!hasEnoughFunds)
            {
                ModelState.AddModelError(string.Empty, $"La cuenta {vm.AccountNumberFrom} no tiene fondos suficientes para realizar la transferencia");
                var now = DateTime.Now;
                var transaction = await _tranferencesService.CreateDeclinedTransactionAsync(vm.AccountNumberFrom!, vm.AccountNumberTo!, vm.Amount);


                vm.HasError = true;
                return View("Index", vm);
            }


            var request = _mapper.Map<TransferenceRequestDto>(vm);
            var response = await _userAccountManagement.TransferAmountToAccount(request);

            if (!response.IsSuccessful)
            {
                ModelState.AddModelError(string.Empty, response.Message);
                vm.HasError = true;
                return View("Index", vm);
            }

            TempData["Menssage"] = "Transacción realizada con éxito";
            return RedirectToAction(nameof(Index), new { message = response.Message });
        }
    }
}
