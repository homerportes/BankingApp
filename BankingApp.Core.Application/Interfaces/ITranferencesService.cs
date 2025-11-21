using BankingApp.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Interfaces
{
    public interface ITranferencesService
    {
        Task<Transaction> CreateDeclinedTransactionAsync(string accountFrom, string accountTo, decimal amount);
    }
}
