using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Entities;
using BankingApp.Infraestructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BankingApp.Infraestructure.Persistence.Repositories
{
    public class TransacctionRepository : GenericRepository<Transaction>, ITransacctionRepository
    {

        private readonly BankingContext context;

        public TransacctionRepository(BankingContext context) : base(context)
        {

            this.context = context;
        }



        public  string GenerateOperationId()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();
        }
        public async Task<bool> ApproveTransaction(int id, Transaction transaction)
        {
            var entry = await context.Set<Transaction>().FindAsync(id);

            if (entry != null)
            {

                entry.Status = OperationStatus.APPROVED;
                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }



        public async Task<bool> DeclieneTransaction(int id, Transaction transaction)
        {
            var entry = await context.Set<Transaction>().FindAsync(id);

            if (entry != null)
            {

                entry.Status = OperationStatus.DECLINED;
                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }


        public async Task<bool> MarkAsCredit(int id, Transaction transaction)
        {
            var entry = await context.Set<Transaction>().FindAsync(id);

            if (entry != null)
            {

                entry.Type = TransactionType.CREDIT;
                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }



        public async Task<bool> MarkAsDebit(int id, Transaction transaction)
        {

            var entry = await context.Set<Transaction>().FindAsync(id);

            if (entry != null)
            {

                entry.Type = TransactionType.DEBIT;
                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<List<Transaction>> GetTransactionsByTellerAndDateAsync(string tellerId, DateTime startDate, DateTime endDate)
        {
            try
            {
                return await context.Set<Transaction>()
                    .Where(t => t.TellerId == tellerId && t.DateTime >= startDate && t.DateTime < endDate)
                    .ToListAsync();
            }
            catch
            {
                return new List<Transaction>();
            }
        }


        public async Task<List<Transaction>> GetListTransaction(string number)
        {

            return await context.Set<Transaction>().Where(t => t.Origin == number || t.Beneficiary == number).ToListAsync();

        }



        public async Task<List<Transaction>> GetListTransactionByNumberCreditCard(string number)
        {

            return await context.Set<Transaction>().Where(t => t.Origin == number ).ToListAsync();

        }



    }
}
