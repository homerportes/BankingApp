using BankingApp.Core.Application.Interfaces;
using BankingApp.Infraestructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Infraestructure.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BankingContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork( BankingContext context)
        {
           _context=context; 
        }
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public  async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
            await _transaction!.CommitAsync();
        }

        public  async Task RollbackAsync()
        {
            await _transaction!.RollbackAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
         return   await _context.SaveChangesAsync();

    }
}
}
