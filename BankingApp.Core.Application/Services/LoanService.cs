using AutoMapper;
using AutoMapper.QueryableExtensions;
using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Application.Helpers;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Core.Application.Services
{
    public class LoanService : GenericService<Loan, LoanDto>, ILoanService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IMapper _mapper;
        public LoanService(ILoanRepository repo, IMapper mapper) : base(repo, mapper)
        {
            _loanRepository = repo;
            _mapper=mapper;
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


    }
}
