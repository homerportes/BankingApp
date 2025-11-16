using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.ViewModels.Teller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankingApp.Areas.Teller.Controllers
{
    [Authorize(Roles = "TELLER")]
    [Area("Teller")]
    public class TellerController : Controller
    {
        private readonly ITellerService _tellerService;

        public TellerController(ITellerService tellerService)
        {
            _tellerService = tellerService;
        }

        #region Deposit

        [HttpGet]
        public IActionResult Deposit()
        {
            return View(new DepositViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Deposit(DepositViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var tellerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var validationResult = await _tellerService.ValidateAccountForDepositAsync(model.AccountNumber);

            if (!validationResult.IsValid)
            {
                ModelState.AddModelError(string.Empty, validationResult.Message);
                return View(model);
            }

            TempData["DepositModel"] = System.Text.Json.JsonSerializer.Serialize(model);
            TempData["AccountHolderName"] = validationResult.AccountHolderName;
            TempData["TellerId"] = tellerId;
            return RedirectToAction(nameof(ConfirmDeposit));
        }

        [HttpGet]
        public IActionResult ConfirmDeposit()
        {
            if (TempData["DepositModel"] == null)
            {
                return RedirectToAction(nameof(Deposit));
            }

            var modelJson = TempData["DepositModel"]?.ToString();
            var model = System.Text.Json.JsonSerializer.Deserialize<DepositViewModel>(modelJson ?? "");
            
            // Mantener los datos para el POST
            TempData.Keep("DepositModel");
            TempData.Keep("TellerId");
            TempData.Keep("AccountHolderName");
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmDeposit(bool confirm)
        {
            if (!confirm)
            {
                // Si cancela, limpiar TempData y regresar
                TempData.Remove("DepositModel");
                TempData.Remove("TellerId");
                TempData.Remove("AccountHolderName");
                return RedirectToAction(nameof(Deposit));
            }

            if (TempData["DepositModel"] == null)
            {
                TempData["ErrorMessage"] = "Los datos del depósito se han perdido. Por favor, intente nuevamente.";
                return RedirectToAction(nameof(Deposit));
            }

            var modelJson = TempData["DepositModel"]?.ToString();
            var tellerId = TempData["TellerId"]?.ToString() ?? string.Empty;
            
            DepositViewModel? model = null;
            try
            {
                model = System.Text.Json.JsonSerializer.Deserialize<DepositViewModel>(modelJson ?? "");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al deserializar el modelo: {ex.Message}";
                return RedirectToAction(nameof(Deposit));
            }

            // Limpiar TempData inmediatamente para evitar resubmit
            TempData.Remove("DepositModel");
            TempData.Remove("TellerId");
            TempData.Remove("AccountHolderName");

            if (model == null)
            {
                TempData["ErrorMessage"] = "El modelo de depósito es nulo.";
                return RedirectToAction(nameof(Deposit));
            }

            var result = await _tellerService.ProcessDepositAsync(model, tellerId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Index", "Home");
            }

            TempData["ErrorMessage"] = result.Message;
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        #endregion

        #region Withdrawal

        [HttpGet]
        public IActionResult Withdrawal()
        {
            return View(new WithdrawalViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Withdrawal(WithdrawalViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var tellerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var validationResult = await _tellerService.ValidateAccountForWithdrawalAsync(model.AccountNumber);

            if (!validationResult.IsValid)
            {
                ModelState.AddModelError(string.Empty, validationResult.Message);
                return View(model);
            }

            TempData["WithdrawalModel"] = System.Text.Json.JsonSerializer.Serialize(model);
            TempData["AccountHolderName"] = validationResult.AccountHolderName;
            TempData["Balance"] = validationResult.Balance.ToString("F2");
            TempData["TellerId"] = tellerId;
            return RedirectToAction(nameof(ConfirmWithdrawal));
        }

        [HttpGet]
        public IActionResult ConfirmWithdrawal()
        {
            if (TempData["WithdrawalModel"] == null)
            {
                return RedirectToAction(nameof(Withdrawal));
            }

            var modelJson = TempData["WithdrawalModel"]?.ToString();
            var model = System.Text.Json.JsonSerializer.Deserialize<WithdrawalViewModel>(modelJson ?? "");
            
            // Mantener los datos para el POST
            TempData.Keep("WithdrawalModel");
            TempData.Keep("TellerId");
            TempData.Keep("AccountHolderName");
            TempData.Keep("Balance");
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmWithdrawal(bool confirm)
        {
            if (!confirm)
            {
                // Si cancela, limpiar TempData y regresar
                TempData.Remove("WithdrawalModel");
                TempData.Remove("TellerId");
                TempData.Remove("AccountHolderName");
                TempData.Remove("Balance");
                return RedirectToAction(nameof(Withdrawal));
            }

            if (TempData["WithdrawalModel"] == null)
            {
                return RedirectToAction(nameof(Withdrawal));
            }

            var modelJson = TempData["WithdrawalModel"]?.ToString();
            var tellerId = TempData["TellerId"]?.ToString() ?? string.Empty;
            var model = System.Text.Json.JsonSerializer.Deserialize<WithdrawalViewModel>(modelJson ?? "");

            // Limpiar TempData inmediatamente para evitar resubmit
            TempData.Remove("WithdrawalModel");
            TempData.Remove("TellerId");
            TempData.Remove("AccountHolderName");
            TempData.Remove("Balance");

            if (model == null)
            {
                return RedirectToAction(nameof(Withdrawal));
            }

            var result = await _tellerService.ProcessWithdrawalAsync(model, tellerId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        #endregion

        #region Credit Card Payment

        [HttpGet]
        public IActionResult CreditCardPayment()
        {
            return View(new CreditCardPaymentViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreditCardPayment(CreditCardPaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var tellerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var validationResult = await _tellerService.ValidateCreditCardPaymentAsync(model.AccountNumber, model.CardNumber);

            if (!validationResult.IsValid)
            {
                ModelState.AddModelError(string.Empty, validationResult.Message);
                return View(model);
            }

            model.CurrentDebt = validationResult.CurrentDebt;
            model.ActualAmountToPay = Math.Min(model.Amount, validationResult.CurrentDebt);

            TempData["CreditCardPaymentModel"] = System.Text.Json.JsonSerializer.Serialize(model);
            TempData["CardHolderName"] = validationResult.CardHolderName;
            TempData["TellerId"] = tellerId;
            return RedirectToAction(nameof(ConfirmCreditCardPayment));
        }

        [HttpGet]
        public IActionResult ConfirmCreditCardPayment()
        {
            if (TempData["CreditCardPaymentModel"] == null)
            {
                return RedirectToAction(nameof(CreditCardPayment));
            }

            var modelJson = TempData["CreditCardPaymentModel"]?.ToString();
            var model = System.Text.Json.JsonSerializer.Deserialize<CreditCardPaymentViewModel>(modelJson ?? "");
            
            // Mantener los datos para el POST
            TempData.Keep("CreditCardPaymentModel");
            TempData.Keep("TellerId");
            TempData.Keep("CardHolderName");
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmCreditCardPayment(bool confirm)
        {
            if (!confirm)
            {
                // Si cancela, limpiar TempData y regresar
                TempData.Remove("CreditCardPaymentModel");
                TempData.Remove("TellerId");
                TempData.Remove("CardHolderName");
                return RedirectToAction(nameof(CreditCardPayment));
            }

            if (TempData["CreditCardPaymentModel"] == null)
            {
                TempData["ErrorMessage"] = "Los datos del pago se han perdido. Por favor, intente nuevamente.";
                return RedirectToAction(nameof(CreditCardPayment));
            }

            var modelJson = TempData["CreditCardPaymentModel"]?.ToString();
            var tellerId = TempData["TellerId"]?.ToString() ?? string.Empty;
            
            CreditCardPaymentViewModel? model = null;
            try
            {
                model = System.Text.Json.JsonSerializer.Deserialize<CreditCardPaymentViewModel>(modelJson ?? "");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al procesar los datos: {ex.Message}";
                return RedirectToAction(nameof(CreditCardPayment));
            }

            if (model == null)
            {
                TempData["ErrorMessage"] = "El modelo de pago es nulo. Por favor, intente nuevamente.";
                return RedirectToAction(nameof(CreditCardPayment));
            }

            // Procesar el pago
            var result = await _tellerService.ProcessCreditCardPaymentAsync(model, tellerId);

            // Limpiar TempData después de procesar
            TempData.Remove("CreditCardPaymentModel");
            TempData.Remove("TellerId");
            TempData.Remove("CardHolderName");

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Index", "Home");
            }

            // Si hubo error, mostrar el mensaje específico
            TempData["ErrorMessage"] = result.Message;
            model.HasError = true;
            model.ErrorMessage = result.Message;
            return View(model);
        }

        #endregion

        #region Loan Payment

        [HttpGet]
        public IActionResult LoanPayment()
        {
            return View(new LoanPaymentViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> LoanPayment(LoanPaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var tellerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var validationResult = await _tellerService.ValidateLoanPaymentAsync(model.AccountNumber, model.LoanNumber);

            if (!validationResult.IsValid)
            {
                ModelState.AddModelError(string.Empty, validationResult.Message);
                return View(model);
            }

            model.RemainingBalance = validationResult.RemainingBalance;

            TempData["LoanPaymentModel"] = System.Text.Json.JsonSerializer.Serialize(model);
            TempData["LoanHolderName"] = validationResult.LoanHolderName;
            TempData["TellerId"] = tellerId;
            return RedirectToAction(nameof(ConfirmLoanPayment));
        }

        [HttpGet]
        public IActionResult ConfirmLoanPayment()
        {
            if (TempData["LoanPaymentModel"] == null)
            {
                return RedirectToAction(nameof(LoanPayment));
            }

            var modelJson = TempData["LoanPaymentModel"]?.ToString();
            var model = System.Text.Json.JsonSerializer.Deserialize<LoanPaymentViewModel>(modelJson ?? "");
            
            // Mantener los datos para el POST
            TempData.Keep("LoanPaymentModel");
            TempData.Keep("TellerId");
            TempData.Keep("LoanHolderName");
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmLoanPayment(bool confirm)
        {
            if (!confirm)
            {
                // Si cancela, limpiar TempData y regresar
                TempData.Remove("LoanPaymentModel");
                TempData.Remove("TellerId");
                TempData.Remove("LoanHolderName");
                return RedirectToAction(nameof(LoanPayment));
            }

            if (TempData["LoanPaymentModel"] == null)
            {
                return RedirectToAction(nameof(LoanPayment));
            }

            var modelJson = TempData["LoanPaymentModel"]?.ToString();
            var tellerId = TempData["TellerId"]?.ToString() ?? string.Empty;
            var model = System.Text.Json.JsonSerializer.Deserialize<LoanPaymentViewModel>(modelJson ?? "");

            // Limpiar TempData inmediatamente para evitar resubmit
            TempData.Remove("LoanPaymentModel");
            TempData.Remove("TellerId");
            TempData.Remove("LoanHolderName");

            if (model == null)
            {
                return RedirectToAction(nameof(LoanPayment));
            }

            var result = await _tellerService.ProcessLoanPaymentAsync(model, tellerId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        #endregion

        #region Third Party Transactions

        [HttpGet]
        public IActionResult ThirdPartyTransactions()
        {
            return View(new ThirdPartyTransactionViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ThirdPartyTransactions(ThirdPartyTransactionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var tellerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var validationResult = await _tellerService.ValidateThirdPartyTransactionAsync(model.SourceAccountNumber, model.DestinationAccountNumber);

            if (!validationResult.IsValid)
            {
                ModelState.AddModelError(string.Empty, validationResult.Message);
                return View(model);
            }

            TempData["ThirdPartyTransactionModel"] = System.Text.Json.JsonSerializer.Serialize(model);
            TempData["DestinationAccountHolderName"] = validationResult.DestinationAccountHolderName;
            TempData["TellerId"] = tellerId;
            return RedirectToAction(nameof(ConfirmThirdPartyTransaction));
        }

        [HttpGet]
        public IActionResult ConfirmThirdPartyTransaction()
        {
            if (TempData["ThirdPartyTransactionModel"] == null)
            {
                return RedirectToAction(nameof(ThirdPartyTransactions));
            }

            var modelJson = TempData["ThirdPartyTransactionModel"]?.ToString();
            var model = System.Text.Json.JsonSerializer.Deserialize<ThirdPartyTransactionViewModel>(modelJson ?? "");
            
            // Mantener los datos para el POST
            TempData.Keep("ThirdPartyTransactionModel");
            TempData.Keep("TellerId");
            TempData.Keep("DestinationAccountHolderName");
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmThirdPartyTransaction(bool confirm)
        {
            if (!confirm)
            {
                // Si cancela, limpiar TempData y regresar
                TempData.Remove("ThirdPartyTransactionModel");
                TempData.Remove("TellerId");
                TempData.Remove("DestinationAccountHolderName");
                return RedirectToAction(nameof(ThirdPartyTransactions));
            }

            if (TempData["ThirdPartyTransactionModel"] == null)
            {
                return RedirectToAction(nameof(ThirdPartyTransactions));
            }

            var modelJson = TempData["ThirdPartyTransactionModel"]?.ToString();
            var tellerId = TempData["TellerId"]?.ToString() ?? string.Empty;
            var model = System.Text.Json.JsonSerializer.Deserialize<ThirdPartyTransactionViewModel>(modelJson ?? "");

            // Limpiar TempData inmediatamente para evitar resubmit
            TempData.Remove("ThirdPartyTransactionModel");
            TempData.Remove("TellerId");
            TempData.Remove("DestinationAccountHolderName");

            if (model == null)
            {
                return RedirectToAction(nameof(ThirdPartyTransactions));
            }

            var result = await _tellerService.ProcessThirdPartyTransactionAsync(model, tellerId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        #endregion
    }
}
