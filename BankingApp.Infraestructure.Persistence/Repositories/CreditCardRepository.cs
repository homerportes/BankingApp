using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

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


        private string HashCVC(string cvc)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(cvc);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public async Task<bool> CardDataIsValidForPaymentAsync(string cardNumber, int monthExpiration, int yearExpiration, string cvc)
        {
            if (string.IsNullOrWhiteSpace(cardNumber) || string.IsNullOrWhiteSpace(cvc))
                return false;

            var card = await _context.Set<CreditCard>()
                                     .FirstOrDefaultAsync(c => c.Number == cardNumber);

            if (card == null)
                return false;

            if (card.Status != CardStatus.ACTIVE)
                return false;

            DateTime providedExpiration;
            try
            {
                providedExpiration = new DateTime(yearExpiration, monthExpiration, DateTime.DaysInMonth(yearExpiration, monthExpiration));
            }
            catch
            {
                return false; 
            }

            if (card.ExpirationDate != providedExpiration)
                return false;


            var hashed = HashCVC(cvc);
            if (card.CVC != hashed)
                return false;

            return true;
        }

        public async Task<bool> CreditCardHasEnoughFunds(string cardNumber, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(cardNumber) || amount <= 0)
                return false;

            var card = await _context.Set<CreditCard>()
                                     .FirstOrDefaultAsync(r => r.Number == cardNumber);

            if (card == null)
                return false;

            if (card.TotalAmountOwed < 0 || card.TotalAmountOwed >= card.CreditLimitAmount)
                return false;

            var availableAmount = card.CreditLimitAmount - card.TotalAmountOwed;

            return availableAmount >= amount && card.TotalAmountOwed+amount<=card.CreditLimitAmount;
        }

        public async Task<CreditCard?> DebitTotalAmountOwedAsync(string number,decimal Amount)
        {

            var entity = await _context.Set<CreditCard>().FirstOrDefaultAsync(c => c.Number == number);


            if(entity is not null)
            {
                
                  entity.TotalAmountOwed = entity.TotalAmountOwed - Amount;
                 _context.Update(entity);
                  await _context.SaveChangesAsync();
                 return entity;
                
            }

            return entity;

        }




        public async Task<CreditCard?> CreditTotalAmountOwedAsync(string number, decimal Amount)
        {

            var entity = await _context.Set<CreditCard>().FirstOrDefaultAsync(c => c.Number == number);


            if (entity is not null)
            {

                entity.TotalAmountOwed = entity.TotalAmountOwed + Amount;
                _context.Update(entity);
                await _context.SaveChangesAsync();
                return entity;

            }

            return entity;

        }





        public async Task<List<CreditCard>> GetActiveByClientIdAsync(string clientId)
        {
            return await _context.Set<CreditCard>().Where( c => c.ClientId == clientId && c.Status == CardStatus.ACTIVE)
                .ToListAsync();
        }
       


        public async Task< int> GetTotalIssuedCreditCards()
        {

            return await _context.Set<CreditCard>().CountAsync();
        }
        public async Task<int> GetTotalActiveCreditCards()
        {

            return await _context.Set<CreditCard>().Where(r=>r.Status==CardStatus.ACTIVE).CountAsync();
        }

        public async Task<int> GetTotalActiveCreditCardsWithClient()
        {

            return await _context.Set<CreditCard>().Where(r => r.ClientId !=null && r.Status== CardStatus.ACTIVE).CountAsync();
        }

        public async Task<int> GetTotalCreditCardsWithClient()
        {

            return await _context.Set<CreditCard>().Where(r => r.ClientId != null).CountAsync();
        }


        public async Task<decimal> GetActiveClientsCreditCardDebt(HashSet<string> ids)
        {
            return await _context.Set<CreditCard>()
                  .Where(r => r.Status==CardStatus.ACTIVE && ids.Contains(r.ClientId)).
                  SumAsync(l => l.TotalAmountOwed);
        }

        public async Task<decimal> GetTotalClientsCreditCardDebt()
        {
            return await _context.Set<CreditCard>()
                  .SumAsync(l => l.TotalAmountOwed);
        }
        public async Task<decimal> GetClientTotalCreditCardDebt(string ClientId)
        {
            return await _context.Set<CreditCard>()
                .Where(r=>r.ClientId==ClientId)
                  .SumAsync(l => l.TotalAmountOwed);
        }



    }
}
