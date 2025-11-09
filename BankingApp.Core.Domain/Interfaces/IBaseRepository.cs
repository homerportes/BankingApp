using BankingApp.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Domain.Interfaces
{
    public interface IBaseRepository<Tentity> where Tentity : class
    {

        Task<Tentity> AddAsync(Tentity entity);
        public Task<Tentity?> GetByIdAsync(int id);
        Task  DeleteAsync(int id);
        Task<List<Tentity>?> GetAllList();


    }
}
