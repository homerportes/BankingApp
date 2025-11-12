using AutoMapper;
using AutoMapper.QueryableExtensions;
using BankingApp.Core.Application.Dtos.Installment;
using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Application.Dtos.Operations;
using BankingApp.Core.Application.Helpers;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
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

        public LoanServiceForWebApi(ILoanRepository repo, IMapper mapper, IInstallmentRepository installmentRepository, ILogger<Loan> logger, IAccountRepository accountRepository, IUnitOfWork unitOfWork) : base(repo, mapper, logger)
        {
            _loanRepository = repo;
            _mapper=mapper;
            _installmentRepo=installmentRepository;
            _accountRepo = accountRepository;
            _unitOfWork=unitOfWork;
            
            _logger = logger;
        }
    
      
        public async Task<CreateLoanResult> HandleCreateRequestApi(LoanApiRequest request)
        {
            var result = new CreateLoanResult();

            result.ClientHasActiveLoan = await base.ClientHasActiveLoan(request.ClientId);
            if (result.ClientHasActiveLoan)
                return result;

            var userDebt = await _loanRepository
                .GetAllQuery()
                .Where(r => r.IsActive && r.ClientId == request.ClientId)
                .Select(r => r.OutstandingBalance)
                .SumAsync();

            var systemDebt = await GetAverageLoanDebth();

            result.ClientIsHighRisk = userDebt > systemDebt || (userDebt + request.LoanAmount) > systemDebt;
            if (result.ClientIsHighRisk)
                return result;

            decimal annualInterest = request.AnualInterest;
            decimal monthlyRate = (annualInterest / 100m) / 12m;
            decimal n = request.LoanTermInMonths;
            decimal P = request.LoanAmount;

            decimal onePlusRPowN = DecimalPow(1 + monthlyRate, n);
            decimal constantPayment = P * (monthlyRate * onePlusRPowN) / (onePlusRPowN - 1);
            constantPayment = Math.Round(constantPayment, 2, MidpointRounding.ToEven);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var now = DateTime.Now;

                var loanEntity = new Loan
                {
                    Id = Guid.NewGuid(),
                    ClientId = request.ClientId,
                    LoanTermInMonths = request.LoanTermInMonths,
                    InterestRate = request.AnualInterest,
                    OutstandingBalance = request.LoanAmount,
                    TotalLoanAmount = request.LoanAmount,
                    PublicId = await GenerateLoanId(),
                    Status = LoanStatus.ONTIME,
                    IsActive = true,
                    CreatedAt = now
                };
                await _loanRepository.AddAsync(loanEntity);

                var installments = new List<Installment>(request.LoanTermInMonths);
                var firstDueDate = DateOnly.FromDateTime(now.AddMonths(1));

                for (int i = 0; i < request.LoanTermInMonths; i++)
                {
                    var dueDate = firstDueDate.AddMonths(i);
                    installments.Add(new Installment
                    {
                        LoanId = loanEntity.Id,
                        Id=0,
                        Number = i+1,
                        Value = constantPayment,
                        PayDate = dueDate,
                        IsPaid = i == 0, 
                        IsDelinquent = dueDate < DateOnly.FromDateTime(now)
                    });
                }
                await _installmentRepo.AddRangeAsync(installments);

                var account = await _accountRepo
                    .GetAllQuery()
                    .FirstOrDefaultAsync(a => a.ClientId == request.ClientId && a.Type == AccountType.PRIMARY);

                if (account is not null)
                {
                    account.Balance += P;
                    await _accountRepo.UpdateAsync(account.Id, account);
                }

                await _unitOfWork.CommitAsync();
                result.LoanCreated = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error al crear el préstamo para el cliente {ClientId}", request.ClientId);
                result.LoanCreated = false;
            }

            return result;
        }
        public async Task<OperationResultDto> UpdateLoanRateAPI(string publicId, decimal newRate)
        {
            var result = new OperationResultDto { IsSuccessful = false };

            var loan = await _loanRepository
                .GetAllQueryWithInclude(new List<string> { "Installments" })
                .FirstOrDefaultAsync(l => l.PublicId == publicId);

            if (loan is null)
            {
                result.StatusMessage = "Préstamo no encontrado.";
                return result;
            }

            if (!loan.IsActive)
            {
                result.StatusMessage = "No se puede actualizar un préstamo inactivo.";
                return result;
            }

            decimal annualInterest = newRate;
            decimal monthlyRate = (annualInterest / 100m) / 12m;
            decimal n = loan.LoanTermInMonths;
            decimal P = loan.TotalLoanAmount;

            decimal onePlusRPowN = DecimalPow(1 + monthlyRate, n);
            decimal newInstallmentValue = P * (monthlyRate * onePlusRPowN) / (onePlusRPowN - 1);
            newInstallmentValue = Math.Round(newInstallmentValue, 2, MidpointRounding.ToEven);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                loan.InterestRate = newRate;
                loan.UpdatedAt = DateTime.Now;

                foreach (var installment in loan.Installments.Where(i => !i.IsPaid))
                {
                    installment.Value = newInstallmentValue;
                    installment.IsModified = true;
                }

                await _loanRepository.UpdateByObjectAsync(loan);
                await _installmentRepo.UpdateRangeAsync(loan.Installments.ToList());

                await _unitOfWork.CommitAsync();

                result.IsSuccessful = true;
                result.StatusMessage = "Tasa actualizada correctamente.";
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error al actualizar la tasa del préstamo {PublicId}", publicId);
                result.StatusMessage = "Ocurrió un error al actualizar la tasa.";
            }

            return result;
        }
        private static decimal DecimalPow(decimal baseValue, decimal exponent)
        {
        
            double result = Math.Pow((double)baseValue, (double)exponent);
            return (decimal)result;
        }





      

       

    }
}
