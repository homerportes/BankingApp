using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Application.Dtos.Operations;
using BankingApp.Core.Application.Dtos.User;
using BankingApp.Core.Domain.Entities;


namespace BankingApp.Core.Application.Interfaces
{
    public interface ILoanService : IGenericService<Loan,LoanDto>
    {
        Task<bool> ClientHasActiveLoan(string clientId);
        Task<ApiLoanPaginationResultDto> GetAllFiltered(int page = 1, int pageSize = 20, string? state = null ,string ? documentId = null);
        Task<DetailedLoanDto?> GetDetailed(string Id);
        Task<CreateLoanResult> HandleCreateRequestApi(LoanApiRequest request);
        Task<OperationResultDto> UpdateLoanRate(string publicId, decimal newRate);
        Task VerifyAndMarkDelayedLoansAsync();
    }
}
