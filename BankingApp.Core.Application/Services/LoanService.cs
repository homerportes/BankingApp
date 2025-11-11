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
    public class LoanService : GenericService<Loan, LoanDto>, ILoanService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IMapper _mapper;
        private readonly IInstallmentRepository _installmentRepo;
        private readonly IAccountRepository _accountRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public LoanService(ILoanRepository repo, IMapper mapper, IInstallmentRepository installmentRepository, ILogger logger, IAccountRepository accountRepository, IUnitOfWork unitOfWork) : base(repo, mapper)
        {
            _loanRepository = repo;
            _mapper=mapper;
            _installmentRepo=installmentRepository;
            _accountRepo = accountRepository;
            _unitOfWork=unitOfWork;
            
            _logger = logger;
        }
        public async Task<string> GenerateLoanId()
        {
            bool LoanIdExists = false;
            string Id;
            do
            {
                Id = new string(
                         Guid.NewGuid()
                         .ToString("N")
                         .Where(char.IsDigit)
                         .Take(9)
                         .ToArray());
                LoanIdExists = await _loanRepository.LoanPublicIdExists(Id);

            } while (LoanIdExists);

            return Id;
        }
        public async Task<ApiLoanPaginationResultDto> GetAllFiltered(int page = 1, int pageSize = 20, string? state = null, string? clientId = null)
        {
            var query = _loanRepository.GetAllQuery();

            if (!string.IsNullOrEmpty(state))
            {
                try
                {
                    var statusEnum = EnumMapper<LoanStatus>.FromString(state);
                    query = query.Where(r => r.Status == statusEnum);
                }
                catch
                {
                    
                }
            }

            if (!string.IsNullOrEmpty(clientId))
            {
                query = query.Where(r => r.ClientId == clientId);
            }

            var totalCount = await query.CountAsync();

            query = query
                .Skip(pageSize * (page - 1))
                .Take(pageSize);

            var data = await query.ToListAsync();
            var mapped = _mapper.Map<List<LoanDto>>(data);

            return new ApiLoanPaginationResultDto
            {
                Data = mapped,
                PagesCount = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = page,
            };
        }


        public async Task<decimal> GetAverageLoanDebth()
        {
            return await _loanRepository.GetAllQuery().Where(r => r.IsActive).SumAsync(r => r.OutstandingBalance);
        }
        public async Task<bool> ClientHasActiveLoan ( string clientId)
        {
          return  await _loanRepository.GetAllQuery().Where(r => r.IsActive && r.ClientId == clientId).AnyAsync();
        }
        public async Task<CreateLoanResult> HandleCreateRequestApi(LoanApiRequest request)
        {
            var result = new CreateLoanResult();

            result.ClientHasActiveLoan = await ClientHasActiveLoan(request.ClientId);
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

        private static decimal DecimalPow(decimal baseValue, decimal exponent)
        {
        
            double result = Math.Pow((double)baseValue, (double)exponent);
            return (decimal)result;
        }



        public async Task VerifyAndMarkDelayedLoansAsync()
        {
            const int batchSize = 200;
            int skip = 0;
            int totalProcessed = 0;


            while (true)
            {
                var loans =  await _loanRepository.GetAllQueryWithInclude(new List<string> { "Installments" })
                          .Where(l => l.Installments.Any(i => !i.IsPaid && i.PayDate < DateOnly.FromDateTime(DateTime.Now)))
                          .Skip(skip)
                          .Take(batchSize)
                          .ToListAsync();


                if (!loans.Any()) break;


                foreach (var loan in loans)
                {
                    bool hasDelay = false;

                    foreach (var installment in loan.Installments)
                    {
                        if (!installment.IsPaid && installment.PayDate < DateOnly.FromDateTime(DateTime.Now))
                        {
                            installment.IsDelinquent = true;
                            hasDelay = true;
                        }
                    }

                    if (hasDelay)
                        loan.Status = LoanStatus.DELIQUENT;
                   
                    await _loanRepository.UpdateByObjectAsync(loan);

                    totalProcessed += loans.Count();
                    skip += batchSize;
                    _logger.LogInformation("Procesados {count} préstamos hasta ahora.", totalProcessed);

                }

            }
  

        }

        public async Task<DetailedLoanDto?> GetDetailed (string Id)
        {
            var loan=await _loanRepository.GetAllQuery().Where(r=>r.PublicId == Id).FirstOrDefaultAsync();   

            if (loan == null) return null;

            return new DetailedLoanDto
            {
                LoadId = loan.PublicId,
                Installments = _mapper.Map<List<InstallmentDto>>(loan.Installments)
            };
        }

        public async Task<OperationResultDto> UpdateLoanRate(string publicId, decimal newRate)
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

    }
}
