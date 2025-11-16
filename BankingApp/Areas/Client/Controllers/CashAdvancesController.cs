using AutoMapper;
using BankingApp.Core.Application.Dtos.Email;
using BankingApp.Core.Application.Dtos.Transaction;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.ViewModels.TransactionToCreditCard;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace BankingApp.Areas.Client.Controllers
{
    [Authorize(Roles = "CLIENT")]
    [Area("Client")]
    public class CashAdvancesController : Controller
    {


        private readonly ITransactionService transactionService;
        private readonly ITransactionToCreditCardService transactionToCreditCard;
        private readonly ICashAdvancesServices cashAdvancesServices;
        private readonly UserManager<AppUser> userManager;
        private readonly IMapper mapper;
        private readonly IEmailService emailService;
        private readonly IWebHostEnvironment webHostEnvironment;



        public CashAdvancesController(ITransactionService transactionService,
                                     ITransactionToCreditCardService transactionToCreditCard, 
                                     UserManager<AppUser> userManager, 
                                     IMapper mapper,
                                     IWebHostEnvironment webHostEnvironment, 
                                     IEmailService emailService,
                                     ICashAdvancesServices cashAdvancesServices)
        {
            this.transactionService = transactionService;
            this.transactionToCreditCard = transactionToCreditCard;
            this.userManager = userManager;
            this.mapper = mapper;
            this.webHostEnvironment = webHostEnvironment;
            this.emailService = emailService;
            this.cashAdvancesServices = cashAdvancesServices;
        }





        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await userManager.GetUserAsync(User);
            var entity = new CreateTransactionCashAvancesViewModel()
            {


                Amount = 0,
                CreditCard = "",
                Account = ""

            };

            ViewBag.CuentasAhorros = await transactionService.CuentaListAsync(user!.Id);
            ViewBag.CreditCard = await transactionToCreditCard.GetCreditCardByIdUser(user!.Id);

            return View(entity);
        }



        [HttpPost]
        public async Task<IActionResult> Create(CreateTransactionCashAvancesViewModel vm)
        {
            var user = await userManager.GetUserAsync(User);


            if (!ModelState.IsValid)
            {

                ViewBag.CuentasAhorros = await transactionService.CuentaListAsync(user!.Id);
                ViewBag.CreditCard = await transactionToCreditCard.GetCreditCardByIdUser(user!.Id);
                return View(vm);
            }


            var validateAmount = await transactionService.ValidateAmount(vm.Account, vm.Amount);
            var validateAmountOwedCreditCard = await cashAdvancesServices.ValidateTotalAmountOwedCreditCard(vm.CreditCard, vm.Amount);
            if (validateAmountOwedCreditCard != null && validateAmountOwedCreditCard.HasError == true)
            {

                ModelState.AddModelError(string.Empty, $"{validateAmountOwedCreditCard.Error}");
                //registrar transaccion en caso de ser rechazada
                var Transaccion = mapper.Map<CreateTransactionDto>(vm);
                Transaccion.Status = OperationStatus.DECLINED;
                Transaccion.Type = TransactionType.PURCHASE;
                Transaccion.AccountId =validateAmountOwedCreditCard!.AccountId;
                Transaccion.AccountNumber = vm.CreditCard;
                Transaccion.DateTime = DateTime.UtcNow;
                Transaccion.Description = DescriptionTransaction.Cash_Advance;

                var salvar = await transactionService.AddAsync(Transaccion);
            }






            if (!ModelState.IsValid)
            {
                ViewBag.CuentasAhorros = await transactionService.CuentaListAsync(user!.Id);
                ViewBag.CreditCard = await transactionToCreditCard.GetCreditCardByIdUser(user!.Id);
                return View(vm);
            }




            //registrar la transaccion para la cuenta del cliente
            var TransaccionApproved = mapper.Map<CreateTransactionDto>(vm);
            TransaccionApproved.Status = OperationStatus.APPROVED;
            TransaccionApproved.Type = TransactionType.CREDIT;
            TransaccionApproved.AccountId = validateAmount!.AccounId;
            TransaccionApproved.AccountNumber = vm.Account;
            TransaccionApproved.DateTime = DateTime.UtcNow;
            TransaccionApproved.Description = DescriptionTransaction.Cash_Advance;



            var gualdar = await cashAdvancesServices.AddAsync(TransaccionApproved);
            if (gualdar == null)
            {
                ViewBag.CuentasAhorros = await transactionService.CuentaListAsync(user!.Id);
                ViewBag.CreditCard = await transactionToCreditCard.GetCreditCardByIdUser(user!.Id);
                ModelState.AddModelError(string.Empty, "No se pudo realizan la transaccion");
                return View(vm);

            }


            //registrar transaccion para la tarjeta
            var CreditCardTransaccion = mapper.Map<CreateTransactionDto>(vm);
            CreditCardTransaccion.Status = OperationStatus.APPROVED;
            CreditCardTransaccion.Type = TransactionType.PURCHASE;
            CreditCardTransaccion.AccountId = validateAmount!.AccounId;
            CreditCardTransaccion.AccountNumber = vm.Account;
            CreditCardTransaccion.DateTime = DateTime.UtcNow;
            CreditCardTransaccion.Description = DescriptionTransaction.Cash_Advance;


            var salver = await cashAdvancesServices.AddAsync(CreditCardTransaccion);



            //debitar balance de la cuenta de ahorro
            decimal interest = 1.0625m;
            await transactionService.CreditBalanceAsync(vm.Account, gualdar!.Amount);
            await cashAdvancesServices.CreditTotalAmountOwedAsync(vm.CreditCard, gualdar.Amount * interest);


            var pathBeneficiary = Path.Combine(webHostEnvironment.WebRootPath, "HTML", "notificaciones", "NotificationCashAvances.html");
            string _template = await System.IO.File.ReadAllTextAsync(pathBeneficiary);

            _template = _template.Replace("{{AMOUNT}}", gualdar.Amount.ToString("C", new CultureInfo("es-DO")))
                                 .Replace("{{Cuenta}}", $"[*****{vm.Account[^4..]}]")
                                 .Replace("{{DATE}}", gualdar.DateTime.ToString("dd/MM/yyyy HH:mm"));

            await emailService.SendAsync(new EmailRequestDto()
            {
                To = user!.Email,
                Subject = $"Avance de efectivo desde la tarjeta [****{vm.CreditCard[^4..]}]",
                BodyHtml = _template
            });


            return RedirectToRoute(new { area = "Client", controller = "Home", action = "Index" });
        }


    }
}
