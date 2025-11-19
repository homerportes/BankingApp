using AutoMapper;
using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Application.Dtos.CreditCard;
using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Application.Dtos.Transaction;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Repositories;
using System.Diagnostics.Metrics;
using static System.Runtime.InteropServices.JavaScript.JSType;



namespace BankingApp.Core.Application.Services
{
    public class HomeClientService : IHomeClietService
    {

        private readonly IMapper _mapper;
        private readonly IAccountRepository accountRepository;
        private readonly ITransactionService transactionService;
        private readonly ILoanRepository loanRepository;
        private readonly IInstallmentRepository installmentRepository;
        private readonly ITransacctionRepository transacctionRepository;
        private readonly ICreditCardRepository creditCardRepository;
        private readonly IPurchaseRepository purchaseRepository;

        
        
        
        public HomeClientService(IMapper _mapper, 
            IAccountRepository accountRepository, 
            ITransacctionRepository transacctionRepository, 
            ITransactionService transactionService,
            ILoanRepository loanRepository,
            IInstallmentRepository installmentRepository,
            ICreditCardRepository creditCardRepository,
             IPurchaseRepository purchaseRepository)
        {
             
            this._mapper = _mapper;
            this.accountRepository = accountRepository;
            this.transacctionRepository = transacctionRepository;   
            this.transactionService = transactionService;
            this.loanRepository = loanRepository;
            this.installmentRepository = installmentRepository;
            this.creditCardRepository = creditCardRepository;
            this.purchaseRepository = purchaseRepository;
        
        }




        public async Task<List<DataAccountaHomeClientDto>> GetDataAccountClient(string idUser)
        {
            try
            {

                var ListAccount = await accountRepository.GetAllListByIdAsync(idUser);

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
            var txs = await transacctionRepository.GetListTransaction(number);

            if (txs == null || !txs.Any())
                return new List<DataTransactionHomeClientDto>();

            var filtered = txs.Where(t =>
            {
                bool isBidirectional =
                    t.Description == DescriptionTransaction.Transaccion_Express ||
                    t.Description == DescriptionTransaction.Transaccion_A_Beneficiario ||
                    t.Description == DescriptionTransaction.TRANSFER ||
                    t.Description == DescriptionTransaction.Cash_Advance;

                if (isBidirectional)
                {
                  
                    if (t.Origin == number)
                        return t.Type == TransactionType.DEBIT;

                    if (t.Beneficiary == number)
                        return t.Type == TransactionType.CREDIT;

                    return false;
                }

             
                return t.Origin == number || t.Beneficiary == number;

            })
            .OrderByDescending(t => t.DateTime)
            .ToList();
           

            return _mapper.Map<List<DataTransactionHomeClientDto>>(filtered);
        }




        public async Task<List<DataLoanHomeClientDto>> GetDataLoanHomeClient(string ClientId)
        {
            try
            {
                var entities = await loanRepository.GetLoanListByIdClient(ClientId);

                if (entities == null || !entities.Any())
                {
                    return new List<DataLoanHomeClientDto>();
                }

                var unitLoan = entities.DistinctBy(d => d.ClientId);


                List<DataLoanHomeClientDto> result = new List<DataLoanHomeClientDto>();

                foreach (var loan in unitLoan)
                {
                    var dto = _mapper.Map<DataLoanHomeClientDto>(loan);

                    var installments = await installmentRepository.GetTotalInstallamentByLoanId(dto.Id);

                    dto.installmentsTotalAmountPaid = installments.Count(s => s.IsPaid);
                    dto.installmentsTotalAmount = installments.Count();
                    dto.IsDelinquent = installments.Any(x => x.IsDelinquent);

                
                   

                    result.Add(dto);
                }

                return result;
            }
            catch (Exception)
            {
                return new List<DataLoanHomeClientDto>();
            }
        }




        public async Task<List<DetailsLoanHomeClientDto>> GetDetailsLoanHomeClient(Guid number)
        {
            try
            {


                var entities = await installmentRepository.GetTotalInstallamentByLoanId(number);

                var ListaIntallment = new List<DetailsLoanHomeClientDto>();

                if (entities == null || !entities.Any())
                {
                    return new List<DetailsLoanHomeClientDto>();

                }


                foreach (var entity in entities)
                {
                    var dto = _mapper.Map<DetailsLoanHomeClientDto>(entity);
                  
                    ListaIntallment.Add(dto);
                }



                return ListaIntallment;


            }
            catch (Exception ex)
            {

                return new List<DetailsLoanHomeClientDto>();

            }
        }



        public async Task<List<DataHomeClientCreditCardDto>> GetDetaCreditCardHomeClient(string idUsuario)
        {
            try
            {

                var entities = await creditCardRepository.GetActiveByClientIdAsync(idUsuario);

                if (entities == null || !entities.Any())
                {

                    return new List<DataHomeClientCreditCardDto>();
                    
                }


                var ListData = _mapper.Map<List<DataHomeClientCreditCardDto>>(entities);
                return ListData;

            }catch(Exception ex)
            {



                return new List<DataHomeClientCreditCardDto>();


            
            }
        }




        public async Task<List<DetailsCreditCardHomeClientDto>> GetDetailsCreditCardHomeClient(string number)
        {
            try
            {

                var purchases = await purchaseRepository.GetByCardNumberAsync(number);
  

                if (purchases == null || !purchases.Any())
                    return new List<DetailsCreditCardHomeClientDto>();



                var dto = _mapper.Map<List<DetailsCreditCardHomeClientDto>>(purchases);
                return dto;

            }
            catch (Exception)
            {

                return new List<DetailsCreditCardHomeClientDto>();

            }
        }


















     
    }
}
