using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Contexts;


namespace BankingApp.Infraestructure.Persistence.Repositories
{
    public class InstallmentRepository : GenericRepository<Installment>, IInstallmentRepository
    {
        public InstallmentRepository(BankingContext context) : base(context)
        {
        }
    }
}
