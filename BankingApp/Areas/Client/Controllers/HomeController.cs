using AutoMapper;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.ViewModels.HomeClient;
using BankingApp.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize(Roles = "CLIENT")]
    public class HomeController : Controller
    {
        private readonly IMapper mapper;
        private readonly IHomeClietService clientService;
        private readonly UserManager<AppUser> userManager;




        public HomeController(IMapper mapper, IHomeClietService clientService, UserManager<AppUser> userManager) 
        {
        
           this.mapper = mapper;    
           this.clientService = clientService; 
           this.userManager = userManager;
        
      
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await userManager.GetUserAsync(User);

            var Accounts = await clientService.GetDataAccountClient(user!.Id);
            var Loans = await clientService.GetDataLoanHomeClient(user!.Id);
            var creditCards = await clientService.GetDetaCreditCardHomeClient(user.Id);

            var _entities = mapper.Map<List<DataAccountHomeClientViewModel>>(Accounts);
            var _entitiesLoan = mapper.Map<List<DataLoanHomeClientViewModel>>(Loans);
            var _credtiCards = mapper.Map<List<DataCreditCardHomeClientViewModel>>(creditCards);
       
            var listAccount = new DataAccountHomeClientListViewModel()
            {
                ListAccountClient = _entities,
                ListLoanHomeClient = _entitiesLoan,
                ListCredtiCardHomeClient = _credtiCards
            };

            return View(listAccount);
        }






        [HttpGet]
        public async Task<IActionResult> DetailsTransaction(string number)
        {

            if (string.IsNullOrEmpty(number))
            {

                TempData["Error"] ="Ocurrio un error al intentar ver los resultados de esta cuenta, favor volver a intentar";
                return RedirectToRoute("Index");
            }


            var Details = await clientService.GetDataListTransaction(number);



            if (Details == null)
            {

                TempData["Error"] = "No se encontraron transacciones a la cuenta seleccionada...";
                return RedirectToRoute("Index");
            }



            var entities = mapper.Map<List<DataTransactionHomeClientViewModel>>(Details);


            var _ListCreditCard = new DataTransactionHomeClientListViewModel() { DataTransactionHomeClient = entities };

            return View(_ListCreditCard);
        }







        [HttpGet]
        public async Task<IActionResult> DetailsLoan(Guid LoanId)
        {

            if (LoanId == Guid.Empty)
            {

                TempData["Error"] = "Ocurrio un error al intentar ver la tabla de amortizacion de este prestamo, favor volver a intentar";
                return RedirectToRoute("Index");
            }


            var Loans = await clientService.GetDetailsLoanHomeClient(LoanId);



            if (Loans == null)
            {

                TempData["Error"] = "No se encontraron transacciones a la cuenta seleccionada...";
                return RedirectToRoute("Index");
            }



            var entities = mapper.Map<List<DetailsLoanHomeClientViewModel>>(Loans);


            var _ListTransaction = new DetailsLoanListHomeClientViewModel() { LoanHomeClientViewModels = entities };

            return View(_ListTransaction);
        }








        [HttpGet]
        public async Task<IActionResult> DetailsCredtiCard(string number)
        {

            if (string.IsNullOrEmpty(number))
            {

                TempData["Error"] = "Ocurrio un error al intentar ver la informacion del consumo, favor intentar otra vez";
                return RedirectToRoute("Index");
            }


            var CredtiCard = await clientService.GetDetailsCreditCardHomeClient(number);



            if (CredtiCard == null)
            {

                TempData["Error"] = "No se encontraron transacciones a la cuenta seleccionada...";
                return RedirectToRoute("Index");
            }



            var entities = mapper.Map<List<DetailsCreditCardHomeClientViewModel>>(CredtiCard);


            var _ListTransaction = new DetailsCreditCardHomeClientListViewModel() { DetailsCreditCard = entities };

            return View(_ListTransaction);
        }

























    }
}
