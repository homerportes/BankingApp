using AutoMapper;
using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Application.Dtos.User;
using BankingApp.Core.Application.Helpers;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Services
{

    //Revisar para el web app
    public class LoanServiceForWebApp : BaseLoanService, ILoanServiceForWebApp
    {
        private readonly IUserService _userService;
        private readonly ILoanRepository _repo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInstallmentRepository _installmentRepository;
        private readonly IAccountRepository _accountRepository;
        private ILogger<Loan> _logger;
        private ICreditCardRepository _cardRepository;
        private readonly IMapper _mapper;
        public LoanServiceForWebApp(
            ILoanRepository repo,
            IMapper mapper,
            ILogger<Loan> logger,
            IUserService userService,
            IUnitOfWork unitOfWork, 
            IInstallmentRepository installmentRepository,
            IAccountRepository accountRepository,
            IEmailService emailService,
            ICreditCardRepository creditCardRepository,
          ITransacctionRepository transacctionRepository

            )
            : base(repo, mapper, logger, unitOfWork, installmentRepository, accountRepository, emailService, userService, creditCardRepository, transacctionRepository)
        {
            _repo = repo;
            _userService= userService;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _installmentRepository = installmentRepository;
            _accountRepository = accountRepository;
            _cardRepository = creditCardRepository;
            _mapper = mapper;



        }

        public async Task<LoanPaginationResultDto> GetAllFilteredWeb(
   int page = 1,
   int pageSize = 20,
   bool? completed = null,
   string? clientId = null)
        {
            if (clientId == "__NO_MATCH__")
            {
                return new LoanPaginationResultDto
                {
                    Data = new List<LoanDto>(),
                    PagesCount = 0,
                    CurrentPage = page
                };
            }

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var query = _repo.GetAllQueryWithInclude(new List<string> { "Installments" });

            if (completed.HasValue)
            {
                query = query.Where(r => r.IsActive == completed.Value);
            }

            if (!string.IsNullOrWhiteSpace(clientId))
            {
                query = query.Where(r => r.ClientId == clientId.Trim());
            }

            query = query.OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();

            var data = await query
                .Skip(pageSize * (page - 1))
                .Take(pageSize)
                .ToListAsync();

            var userIds = data.Select(x => x.ClientId).Distinct().ToList();
            var users = await _userService.GetUsersBasicInfoAsync(userIds);
            var usersDict = users.ToDictionary(u => u.Id);

            var mapped = _mapper.Map<List<LoanDto>>(data);

            foreach (var loan in mapped)
            {
                if (usersDict.TryGetValue(loan.ClientId, out var user))
                {
                    loan.ClientName = user.FullName;
                    loan.ClientDocumentIdNumber = user.DocumentId;
                    loan.LoanStatus = EnumMapper<LoanStatus>.ToString(loan.Status);
                }
            }

            return new LoanPaginationResultDto
            {
                Data = mapped,
                PagesCount = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = page
            };
        }
        public async Task<List<UserDto>> GetClientsAvailableForLoan(string? DocumentId = null)
        {
            var clientsWithActiveLoan = await _repo.GetAllQuery()
                .Where(r => r.IsActive)
                .Select(r => r.ClientId)
                .Distinct()
                .ToListAsync();

            var activeLoanSet = new HashSet<string>(clientsWithActiveLoan);

            var clientDebts = await _repo.GetAllQuery()
                .Where(r => r.IsActive && !activeLoanSet.Contains(r.ClientId))
                .GroupBy(r => r.ClientId)
                .Select(g => new {
                    ClientId = g.Key,
                    TotalDebt = g.Sum(r => r.OutstandingBalance)
                })
                .ToDictionaryAsync(x => x.ClientId, x => x.TotalDebt);

            var result = await _userService.GetClientsWithDebtInfo(
                clientDebts,
                activeLoanSet, 
                DocumentId);

            return result;
        }


        public async Task<CreateLoanResult> ForceLoan(LoanRequest request)
        {
           var created= await Create(request);
          if (created.LoanCreated)
            {
               await SendEmail(request, created);
            }

            return created;
        }




        public async Task<CreateLoanResult> HandleCreateRequestApp(LoanRequest request)
        {
            var result = new CreateLoanResult();


            var clientLoansDebt = await GetClientLoansDebt(request.ClientId);
            var cardDebt = await _cardRepository.GetClientTotalCreditCardDebt(request.ClientId);

            var userDebt = clientLoansDebt + cardDebt;

            var systemLoansDebt = await GetTotalLoanDebt();
            var systemCardDebt = await _cardRepository.GetTotalClientsCreditCardDebt();

            var clientsCount = await _userService.GetAllClientsCount();
            var systemDebt =( systemLoansDebt + systemCardDebt) / clientsCount;


            if (userDebt > systemDebt)
            {
                result.ClientIsAlreadyHighRisk = true;
                return result;
            }

         

            if (systemDebt > 0)
            {
                result.ClientIsHighRisk = userDebt > systemDebt ||
                    (userDebt + ((request.LoanAmount * request.AnualInterest / 100) * (request.LoanTermInMonths / 12))) > systemDebt;
            }


            if (result.ClientIsHighRisk)
            {
                result.ClientIsAlreadyHighRisk = userDebt > systemDebt;
                return result;
            }

            return await Create(request);
        }

    }
}

