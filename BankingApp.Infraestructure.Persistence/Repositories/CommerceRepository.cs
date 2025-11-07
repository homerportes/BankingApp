using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Contexts;


namespace BankingApp.Infraestructure.Persistence.Repositories
{
    public class CommerceRepository : GenericRepository<Commerce>, ICommerceRepository
    {
        public CommerceRepository(BankingContext context) : base(context)
        {
        }
    }
}
