using BankingApp.Core.Domain.Common.Enums;
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


        public async Task<bool> AccountExists(string accountNumber)
        {


            return await _context.Set<Account>().AnyAsync(r => r.Number == accountNumber);


        }

        public async Task<int> CountSavingAccountsByUserIds(HashSet<string> userIds)
        {
            return await _context.Set<Account>()
                .CountAsync(a => userIds.Contains(a.UserId) && a.Type == AccountType.PRIMARY);
        }




        public async Task<Account?> CreditBalance(string number, decimal amount)
        {
            var entry = await _context.Set<Account>().FirstOrDefaultAsync(s => s.Number == number);

            if (entry != null)
            {
                entry.Balance += amount;
                _context.Set<Account>().Update(entry);
            }

            return entry;
        }



        public async Task<Account?> DebitBalance(string number, decimal amount)
        {
            var entry = await _context.Set<Account>().FirstOrDefaultAsync(s => s.Number == number);

            if (entry != null)
            {
                entry.Balance -= amount;
                _context.Set<Account>().Update(entry);
            }

            return entry;
        }



        public  async Task<Account?> GetAccountByNumber(string accountNumber)
        {
           
            return  await _context.Set<Account>().FirstOrDefaultAsync(r => r.Number == accountNumber); 

        }



        public async Task<List<Account>> GetAllListByIdAsync(string IdCliente)
        {

            return  await _context.Set<Account>().Where(s => s.UserId == IdCliente).ToListAsync();

        }




        public async Task<Account?> GetAccounByIdAsync(string IdCliente)
        {

            return await _context.Set<Account>().FirstOrDefaultAsync(s => s.UserId == IdCliente && s.Status == AccountStatus.ACTIVE);

        }


    }
}
