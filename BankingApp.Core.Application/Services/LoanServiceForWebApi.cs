using AutoMapper;
using AutoMapper.QueryableExtensions;
using BankingApp.Core.Application.Dtos.Email;
using BankingApp.Core.Application.Dtos.Installment;
using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Application.Dtos.Operations;
using BankingApp.Core.Application.Helpers;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection.Metadata;

namespace BankingApp.Core.Application.Services
{
    public class LoanServiceForWebApi : BaseLoanService, ILoanServiceForWebApi
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IMapper _mapper;
        private readonly IInstallmentRepository _installmentRepo;
        private readonly IAccountRepository _accountRepo;
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<Loan> _logger;
        private readonly ICreditCardRepository _cardRepository;

        public LoanServiceForWebApi(
            ILoanRepository repo,
            IMapper mapper,
            IInstallmentRepository installmentRepository,
            ILogger<Loan> logger,
            IAccountRepository accountRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IUserService userService,
            ICreditCardRepository cardRepository,
            ITransacctionRepository transacctionRepository
            )
            : base(repo, mapper, logger, unitOfWork, installmentRepository, accountRepository, emailService, userService, cardRepository, transacctionRepository)
        {
            _loanRepository = repo;
            _mapper = mapper;
            _installmentRepo = installmentRepository;
            _accountRepo = accountRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cardRepository= cardRepository;


        }

       



      



      

       

    }
}
