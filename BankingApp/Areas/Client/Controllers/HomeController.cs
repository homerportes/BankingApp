using AutoMapper;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.ViewModels.HomeClient;
using BankingApp.Core.Domain.Common.Enums;
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

            var _entities = mapper.Map<List<DataAccountHomeClientViewModel>>(Accounts);

       
            var listAccount = new DataAccountHomeClientListViewModel()
            {
                ListAccountClient = _entities
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


            var transactions = await clientService.GetDataListTransaction(number);



            if (transactions == null)
            {

                TempData["Error"] = "No se encontraron transacciones a la cuenta seleccionada...";
                return RedirectToRoute("Index");
            }



            var entities = mapper.Map<List<DataTransactionHomeClientViewModel>>(transactions);


            var _ListTransaction = new DataTransactionHomeClientListViewModel() { DataTransactionHomeClient = entities };

            return View(_ListTransaction);
        }


    }
}
