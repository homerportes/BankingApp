using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Infraestructure.Persistence.Repositories
{
    public class PurchaseRepository : GenericRepository<Purchase>, IPurchaseRepository
    {
        private readonly BankingContext _context;

        public PurchaseRepository(BankingContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Purchase>> GetByCardNumberAsync(string cardNumber)
        {
            return await _context.Set<Purchase>()
                .Where(p => p.CardNumber == cardNumber)
                .OrderByDescending(p => p.DateTime)
                .ToListAsync();
        }
    }
}
