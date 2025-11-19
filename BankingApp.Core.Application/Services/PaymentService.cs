using AutoMapper;
using BankingApp.Core.Application.Dtos.Email;
using BankingApp.Core.Application.Dtos.Payment;
using BankingApp.Core.Application.Dtos.Transaction;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BankingApp.Core.Application.Services
{
    public class PaymentService: IPaymentService
    {
        private readonly ITransacctionRepository _transactionRepository;
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly ICommerceRepository _commerceRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccountRepository _accountRepository;
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public PaymentService(ICreditCardRepository creditCardRepository , ITransacctionRepository transacctionRepository, ICommerceRepository commerceRepository, IUnitOfWork unitOfWork, IAccountRepository accountRepository, IPurchaseRepository purchaseRepository, IUserService userService, IEmailService emailService, IMapper mapper)
        {
            _transactionRepository= transacctionRepository;
            _creditCardRepository= creditCardRepository;
            _commerceRepository=commerceRepository;
            _unitOfWork = unitOfWork;
            _accountRepository= accountRepository;
            _purchaseRepository=purchaseRepository;
            _userService= userService;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<List<CommerceTransactionDto>> GetTransactionsForCommerceId(int commerceId, int page, int pageSize)
        {
            var usersCommerceAssociated = await _commerceRepository.GetAssociatesCommerceUsersId(commerceId);

            if (usersCommerceAssociated == null || !usersCommerceAssociated.Any())
                return new List<CommerceTransactionDto>();


            var accountNumbers = await _accountRepository
                .GetAllQuery()
                .Where(a => usersCommerceAssociated.Contains(a.UserId))
                .Select(a => a.Number)
                .ToListAsync();

            if (accountNumbers == null || !accountNumbers.Any())
                return new List<CommerceTransactionDto>();


            var query = _transactionRepository
                .GetAllQuery()
                .Where(t => accountNumbers.Contains(t.Beneficiary));


            var transactions = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            return _mapper.Map<List<CommerceTransactionDto>>(transactions);
        }


        public async Task<PaymentResultDto> Pay(PaymentRequestDto requestDto, int commerceId)
        {
            var response = new PaymentResultDto();

            var commerce = await _commerceRepository.GetByIdAsync(commerceId);
            if (commerce == null || !commerce.IsActive)
            {
                response.Message = "El comercio no existe o está inactivo";
                response.Continue = false;
                return response;
            }

            response = await PreprocessPay(requestDto, commerce);
            if (!response.Continue)
                return response;

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var creditCard = await _creditCardRepository.GetByNumberAsync(requestDto.CardNumber);
                if (creditCard == null)
                    throw new Exception("Tarjeta no encontrada");

                var commerceUserId = await _commerceRepository.GetAssociatedCommerceUserId(commerceId);
                if (commerceUserId == null)
                    throw new Exception("Usuario del comercio no encontrado");

                var commerceAccount = await _accountRepository.GetAccounByIdAsync(commerceUserId);
                var comerceUser = await _userService.GetUserById(commerceUserId);
                if (commerceAccount == null || comerceUser == null)
                    throw new Exception("Cuenta o usuario del comercio no encontrado");

                var clientId = await _creditCardRepository.GetAllQuery()
                                .Where(c => c.Number == requestDto.CardNumber)
                                .Select(c => c.ClientId)
                                .FirstOrDefaultAsync();
                var user = await _userService.GetUserById(clientId);
                if (user == null)
                    throw new Exception("Usuario del cliente no encontrado");

                // Acreditar monto
                commerceAccount.Balance += requestDto.TransactionAmount;
                await _accountRepository.UpdateAsync(commerceAccount.Id, commerceAccount);

                var transaction =new Transaction
                {
                    AccountNumber = commerceAccount.Number,
                    Beneficiary=commerceAccount.Number,
                    Status= OperationStatus.APPROVED,
                    Origin=creditCard.Number,
                    Amount= requestDto.TransactionAmount,
                    Type= TransactionType.CREDIT
                    
                };

                await _transactionRepository.AddAsync(transaction);
                // Registrar consumo
                var purchase = new Purchase
                {
                    MerchantName = commerce.Name,
                    AmountSpent = requestDto.TransactionAmount,
                    CardNumber = creditCard.Number,
                    DateTime = DateTime.Now,
                    Status = OperationStatus.APPROVED,
                    Id = Guid.NewGuid()
                };
                await _purchaseRepository.AddAsync(purchase);

                creditCard.TotalAmountOwed += requestDto.TransactionAmount;
                await _creditCardRepository.UpdateAsync(creditCard.Id, creditCard);

                await _unitOfWork.CommitAsync();

                // Enviar correos
                var last4Digits = creditCard.Number[^4..];
                await SendUserEmail(user.Email, last4Digits, requestDto.TransactionAmount, commerce.Name, purchase.DateTime);
                await SendCommerceEmail(comerceUser.Email, last4Digits, requestDto.TransactionAmount, commerce.Name, purchase.DateTime);

                response.IsCompleted = true;
                response.Message = "Pago procesado correctamente";
                return response;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                response.IsCompleted = false;
                response.Message = "Error al procesar el pago";
                return response;
            }
        }


        private async Task SendUserEmail(string email, string last4Digits, decimal amount, string commerceName, DateTime dateTime)
        {
            var subject = $"Consumo realizado con la tarjeta {last4Digits}";
            var body = $"Se ha realizado un pago de {amount:C} en {commerceName} con la tarjeta terminada en {last4Digits}.\nFecha y hora: {dateTime}";
            await _emailService.SendAsync(new EmailRequestDto
            {
                To = email,
                BodyHtml = body,
                Subject = subject,

            });
        }

        private async Task SendCommerceEmail(string email, string last4Digits, decimal amount, string commerceName, DateTime dateTime)
        {
            var subject = $"Pago recibido a través de tarjeta {last4Digits}";
            var body = $"Se ha recibido un pago de {amount:C} con la tarjeta terminada en {last4Digits}.\nNombre del comercio: {commerceName}\nFecha y hora: {dateTime}";
            await _emailService.SendAsync(new EmailRequestDto {
            To= email,
            BodyHtml= body,
            Subject= subject,
          
            });
        }

        private async Task<PaymentResultDto> PreprocessPay(PaymentRequestDto requestDto, Commerce? commerce)
        {
            var response = new PaymentResultDto()
            {
                Continue = false,
                IsCompleted= false
            };
            response.CommerceIsValid = commerce!=null && commerce.IsActive;
            if (!response.CommerceIsValid)
            {
                response.Message = "El comercio no existe o está inactivo";
                return response;
            }


            response.CardExists= await _creditCardRepository.CardNumberExistsAsync(requestDto.CardNumber);
            if (!response.CardExists)
            {
                response.Message = "La tarjeta es no existe";
                return response;
            }

            response.CardIsValid = await _creditCardRepository.CardDataIsValidForPaymentAsync(requestDto.CardNumber,requestDto.MonthExpirationCard, requestDto.YearExpirationCard, requestDto.Cvc);
            if (!response.CardIsValid)
            {
                response.Message = "La tarjeta es inválida. Asegúrese de quue los datos sean correctos y la tarjeta no esté vencida o cancelada";
                return response;
            }


            response.CardHasEnoughFunds = await _creditCardRepository.CreditCardHasEnoughFunds(requestDto.CardNumber, requestDto.TransactionAmount);
            if (!response.CardHasEnoughFunds)
            {
                response.Message = "Pago cancelado, tarjeta con fondos insuficientes";
                return response;
            }

            response.Continue = true;
            response.Message = "Pago autorizado para procesar";
            return response;
        }


    }
}
