using AutoMapper;
using BankingApp.Core.Application.Dtos.Installment;
using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Application.Helpers;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace BankingApp.Core.Application.Services
{
    public abstract class BaseLoanService : GenericService<Loan, LoanDto>, IBaseLoanService
    {
        private readonly IMapper _mapper;
        private readonly ILogger<Loan> _logger;
        private readonly ILoanRepository _repo;

        public BaseLoanService(ILoanRepository repo, IMapper mapper, ILogger<Loan> logger) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
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
                LoanIdExists = await _repo.LoanPublicIdExists(Id);

            } while (LoanIdExists);

            return Id;

        }

        public async Task<LoanPaginationResultDto> GetAllFiltered(int page = 1, int pageSize = 20, string? state = null, string? clientId = null)
        {
            var query = _repo.GetAllQuery();

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

            return new LoanPaginationResultDto
            {
                Data = mapped,
                PagesCount = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = page,
            };
        }


        public async Task<decimal> GetAverageLoanDebth()
        {
            return await _repo.GetAllQuery().Where(r => r.IsActive).SumAsync(r => r.OutstandingBalance);
        }
        public async Task<bool> ClientHasActiveLoan(string clientId)
        {
            return await _repo.GetAllQuery().Where(r => r.IsActive && r.ClientId == clientId).AnyAsync();
        }



        public async Task VerifyAndMarkDelayedLoansAsync()
        {
            const int batchSize = 200;
            int skip = 0;
            int totalProcessed = 0;


            while (true)
            {
                var loans = await _repo.GetAllQueryWithInclude(new List<string> { "Installments" })
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

                    await _repo.UpdateByObjectAsync(loan);

                    totalProcessed += loans.Count();
                    skip += batchSize;
                    _logger.LogInformation("Procesados {count} préstamos hasta ahora.", totalProcessed);

                }
            }
        }
            
              public async Task<DetailedLoanDto?> GetDetailed(string Id)
             {
            var loan = await _repo.GetAllQuery().Where(r => r.PublicId == Id).FirstOrDefaultAsync();

            if (loan == null) return null;

            return new DetailedLoanDto
            {
                LoadId = loan.PublicId,
                Installments = _mapper.Map<List<InstallmentDto>>(loan.Installments)
            };
        }

    }
    

}
