using AutoMapper;
using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Application.Dtos.User;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
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
        private readonly ISavingAccountServiceForApi _SavingAccountService;

        public UserAccountManagmentService(IUnitOfWork unitOfWork, IMapper mapper, IAccountServiceForWebAPP accountUserService, ISavingAccountServiceForApi SavingAccountService)
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
            };                await _SavingAccountService.AddAsync(accountDto);

                response = _mapper.Map<RegisterUserWithAccountResponseDto>(user);
                response.EntityId = user.Id;
                response.IsSuccesful = true;
                response.StatusMessage = "Usuario y cuenta creados exitosamente.";

                await _unitOfWork.CommitAsync();
                return response;
            }
            catch (Exception ex)
            {
                // Solo hacer rollback si la transacción aún está activa
                try
                {
                    await _unitOfWork.RollbackAsync();
                }
                catch
                {
                    // La transacción ya fue completada
                }

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
                var editDto = await _accountUserService.EditUser(saveUserDto, null, true, ForApi);

                if (editDto.HasError)
                {
                    response.IsSuccesful = false;
                    response.UserAlreadyExists = editDto.HasError;
                    response.StatusMessage = editDto.Message;
                    await _unitOfWork.RollbackAsync();
                    return response;
                }
                
                // Si hay monto adicional, obtener cuenta y actualizar balance
                if (request.AdditionalBalance.HasValue && request.AdditionalBalance > 0)
                {
                    if (string.IsNullOrEmpty(request.Id))
                    {
                        response.IsSuccesful = false;
                        response.StatusMessage = "El ID del usuario es requerido para agregar monto adicional.";
                        await _unitOfWork.RollbackAsync();
                        return response;
                    }

                    var account = await _SavingAccountService.GetAccountByClientId(request.Id);

                    if (account == null)
                    {
                        response.IsSuccesful = false;
                        response.StatusMessage = "No se encontró una cuenta asociada al usuario.";
                        await _unitOfWork.RollbackAsync();
                        return response;
                    }

                    account.Balance += request.AdditionalBalance.Value;
                    await _SavingAccountService.UpdateAsync(account.Id, account);
                }

                await _unitOfWork.CommitAsync();

                response = _mapper.Map<RegisterUserWithAccountResponseDto>(editDto);
                response.IsSuccesful = true;
                response.StatusMessage = "Usuario actualizado exitosamente.";
                return response;

            }
            catch (Exception ex)
            {
                // Solo hacer rollback si la transacción aún está activa
                try
                {
                    await _unitOfWork.RollbackAsync();
                }
                catch
                {
                    // La transacción ya fue completada (commit o rollback)
                }

                response.IsSuccesful = false;
                response.IsInternalError = true;
                response.StatusMessage = $"Error interno: {ex.Message}";
                return response;
            }
        }


        public async Task<AccountDto> GetMainSavingAccount (string clientId)
        {
            var account= await _SavingAccountService.GetAccountByClientId (clientId);
            return _mapper.Map<AccountDto>(account);
        }


        public async Task ChangeBalanceForClient(string id, decimal AdditionalBalance)
        {
            var account = await _SavingAccountService.GetAccountByClientId(id);
            
            if (account == null)
            {
                throw new InvalidOperationException($"No se encontró cuenta para el cliente con ID: {id}");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                account.Balance += AdditionalBalance;
                await _SavingAccountService.UpdateAsync(account.Id, account);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                try
                {
                    await _unitOfWork.RollbackAsync();
                }
                catch
                {
                    // La transacción ya fue completada
                }
                throw;
            }
        }
    }
}
