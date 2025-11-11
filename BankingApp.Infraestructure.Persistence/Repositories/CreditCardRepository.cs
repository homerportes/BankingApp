using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Infraestructure.Persistence.Repositories
{
    public class CreditCardRepository : GenericRepository<CreditCard>, ICreditCardRepository
    {
        private readonly BankingContext _context;

        public CreditCardRepository(BankingContext context) : base(context)
        {
            _context = context;
        }

        public async Task<CreditCard?> GetByNumberAsync(string cardNumber)
        {
            return await _context.Set<CreditCard>()
                .FirstOrDefaultAsync(c => c.Number == cardNumber);
        }

        public async Task<List<CreditCard>> GetByClientIdAsync(string clientId)
        {
            return await _context.Set<CreditCard>()
                .Where(c => c.ClientId == clientId)
                .OrderByDescending(c => c.Id)
                .ToListAsync();
        }

        public async Task<bool> CardNumberExistsAsync(string cardNumber)
        {
            return await _context.Set<CreditCard>()
                .AnyAsync(c => c.Number == cardNumber);
        }
    }
}
