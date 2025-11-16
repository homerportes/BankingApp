using AutoMapper;
using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Application.Dtos.Transaction.Transference;
using BankingApp.Core.Application.Dtos.User;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Services
{
    public class UserAccountManagmentService :IUserAccountManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAccountServiceForWebAPP _accountUserService;
        private readonly ISavingsAccountServiceForWebApp _SavingAccountService;

        public UserAccountManagmentService(IUnitOfWork unitOfWork, IMapper mapper, IAccountServiceForWebAPP accountUserService , ISavingsAccountServiceForWebApp SavingAccountService)
        {
           _unitOfWork = unitOfWork;
            _mapper = mapper;
            _accountUserService=accountUserService;
            _SavingAccountService = SavingAccountService;
        }
        public async Task<RegisterUserWithAccountResponseDto> CreateUserWithAmount(CreateUserDto request, string AdminId, bool ForApi=false, string? origin=null)
        {
            var response = new RegisterUserWithAccountResponseDto();

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var saveUserDto = _mapper.Map<SaveUserDto>(request);
                var user = await _accountUserService.RegisterUser(saveUserDto, origin, ForApi);

                if (user.HasError)
                {
                    response.IsSuccesful = false;
                    response.UserAlreadyExists = true;
                    response.StatusMessage = user.Message;

                    await _unitOfWork.RollbackAsync();
                    return response;
                }

                var accountDto = new AccountDto
                {
                    UserId = user.Id,
                    Id = 0,
                    Type = AccountType.PRIMARY,
                    Number = await _SavingAccountService.GenerateAccountNumber(),
                    Balance = request.InitialAmount ?? 0,
                    AdminId = AdminId
                };

                var accountResult=await _SavingAccountService.AddAsync(accountDto);
                if(accountResult==null)
                {
                    response.IsSuccesful = false;
                    response.IsInternalError = true;
                    await _unitOfWork.RollbackAsync();

                }
                var reponsemap = _mapper.Map<RegisterUserWithAccountResponseDto>(user);
                response = reponsemap;
                response.EntityId = reponsemap.EntityId;
                response.IsSuccesful = true;
                response.StatusMessage = "Usuario y cuenta creados exitosamente.";

                await _unitOfWork.CommitAsync();
                return response;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();

                response.IsSuccesful = false;
                response.IsInternalError = true;
                response.StatusMessage = $"Error interno: {ex.Message}";
                return response;
            }
        }

        public async Task<RegisterUserWithAccountResponseDto> EditUserAndAmountAsync(UpdateUserDto request, string AdminId, bool ForApi = false, string? origin = null)
        {
            var response = new RegisterUserWithAccountResponseDto();
          
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Editar usuario
                var saveUserDto = _mapper.Map<SaveUserDto>(request);
                 var editDto =await _accountUserService.EditUser(saveUserDto, null, true, ForApi);

                if (editDto.HasError)
                {
                    response.IsSuccesful = false;
                    response.UserAlreadyExists = editDto.HasError;
                    response.StatusMessage = editDto.Message;
                    await _unitOfWork.RollbackAsync();
                    return response;
                }
                // Obtener cuenta y actualizar balance
                var account = await _SavingAccountService.GetAccountByClientId(request.Id??"");

                if (request.AdditionalBalance > 0 && account!=null)
                {
                    account.Balance += request.AdditionalBalance.Value;
                 await _SavingAccountService.UpdateAsync(account.Id, account);
                }

                await _unitOfWork.CommitAsync();

                response = _mapper.Map<RegisterUserWithAccountResponseDto>(editDto);
                response.IsSuccesful = true;
                return response;

            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                response.IsSuccesful = false;
                response.IsInternalError = true;
                return response;
            }
        }


        public async Task<AccountDto?> GetMainSavingAccount (string clientId)
        {
            var account= await _SavingAccountService.GetAccountByClientId (clientId);
            if (account == null) return null;
            return _mapper.Map<AccountDto>(account);
        }


        public async Task ChangeBalanceForClient(string id, decimal AdditionalBalance)
        {
            var account = await _SavingAccountService.GetAccountByClientId(id);
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (account != null)
                {
                    account!.Balance += AdditionalBalance;
                    await _SavingAccountService.UpdateAsync(account.Id, account);
                }
              
                await _unitOfWork.CommitAsync();

            }
            catch
            {
                await _unitOfWork.RollbackAsync();
            }
        }


        public async Task<List<string>> GetCurrentUserActiveAccounts(string currentUserName)
        {
            var accounts = new List<string>();

            var user = await _accountUserService.GetUserByUserName(currentUserName);
            if (user == null) return accounts;

            accounts = await _SavingAccountService.GetActiveAccountsByClientId(user.Id);

            return accounts ?? new List<string>();
        }

        public async Task<bool> AccountHasEnoughFounds(string accountNumber, decimal requestAmount)
        {
            return await _SavingAccountService.AccountHasEnoughFounds (accountNumber, requestAmount);
        }


        public async Task<TransferenceResponseDto> TransferAmountToAccount(TransferenceRequestDto tranferenceRequest)
        {
            return await _SavingAccountService.ExecuteTransference(tranferenceRequest);
        }

    }
}
