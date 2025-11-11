using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Infraestructure.Persistence.Repositories
{
    public class LoanRepository : GenericRepository<Loan>, ILoanRepository
    {

        private readonly BankingContext _bankingContext;
        public LoanRepository(BankingContext context) : base(context)
        {
            _bankingContext = context;
        }

        public async Task<bool> LoanPublicIdExists(string id)
        {
            return await _bankingContext.Set<Loan>().AnyAsync(r => r.PublicId == id);

        }

        public async Task<Loan> UpdateByObjectAsync(Loan entity)
        {
            var entry = await _bankingContext.Set<Loan>().Where(r=>r.Id== entity.Id).FirstOrDefaultAsync();

            if (entry != null)
            {
                _bankingContext.Entry(entry).CurrentValues.SetValues(entity);
                await _bankingContext.SaveChangesAsync();
            }
            return entry;
        }
    }
}
