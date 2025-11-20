using AutoMapper;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.ViewModels.CreditCard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class CreditCardController : Controller
    {
        private readonly ICreditCardService _creditCardService;
        private readonly IUserService _userService;
        private readonly IAccountServiceForWebAPP _accountService;
        private readonly IMapper _mapper;

        public CreditCardController(
            ICreditCardService creditCardService,
            IUserService userService,
            IAccountServiceForWebAPP accountService,
            IMapper mapper)
        {
            _creditCardService = creditCardService;
            _userService = userService;
            _accountService = accountService;
            _mapper = mapper;
        }

        // GET: Admin/CreditCard
        public async Task<IActionResult> Index(int page = 1, string? cedula = null, string? estado = null)
        {
            try
            {
                const int pageSize = 20;
                List<CreditCardViewModel> viewModels;
                int totalRecords = 0;

                var allCards = await _creditCardService.GetAllAsync(1, 10000, "ALL");
                var cardsActive = allCards.Where(s => s.Status == Core.Domain.Common.Enums.CardStatus.ACTIVE).ToList();

                if (!string.IsNullOrEmpty(cedula))
                {
                    cedula = cedula.Trim();
                    var user = await _userService.GetByDocumentId(cedula);

                    if (user == null)
                    {
                        TempData["Warning"] = $"No se encontró ningún cliente con la cédula '{cedula}'. Verifique el número e intente nuevamente.";
                        viewModels = new List<CreditCardViewModel>();
                        ViewBag.Cedula = cedula;
                        ViewBag.Estado = estado;
                        ViewBag.CurrentPage = 1;
                        ViewBag.TotalPages = 1;
                        ViewBag.PageSize = pageSize;
                        ViewBag.TotalRecords = 0;
                        return View(viewModels);
                    }
                    else
                    {
                        allCards = allCards.Where(c => c.ClientId == user.Id).ToList();
                        ViewBag.UserName = $"{user.Name} {user.LastName}";
                    }
                }


            
                if (estado == null && cedula != null)
                {

                    allCards = allCards.Where(c => c.Status == Core.Domain.Common.Enums.CardStatus.ACTIVE 
                    || c.Status == Core.Domain.Common.Enums.CardStatus.CANCELLED).ToList();

                }else if (!string.IsNullOrEmpty(estado))
                {
                    allCards = allCards.Where(c => c.Status.ToString() == estado).ToList();
                }



                if (estado == null && cedula == null)
                {
                    var Active = cardsActive
                      .OrderByDescending(c => c.Status == BankingApp.Core.Domain.Common.Enums.CardStatus.ACTIVE)
                      .ThenByDescending(c => c.Id)
                      .ToList();


                    totalRecords = Active.Count;

                    var paginatedCardActive = Active
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

                    viewModels = _mapper.Map<List<CreditCardViewModel>>(paginatedCardActive);


                }
                else
                {

                    var sortedCards = allCards
                           .OrderByDescending(c => c.Status == BankingApp.Core.Domain.Common.Enums.CardStatus.ACTIVE)
                           .ThenByDescending(c => c.Id)
                           .ToList();


                    totalRecords = sortedCards.Count;

                    var paginatedCards = sortedCards
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

                    viewModels = _mapper.Map<List<CreditCardViewModel>>(paginatedCards);
                }


                ViewBag.Cedula = cedula;
                ViewBag.Estado = estado;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(totalRecords / (double)pageSize));
                ViewBag.PageSize = pageSize;
                ViewBag.TotalRecords = totalRecords;

                return View(viewModels);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar las tarjetas de crédito: " + ex.Message;
                return View(new List<CreditCardViewModel>());
            }
        }


        // GET: Admin/CreditCard/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var card = await _creditCardService.GetByIdAsync(id);
                if (card == null)
                {
                    TempData["Error"] = "La tarjeta especificada no existe";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<CreditCardViewModel>(card);

                try
                {
                    var purchases = await _creditCardService.GetPurchasesByCardIdAsync(id);
                    var sortedPurchases = purchases.OrderByDescending(p => p.DateTime).ToList();
                    var purchaseViewModels = _mapper.Map<List<PurchaseViewModel>>(sortedPurchases);
                    ViewBag.Purchases = purchaseViewModels;
                }
                catch (Exception exPurchases)
                {
                    TempData["Warning"] = $"No se pudieron cargar los consumos: {exPurchases.Message}";
                    ViewBag.Purchases = new List<PurchaseViewModel>();
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los detalles: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/CreditCard/SelectClient
        public async Task<IActionResult> SelectClient(string? cedula = null)
        {
            try
            {
                var allUsers = await _accountService.GetAllUser(isActive: true);
                var clients = allUsers.Where(u => u.Role == "CLIENT").ToList();

                decimal totalDebt = 0;
                int clientCount = 0;
                var clientDebts = new Dictionary<string, decimal>();

                foreach (var client in clients)
                {
                    var clientCards = await _creditCardService.GetAllAsync(1, 10000, "ALL");
                    var userCards = clientCards.Where(c => c.ClientId == client.Id).ToList();

                    decimal clientDebt = 0;
                    if (userCards.Any())
                    {
                        clientDebt = userCards.Sum(c => c.TotalAmountOwed);
                        totalDebt += clientDebt;
                        clientCount++;
                    }

                    clientDebts[client.Id] = clientDebt;
                }

                decimal averageDebt = clientCount > 0 ? (totalDebt / clients.Count) : 0;
                ViewBag.AverageDebt = averageDebt;
                ViewBag.ClientDebts = clientDebts;

                if (!string.IsNullOrEmpty(cedula))
                {
                    cedula = cedula.Trim();
                    var user = await _userService.GetByDocumentId(cedula);

                    if (user != null)
                    {
                        clients = clients.Where(c => c.Id == user.Id).ToList();
                    }
                    else
                    {
                        clients = new List<BankingApp.Core.Application.Dtos.User.UserDto>();
                        TempData["Warning"] = $"No se encontró ningún cliente con la cédula '{cedula}'";
                    }
                }

                ViewBag.Cedula = cedula;
                return View(clients);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los clientes: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/CreditCard/Create
        public async Task<IActionResult> Create(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                TempData["Error"] = "Debe seleccionar un cliente";
                return RedirectToAction(nameof(SelectClient));
            }

            var client = await _userService.GetUserById(clientId);
            if (client == null)
            {
                TempData["Error"] = "El cliente seleccionado no existe";
                return RedirectToAction(nameof(SelectClient));
            }

            ViewBag.ClientName = $"{client.Name} {client.LastName}";

            var viewModel = new SaveCreditCardViewModel
            {
                ClientId = clientId
            };

            return View(viewModel);
        }

        // POST: Admin/CreditCard/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaveCreditCardViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                if (string.IsNullOrEmpty(viewModel.ClientId))
                {
                    ModelState.AddModelError("", "El cliente es requerido");
                    return View(viewModel);
                }

                var client = await _userService.GetUserById(viewModel.ClientId);
                if (client == null)
                {
                    ModelState.AddModelError("", "El cliente especificado no existe");
                    return View(viewModel);
                }

                var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                {
                    ModelState.AddModelError("", "No se pudo identificar al administrador");
                    return View(viewModel);
                }

                await _creditCardService.CreateAsync(viewModel.ClientId, viewModel.CreditLimit, adminId);

                TempData["Success"] = "Tarjeta de crédito creada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al crear la tarjeta: " + ex.Message);
                return View(viewModel);
            }
        }

        // GET: Admin/CreditCard/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var card = await _creditCardService.GetByIdAsync(id);
                if (card == null)
                {
                    TempData["Error"] = "La tarjeta especificada no existe";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new SaveCreditCardViewModel
                {
                    Id = id,
                    CreditLimit = card.CreditLimitAmount,
                    ClientId = card.ClientId
                };

                var creditCardViewModel = _mapper.Map<CreditCardViewModel>(card);
                ViewBag.CreditCard = creditCardViewModel;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar la tarjeta: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/CreditCard/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SaveCreditCardViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var cardForView = await _creditCardService.GetByIdAsync(id);
                if (cardForView != null)
                {
                    var creditCardViewModel = _mapper.Map<CreditCardViewModel>(cardForView);
                    ViewBag.CreditCard = creditCardViewModel;
                }
                return View(viewModel);
            }

            try
            {
                var card = await _creditCardService.GetByIdAsync(id);
                if (card == null)
                {
                    TempData["Error"] = "La tarjeta especificada no existe";
                    return RedirectToAction(nameof(Index));
                }

                if (viewModel.CreditLimit < card.TotalAmountOwed)
                {
                    ModelState.AddModelError("CreditLimit",
                        $"El nuevo límite no puede ser inferior al monto adeudado actual (RD${card.TotalAmountOwed:N2})");

                    var creditCardViewModel = _mapper.Map<CreditCardViewModel>(card);
                    ViewBag.CreditCard = creditCardViewModel;
                    return View(viewModel);
                }

                var updated = await _creditCardService.UpdateCreditLimitAsync(id, viewModel.CreditLimit);
                if (!updated)
                {
                    ModelState.AddModelError("", "No se pudo actualizar el límite de la tarjeta");

                    var creditCardViewModel = _mapper.Map<CreditCardViewModel>(card);
                    ViewBag.CreditCard = creditCardViewModel;
                    return View(viewModel);
                }

                TempData["Success"] = "Límite de crédito actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al actualizar la tarjeta: " + ex.Message);
                return View(viewModel);
            }
        }

        // GET: Admin/CreditCard/Cancel/5
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var card = await _creditCardService.GetByIdAsync(id);
                if (card == null)
                {
                    TempData["Error"] = "La tarjeta especificada no existe";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<CreditCardViewModel>(card);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar la tarjeta: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/CreditCard/CancelConfirmed/5
        [HttpPost, ActionName("CancelConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            try
            {
                var card = await _creditCardService.GetByIdAsync(id);
                if (card == null)
                {
                    TempData["Error"] = "La tarjeta especificada no existe";
                    return RedirectToAction(nameof(Index));
                }

                if (card.TotalAmountOwed > 0)
                {
                    TempData["Error"] = "Para cancelar esta tarjeta, el cliente debe saldar la totalidad de la deuda pendiente";
                    return RedirectToAction(nameof(Cancel), new { id });
                }

                var cancelled = await _creditCardService.CancelCardAsync(id);
                if (!cancelled)
                {
                    TempData["Error"] = "No se pudo cancelar la tarjeta";
                    return RedirectToAction(nameof(Cancel), new { id });
                }

                TempData["Success"] = "Tarjeta cancelada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cancelar la tarjeta: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
