using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Application.Dtos.Operations;
using BankingApp.Core.Application.Dtos.User;
using BankingApp.Core.Domain.Entities;

namespace BankingApp.Core.Application.Interfaces
{
    public interface ILoanServiceForWebApp : IGenericService<Loan, LoanDto>, IBaseLoanService
    {
        Task<CreateLoanResult> ForceLoan(LoanRequest request);
        Task<LoanPaginationResultDto> GetAllFilteredWeb(int page = 1, int pageSize = 20, bool? completed = null, string? clientId = null);
        Task<List<UserDto>> GetClientsAvailableForLoan(string? DocumentId = null);
        Task<CreateLoanResult> HandleCreateRequestApp(LoanRequest request);
    }
}
