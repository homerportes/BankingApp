using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Infraestructure.Persistence.Repositories
{
    public class CommerceUserRepository : GenericRepository<CommerceUser>, ICommerceUserRepository
    {

        private readonly BankingContext _context;
        public CommerceUserRepository(BankingContext context) : base(context)
        {
            _context = context;
        }


        public async Task<int?> GetCommerceAsociatedToUserId(string userId)
        {
            return await _context.Set<CommerceUser>()
                .AsNoTracking()
                .Where(r => r.UserId == userId)
                .Select(r => r.CommerceId)
                .SingleOrDefaultAsync();
        }

    }
}
