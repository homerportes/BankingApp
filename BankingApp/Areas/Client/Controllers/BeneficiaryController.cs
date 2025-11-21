using AutoMapper;
using BankingApp.Core.Application.Dtos.Beneficiary;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.ViewModels.Beneficiary;
using BankingApp.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace BankingApp.Areas.Client.Controllers
{
    [Authorize(Roles = "CLIENT")]
    [Area("Client")]
    public class BeneficiaryController : Controller
    {


        private readonly IMapper mapper;
        private readonly IBeneficiaryService service;
        private readonly UserManager<AppUser> userManager;
        private readonly ITransactionService transactionService;



        public BeneficiaryController(IBeneficiaryService service, IMapper mapper, UserManager<AppUser> userManager, ITransactionService transactionService)
        {


            this.service = service;
            this.mapper = mapper;
            this.userManager = userManager;
            this.transactionService = transactionService;
        }





        public async Task<IActionResult> Index()
        {


            var user = await userManager.GetUserAsync(User);
            var beneficiaries = await service.GetBeneficiaryList(user!.Id);
       

            var entities = mapper.Map<List<DataBeneficiaryViewModel>>(beneficiaries);
            var data = new DataBeneficiaryListViewModel() {ListBeneficiary = entities};
            var listBeneficiaries = new BeneficiaryIndexViewModel()
            {
                CreateBeneficiary = new CreateBeneficiaryViewModel()
                { Id = 0,
                    BeneficiaryId = "",
                    ClientId = "",
                    Fecha = DateTime.UtcNow,
                    Number = ""
                },
                ListBeneficiary = data,
            };

            return View(listBeneficiaries);

        }




        [HttpPost]
        public async Task<IActionResult> Create(BeneficiaryIndexViewModel vm)
        {
            var createVm = vm.CreateBeneficiary;
            var user = await userManager.GetUserAsync(User);

            var dataBeneficiary = await service.ValidateAccountNumberExist(createVm.Number);
            var Accounts = await transactionService.CuentaListAsync(user!.Id);
            if (dataBeneficiary == null || dataBeneficiary.IsExist == false)
            {
                ModelState.AddModelError(string.Empty, "El número de cuenta ingresado no corresponde a ninguna cuenta válida");
            }
            else if (!await service.ValidateAccountNumber(createVm.Number, user!.Id))
            {
                ModelState.AddModelError(string.Empty, "El número de cuenta ingresado ya corresponde a uno de tus beneficiarios");
            }

            if (Accounts!.Contains(vm.CreateBeneficiary.Number))
            {

                ModelState.AddModelError(string.Empty, "No puedes agregar un numero de cuenta propio como beneficiario, Favor verificar y volver a intentar");

            }


            if (!ModelState.IsValid)
            {
                var beneficiaries = await service.GetBeneficiaryList(user!.Id);
                var entities = mapper.Map<List<DataBeneficiaryViewModel>>(beneficiaries);
                var listBeneficiaries = new BeneficiaryIndexViewModel
                {
                    ListBeneficiary = new DataBeneficiaryListViewModel { ListBeneficiary = entities },
                    CreateBeneficiary = createVm,
                    ShowCreateModal = true
                };
                return View("Index", listBeneficiaries);
            }

            var relacion = new CreateBeneficiaryDto
            {
                Id = 0,
                ClientId = user!.Id,
                Fecha = DateTime.Now,
                BeneficiaryId = dataBeneficiary!.IdBeneficiary
            };

            await service.AddAsync(relacion);

            return RedirectToAction("Index");
        }





        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {

            var entity = await service.GetByIdAsync(id);

            if (entity == null)
            {

                ModelState.AddModelError("","Hubo un error al intentar eliminar el beneficiario, dicha entidad no se encontro");
                return View();

            }

            var _entity = mapper.Map<DeleteBeneficiaryViewModel>(entity);
            return View(_entity);
        }



        [HttpPost]
        public async Task<IActionResult> Delete(DeleteBeneficiaryViewModel vm)
        {

            if (vm == null)
            {
            
               return View(vm);
            
            }

            if (!ModelState.IsValid)
            {

                return View(vm);
               
            
            }
                

            await service.DeleteAsync(vm.Id);
            return RedirectToRoute(new { area = "Client", controller = "Beneficiary", action = "Index" });
        }







    }
}
