using BankingApp.Core.Application.Dtos.CreditCard;
using BankingApp.Core.Application.Helpers;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using AutoMapper;
using System.Security.Cryptography;
using System.Text;

namespace BankingApp.Core.Application.Services
{
    public class CreditCardService : ICreditCardService
    {
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public CreditCardService(
            ICreditCardRepository creditCardRepository,
            IPurchaseRepository purchaseRepository,
            IAccountRepository accountRepository,
            IUserService userService,
            IEmailService emailService,
            IMapper mapper)
        {
            _creditCardRepository = creditCardRepository;
            _purchaseRepository = purchaseRepository;
            _accountRepository = accountRepository;
            _userService = userService;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<List<CreditCardDto>> GetAllAsync(int page, int pageSize, string? status = null)
        {
            var allCards = await _creditCardRepository.GetAllList();
            if (allCards == null) return new List<CreditCardDto>();

            // Filtrar por estado si se proporciona
            if (!string.IsNullOrWhiteSpace(status))
            {
                // Si el estado es "ALL", no filtrar por estado
                if (status.ToUpper() != "ALL")
                {
                    var cardStatusEnum = EnumMapper<CardStatus>.FromString(status);
                    allCards = allCards.Where(c => c.Status == cardStatusEnum).ToList();
                }
                // Si es "ALL", no aplicar filtro - mostrar todas
            }
            else
            {
                // Por defecto, solo mostrar tarjetas activas
                allCards = allCards.Where(c => c.Status == CardStatus.ACTIVE).ToList();
            }

            // Ordenar de más reciente a más antigua
            allCards = allCards.OrderByDescending(c => c.Id).ToList();

            // Paginación
            var paginatedCards = allCards
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new List<CreditCardDto>();

            foreach (var card in paginatedCards)
            {
                var cardDto = _mapper.Map<CreditCardDto>(card);
                var user = await _userService.GetUserById(card.ClientId);
                cardDto.ClientName = user != null ? $"{user.Name} {user.LastName}" : "";
                result.Add(cardDto);
            }

            return result;
        }

        public async Task<List<CreditCardDto>> GetByClientDocumentAsync(string documentId, string? status = null)
        {
            var user = await _userService.GetByDocumentId(documentId);
            if (user == null) return new List<CreditCardDto>();

            var cards = await _creditCardRepository.GetByClientIdAsync(user.Id);

            // Filtrar por estado si se proporciona
            if (!string.IsNullOrWhiteSpace(status))
            {
                var cardStatusEnum = EnumMapper<CardStatus>.FromString(status);
                cards = cards.Where(c => c.Status == cardStatusEnum).ToList();
            }

            // Ordenar: primero activas, luego canceladas, y dentro de cada grupo por fecha descendente
            cards = cards.OrderBy(c => c.Status == CardStatus.CANCELLED ? 1 : 0)
                        .ThenByDescending(c => c.Id)
                        .ToList();

            var result = _mapper.Map<List<CreditCardDto>>(cards);

            // Agregar nombre del cliente a cada tarjeta
            foreach (var cardDto in result)
            {
                cardDto.ClientName = $"{user.Name} {user.LastName}";
            }

            return result;
        }

        public async Task<CreditCardDto?> GetByIdAsync(int id)
        {
            var card = await _creditCardRepository.GetByIdAsync(id);
            if (card == null) return null;

            var cardDto = _mapper.Map<CreditCardDto>(card);
            var user = await _userService.GetUserById(card.ClientId);
            cardDto.ClientName = user != null ? $"{user.Name} {user.LastName}" : "";

            return cardDto;
        }

        public async Task<CreditCardDto?> GetByNumberAsync(string cardNumber)
        {
            var card = await _creditCardRepository.GetByNumberAsync(cardNumber);
            if (card == null) return null;

            var cardDto = _mapper.Map<CreditCardDto>(card);
            var user = await _userService.GetUserById(card.ClientId);
            cardDto.ClientName = user != null ? $"{user.Name} {user.LastName}" : "";

            return cardDto;
        }

        public async Task<string> GenerateCardNumber()
        {
            string cardNumber;
            do
            {
                cardNumber = GenerateRandomCardNumber();
            } while (await _creditCardRepository.CardNumberExistsAsync(cardNumber));

            return cardNumber;
        }

        private string GenerateRandomCardNumber()
        {
            var random = new Random();
            var cardNumber = "";
            for (int i = 0; i < 16; i++)
            {
                cardNumber += random.Next(0, 10).ToString();
            }
            return cardNumber;
        }

        public async Task<string> GenerateCVC()
        {
            var random = new Random();
            var cvc = random.Next(100, 1000).ToString();
            return cvc;
        }

        private string HashCVC(string cvc)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(cvc);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public async Task<CreditCardDto> CreateAsync(string clientId, decimal creditLimit, string adminId)
        {
            var cardNumber = await  GenerateCardNumber();
            var cvc = await GenerateCVC();
            var hashed = HashCVC(cvc);

            var expirationDate = DateTime.Now.AddYears(3);
            expirationDate = new DateTime(expirationDate.Year, expirationDate.Month, DateTime.DaysInMonth(expirationDate.Year, expirationDate.Month));

            var creditCard = new CreditCard
            {
                Id = 0,
                Number = cardNumber,
                ClientId = clientId,
                CreditLimitAmount = creditLimit,
                ExpirationDate = expirationDate,
                TotalAmountOwed = 0,
                CVC = hashed,
                Status = CardStatus.ACTIVE,
                AdminId = adminId
            };

            await _creditCardRepository.AddAsync(creditCard);

            var cardDto = _mapper.Map<CreditCardDto>(creditCard);
            var user = await _userService.GetUserById(clientId);
            cardDto.ClientName = user != null ? $"{user.Name} {user.LastName}" : "";
            cardDto.CVC = cvc;

            return cardDto;
        }

        public async Task<bool> UpdateCreditLimitAsync(int id, decimal newLimit)
        {
            var card = await _creditCardRepository.GetByIdAsync(id);
            if (card == null) return false;

            // Validar que el nuevo límite no sea inferior a la deuda actual
            if (newLimit < card.TotalAmountOwed) return false;

            card.CreditLimitAmount = newLimit;
            await _creditCardRepository.UpdateAsync(card.Id, card);

            // Enviar correo al cliente
            var user = await _userService.GetUserById(card.ClientId);
            if (user != null)
            {
                var lastFourDigits = card.Number.Substring(card.Number.Length - 4);
                await _emailService.SendAsync(new Dtos.Email.EmailRequestDto
                {
                    To = user.Email,
                    Subject = $"Límite de crédito actualizado - Tarjeta {lastFourDigits}",
                    BodyHtml = $"Estimado/a {user.Name} {user.LastName},<br><br>" +
                           $"Le informamos que el límite de crédito de su tarjeta terminada en {lastFourDigits} ha sido modificado.<br>" +
                           $"Nuevo límite: RD${newLimit:N2}<br><br>" +
                           $"Saludos cordiales,<br>Equipo de Banca en Línea"
                });
            }

            return true;
        }

        public async Task<bool> CancelCardAsync(int id)
        {
            var card = await _creditCardRepository.GetByIdAsync(id);
            if (card == null) return false;

            // Validar que no tenga deuda pendiente
            if (card.TotalAmountOwed > 0) return false;

            card.Status = CardStatus.CANCELLED;
            await _creditCardRepository.UpdateAsync(card.Id, card);

            return true;
        }

        public async Task<List<PurchaseDto>> GetPurchasesByCardIdAsync(int cardId)
        {
            var card = await _creditCardRepository.GetByIdAsync(cardId);
            if (card == null) return new List<PurchaseDto>();

            var purchases = await _purchaseRepository.GetByCardNumberAsync(card.Number);

            return _mapper.Map<List<PurchaseDto>>(purchases);
        }
    }
}
