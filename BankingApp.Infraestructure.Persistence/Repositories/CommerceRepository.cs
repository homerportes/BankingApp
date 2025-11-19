using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;


namespace BankingApp.Infraestructure.Persistence.Repositories
{
    public class CommerceRepository : GenericRepository<Commerce>, ICommerceRepository
    {
        BankingContext _context;
        public CommerceRepository(BankingContext context) : base(context)
        {
            _context = context;

        }
        public async Task<bool> CommerceIsValid(int id)
        {
            if (id <= 0)
                return false;

            return await _context.Set<Commerce>()
                                 .AnyAsync(r => r.Id == id && r.IsActive);
        }
        public async Task<string?> GetAssociatedCommerceUserId(int commerceId)
        {
            return await _context.Set<Commerce>()
                                 .Where(c => c.Id == commerceId)
                                 .Select(c => c.Users.Select(u => u.UserId).FirstOrDefault())
                                 .FirstOrDefaultAsync();
        }
        public async Task<List<string>> GetAssociatesCommerceUsersId(int commerceId)
        {
            return await _context.Set<Commerce>()
                                 .Where(c => c.Id == commerceId)
                                 .SelectMany(c => c.Users)
                                 .Select(u => u.UserId)
                                 .ToListAsync();
        }



    }
}
