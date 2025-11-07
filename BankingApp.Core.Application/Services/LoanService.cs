using AutoMapper;
using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;

namespace BankingApp.Core.Application.Services
{
    public class LoanService : GenericService<Loan, LoanDto>, ILoanService
    {
        private readonly ILoanRepository _loanRepository;
        public LoanService(ILoanRepository repo, IMapper mapper) : base(repo, mapper)
        {
            _loanRepository = repo;
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
        public Task<ApiLoanPaginationResultDto> GetAllFiltered(int page = 1, int pageSize = 20, string? state = null, string? documentId = null)
        {

            var query = _loanRepository.GetAllQuery();

            if (s)
        }
    }
}
