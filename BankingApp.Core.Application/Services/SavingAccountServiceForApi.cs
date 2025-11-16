using AutoMapper;
using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Repositories;


namespace BankingApp.Core.Application.Services
{
    public class SavingAccountServiceForApi : BaseSavingAccountService, ISavingAccountServiceForApi
    {

        private readonly IAccountRepository _repo;
        private readonly IMapper _mapper;
        public SavingAccountServiceForApi(IAccountRepository repo, IMapper mapper, ITransacctionRepository transactionRepository,IUnitOfWork unitOfWork) : base(repo, mapper, transactionRepository, unitOfWork)
        {
            _repo = repo;
            _mapper = mapper;
        }

    
    }
}
