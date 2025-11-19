using AutoMapper;
using BankingApp.Core.Application.Dtos.Transaction.Transference;
using BankingApp.Core.Application.Interfaces;
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
        private readonly ITransacctionRepository transacctionRepository;
        private readonly IMapper _mapper;

        public TransferencesController(IUserAccountManagementService userAccountManagement, IMapper mapper, ITransacctionRepository transacctionRepository)
        {
            _userAccountManagement = userAccountManagement;
            _mapper = mapper;
            this.transacctionRepository = transacctionRepository;
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
                var operationId = transacctionRepository.GenerateOperationId();
                await transacctionRepository.AddAsync(new Transaction
                {
                    //registrar transferencia en caso de ser rechazada
                    Id = Guid.NewGuid(),
                    AccountNumber = vm.AccountNumberFrom!,
                    Beneficiary = vm.AccountNumberTo!,
                    Type = TransactionType.DEBIT,
                    Origin = vm.AccountNumberFrom!,
                    Amount = vm.Amount,
                    Description = DescriptionTransaction.TRANSFER,
                    Status = OperationStatus.DECLINED,
                    OperationId = operationId,
                    DateTime = now
                });

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
