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
    public class AccountRepository : GenericRepository<Account>, IAccountRepository
    {
        private readonly BankingContext _context;
        public AccountRepository(BankingContext context) : base(context)
        {
           _context = context;
        }

        public async Task <bool> AccountExists(string accountNumber)
        {
         return  await _context.Set<Account>().AnyAsync(r=>r.Number == accountNumber);
        }
    }
}
