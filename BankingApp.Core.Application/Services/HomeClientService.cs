using AutoMapper;
using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Application.Dtos.Transaction;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Repositories;
using System.Net.WebSockets;


namespace BankingApp.Core.Application.Services
{
    public class HomeClientService : IHomeClietService
    {

        private readonly IMapper _mapper;
        private readonly IAccountRepository accountRepository;
        private readonly ITransactionService transactionService;
        private readonly ITransacctionRepository transacctionRepository;

        
        
        
        public HomeClientService(IMapper _mapper, 
            IAccountRepository accountRepository, 
            ITransacctionRepository transacctionRepository, 
            ITransactionService transactionService)
        {
             
            this._mapper = _mapper;
            this.accountRepository = accountRepository;
            this.transacctionRepository = transacctionRepository;   
            this.transactionService = transactionService;
        
        }




        public async Task<List<DataAccountaHomeClientDto>> GetDataAccountClient(string idUser)
        {
            try
            {

                var ListAccount = await accountRepository.GetAllListByIdClienteAsync(idUser);

                if (ListAccount == null)
                {
                   return new List<DataAccountaHomeClientDto>();
                
                }

                var _entities = ListAccount
                 .OrderBy(s => s.Type != AccountType.PRIMARY)
                 .ThenByDescending(s => s.Balance)
                 .ToList();


                var entities = _mapper.Map<List<DataAccountaHomeClientDto>>(_entities);

                return entities;
            }
            catch (Exception ex) 
            {

                return new List<DataAccountaHomeClientDto>();
            }

        }









        public async Task<List<DataTransactionHomeClientDto>> GetDataListTransaction(string number)
        {
            try
            {


                var entities = await transacctionRepository.GetListTransaction(number);


                if(entities == null)
                {



                    return new List<DataTransactionHomeClientDto>();
                
                }


                var data = _mapper.Map<List<DataTransactionHomeClientDto>>(entities);
            
               return data;    


            }
            catch (Exception ex)
            {
            
               
                return new List<DataTransactionHomeClientDto>();    
            
            
            }
        }




    }
}
