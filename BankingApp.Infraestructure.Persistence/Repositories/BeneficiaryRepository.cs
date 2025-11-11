

using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Infraestructure.Persistence.Repositories
{
    public class BeneficiaryRepository : GenericRepository<Beneficiary>,IBeneficiaryRepository
    {

        private readonly BankingContext context;


        public BeneficiaryRepository(BankingContext context) : base(context)
        {

            this.context = context;

        }


        public async  Task<List<Beneficiary>> GetBeneficiariesByIdCliente(string IdCliente)
        {
           
            return await context.Set<Beneficiary>()
                .Where(s => s.ClientId == IdCliente).ToListAsync(); 

        }
    }}
