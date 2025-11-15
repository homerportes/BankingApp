using AutoMapper;
using BankingApp.Core.Application.Dtos.Transaction;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Repositories;


namespace BankingApp.Core.Application.Services
{
    public class CashAdvanceService : GenericService<Transaction, CreateTransactionDto>, ICashAdvancesServices
    {

        private readonly IMapper _mapper;
        private readonly ICreditCardRepository _cardRepository;
        private readonly ITransacctionRepository _transacctionRepository;   
        private readonly IUnitOfWork _unitOfWork;    



        public CashAdvanceService(IGenericRepository<Transaction> repo, 
                                 IMapper mapper, 
                                 ICreditCardRepository cardRepository, 
                                 IUnitOfWork _unitOfWork, 
                                 ITransacctionRepository _transacctionRepository
                                 ) : base(repo, mapper)
        {

            _mapper = mapper;
            _cardRepository = cardRepository;
            this._unitOfWork = _unitOfWork;
            this._transacctionRepository = _transacctionRepository;
            
        }






        public override async Task<CreateTransactionDto?> AddAsync(CreateTransactionDto Dto)
        {

            await _unitOfWork.BeginTransactionAsync();
            try
            {

                var _validatelAmountOwedCreditCard = await ValidateTotalAmountOwedCreditCard(Dto.Origin,Dto.Amount);
                if (_validatelAmountOwedCreditCard == null)
                {
                    return null;

                }


               
                var entity = _mapper.Map<Transaction>(Dto);
                if (entity is not null)
                {
                    var transac = await _transacctionRepository.AddAsync(entity);
                    var dto = _mapper.Map<CreateTransactionDto>(transac);
                    await _unitOfWork.CommitAsync();
                    return dto;
                }


                return null;

            }
            catch
            {

                await _unitOfWork.RollbackAsync();
                return default;

            }
        }








        public async Task<bool> CreditTotalAmountOwedAsync(string number, decimal amount)
        {
            try
            {



                var entity = await _cardRepository.CreditTotalAmountOwedAsync(number,amount);

                if(entity == null)
                {
                 
                     return false;
                
                }


                return true;

            }catch(Exception ex)
            {


                return false;
             
            
            
            }

        }







        public async Task<ValidateTotalAmountOwedResponseDto> ValidateTotalAmountOwedCreditCard(string number, decimal amount)
        {

            var response = new ValidateTotalAmountOwedResponseDto() { HasError = false, Error = "" };

            try
            {

                var creditCard = await _cardRepository.GetByNumberAsync(number);
                decimal interest = 1.0625m;

                if (creditCard == null)
                {
                   
                     response.HasError = true;
                     response.Error = "Ocurrio un error, no pudo encontrar una tarjeta con el numero indicado";
                    
                }


                if (creditCard!.CreditLimitAmount < amount * interest)
                {
                       response.HasError = true;
                       response.Error = "EL avance no se pudo realizar, El monto digitado  mas los intereses superan el limite de credito de la tarjeta";

                }




                if (creditCard!.CreditLimitAmount >= amount * interest)
                {

                    decimal AmountValidate = creditCard.TotalAmountOwed + amount * interest;

                    if(AmountValidate > creditCard.CreditLimitAmount)
                    {

                        response.HasError = true;
                        response.Error = "EL avance no se pudo realizar, El total adeudado  mas el avances y los intereses a general superan el limite de  credito de la tarjeta";

                    }

                }



                response.AccountId = creditCard.Id;
                return response;


                
            }catch(Exception ex)
            {


                response.HasError = true;
                response.Error = ex.Message;
                return response;
            
            }

        }






    }

}
