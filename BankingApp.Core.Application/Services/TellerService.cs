using BankingApp.Core.Application.Dtos.Email;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.ViewModels.Teller;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Repositories;
using System.Buffers;
using OperationStatus = BankingApp.Core.Domain.Common.Enums.OperationStatus;

namespace BankingApp.Core.Application.Services
{
    /// <summary>
    /// Servicio para las operaciones del cajero (Teller)
    /// </summary>
    public class TellerService : ITellerService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransacctionRepository _transactionRepository;
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly IInstallmentRepository _installmentRepository;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITransactionToLoanService loanService;

        public TellerService(
            IAccountRepository accountRepository,
            ITransacctionRepository transactionRepository,
            ICreditCardRepository creditCardRepository,
            ILoanRepository loanRepository,
            IInstallmentRepository installmentRepository,
            IUserService userService,
            IEmailService emailService,
            IUnitOfWork unitOfWork,
            ITransactionToLoanService loanService)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _creditCardRepository = creditCardRepository;
            _loanRepository = loanRepository;
            _installmentRepository = installmentRepository;
            _userService = userService;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            this.loanService = loanService;
        }

        public async Task<TellerDashboardViewModel> GetTellerDashboardDataAsync(string tellerId)
        {
            try
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                var tellerTransactions = await _transactionRepository.GetTransactionsByTellerAndDateAsync(tellerId, today, tomorrow);

                var tellerTransactionApprove =  tellerTransactions.Where(t => t.Status == OperationStatus.APPROVED).ToList();

                var dashboard = new TellerDashboardViewModel
                {
                    TotalTransactionsToday = tellerTransactionApprove.Count,
                    TotalDepositsToday = tellerTransactionApprove.Count(t => t.Type == TransactionType.CREDIT && t.Origin == "DEPÓSITO"),
                    TotalWithdrawalsToday = tellerTransactionApprove.Count(t => t.Type == TransactionType.DEBIT && t.Beneficiary == "RETIRO"),
                    TotalPaymentsToday = tellerTransactionApprove.Count(t =>
                        (t.Type == TransactionType.DEBIT && t.Beneficiary != "RETIRO" && t.Beneficiary.Length > 9) ||
                        (t.Type == TransactionType.DEBIT && t.Beneficiary.Length == 9))
                };

                return dashboard;
            }
            catch
            {
                return new TellerDashboardViewModel();
            }
        }

        #region Validaciones

        public async Task<(bool IsValid, string AccountHolderName, string Message)> ValidateAccountForDepositAsync(string accountNumber)
        {
            try
            {
                var account = await _accountRepository.GetAccountByNumber(accountNumber);

                if (account == null)
                {
                return (false, string.Empty, "El número de cuenta ingresado no es válido.");
            }

            if (account.Status != AccountStatus.ACTIVE)
            {
                return (false, string.Empty, "La cuenta está inactiva.");
            }

            var user = await _userService.GetUserById(account.UserId);
            if (user == null)
            {
                return (false, string.Empty, "No se encontró el titular de la cuenta.");
            }

            var accountHolderName = $"{user.Name} {user.LastName}";
            return (true, accountHolderName, string.Empty);
            }
            catch
            {
                return (false, string.Empty, "Error al validar la cuenta.");
            }
        }

        public async Task<(bool IsValid, string AccountHolderName, decimal Balance, string Message)> ValidateAccountForWithdrawalAsync(string accountNumber,decimal amount, string tellerId)
        {
            try
            {
                var account = await _accountRepository.GetAccountByNumber(accountNumber);

                if (account == null)
                {
                    return (false, string.Empty, 0, "El número de cuenta ingresado no es válido.");
                }

                if (account.Status != AccountStatus.ACTIVE)
                {
                    return (false, string.Empty, 0, "La cuenta está inactiva.");
                }

                var user = await _userService.GetUserById(account.UserId);
                if (user == null)
                {
                    return (false, string.Empty, 0, "No se encontró el titular de la cuenta.");
                }


                if(account.Balance < amount)
                {
                    var operationId1 = _transactionRepository.GenerateOperationId();
                    var transactionDeclined = new Transaction
                    {
                        Amount = amount,
                        DateTime = DateTime.Now,
                        Type = TransactionType.DEBIT,
                        Origin = accountNumber,
                        Beneficiary = "RETIRO",
                        AccountNumber = accountNumber,
                        AccountId = account.Id,
                        Status = OperationStatus.DECLINED,
                        Description = DescriptionTransaction.WITHDRAWAL,

                        TellerId = tellerId
                    };

                    await _transactionRepository.AddAsync(transactionDeclined);
                    return (false, string.Empty, 0, "El monto digitado excede el balance disponnble de la cuenta.");

                }

                var accountHolderName = $"{user.Name} {user.LastName}";
                return (true, accountHolderName, account.Balance, string.Empty);
            }
            catch
            {
                return (false, string.Empty, 0, "Error al validar la cuenta.");
            }
        }

        public async Task<(bool IsValid, string CardHolderName, decimal CurrentDebt, string Message)> ValidateCreditCardPaymentAsync(string accountNumber, string cardNumber,decimal amount, string tellerId)
        {
            try
            {
                var account = await _accountRepository.GetAccountByNumber(accountNumber);
                if (account == null || account.Status != AccountStatus.ACTIVE)
                {
                    return (false, string.Empty, 0, "La cuenta origen no es válida o está inactiva.");
                }

                var creditCard = await _creditCardRepository.GetByNumberAsync(cardNumber);
                if (creditCard == null || creditCard.Status != CardStatus.ACTIVE)
                {
                    return (false, string.Empty, 0, "El número de tarjeta ingresado no es válido o está inactiva.");
                }

                if (creditCard.TotalAmountOwed <= 0)
                {
                    var operationId = _transactionRepository.GenerateOperationId();
                    // Registrar la transacción rechazada
                    var transaction = new Transaction
                    {
                        Amount = amount,
                        DateTime = DateTime.Now,
                        Type = TransactionType.DEBIT,
                        Origin = accountNumber,
                        Beneficiary = cardNumber,
                        AccountNumber = accountNumber,
                        AccountId = account.Id,
                        Status = OperationStatus.DECLINED,
                        Description = DescriptionTransaction.CREDITCARDPAYMENT,
                        TellerId = tellerId,
                    };

                    await _transactionRepository.AddAsync(transaction);
                    return (false, string.Empty, 0, "La tarjeta no tiene deuda pendiente.");
                }




                if (account.Balance < amount)
                {
                    var operationId = _transactionRepository.GenerateOperationId();

                    // Registrar la transacción rchazada
                    var transaction = new Transaction
                    {
                        Amount = amount,
                        DateTime = DateTime.Now,
                        Type = TransactionType.DEBIT,
                        Origin = accountNumber,
                        Beneficiary = cardNumber,
                        AccountNumber = accountNumber,
                        AccountId = account.Id,
                        Status = OperationStatus.DECLINED,
                        Description = DescriptionTransaction.CREDITCARDPAYMENT,
                        TellerId = tellerId,
                    };

                    await _transactionRepository.AddAsync(transaction);
                    return (false, string.Empty, 0, "El monto excede el saldo disponible de la cuenta.");
                }



                var user = await _userService.GetUserById(creditCard.ClientId);
                if (user == null)
                {
                    return (false, string.Empty, 0, "No se encontró el titular de la tarjeta.");
                }



                var cardHolderName = $"{user.Name} {user.LastName}";
                return (true, cardHolderName, creditCard.TotalAmountOwed, string.Empty);
            }
            catch
            {
                return (false, string.Empty, 0, "Error al validar el pago.");
            }
        }

        public async Task<(bool IsValid, string LoanHolderName, decimal RemainingBalance, string Message, Guid IdLoan)> ValidateLoanPaymentAsync(string accountNumber, string loanNumber,decimal amount,string tellerId)
        {
            try
            {
              

                var loan = await _loanRepository.GetByNumberAsync(loanNumber);
                if (loan == null || !loan.IsActive)
                {
                    return (false, string.Empty, 0, "El número de préstamo ingresado no es válido o está completado.",loan!.Id);
                }


                var account = await _accountRepository.GetAccountByNumber(accountNumber);
                if (account == null || account.Status != AccountStatus.ACTIVE)
                {
                    return (false, string.Empty, 0, "La cuenta origen no es válida o está inactiva.", loan!.Id);
                }

                if (account.Balance < amount )
                {


                    var operationId = _transactionRepository.GenerateOperationId();
                    // Registrar la transacción
                    var transaction = new Transaction
                    {
                        Amount = amount,
                        DateTime = DateTime.Now,
                        Type = TransactionType.DEBIT,
                        Origin = accountNumber,
                        Beneficiary = loanNumber,
                        AccountNumber = accountNumber,
                        AccountId = account.Id,
                        Status = OperationStatus.DECLINED,
                        Description = DescriptionTransaction.LOANPAYMENT,
                        TellerId = tellerId,
                    };


                    await _transactionRepository.AddAsync(transaction);
                    return (false, string.Empty, 0, "El balance digitado excede el saldo disponible..", loan!.Id);
                }

                var user = await _userService.GetUserById(loan.ClientId);
                if (user == null)
                {
                    return (false, string.Empty, 0, "No se encontró el titular del préstamo.", loan!.Id);
                }

                var loanHolderName = $"{user.Name} {user.LastName}";
                return (true, loanHolderName, loan.OutstandingBalance, string.Empty, loan!.Id);
            }
            catch
            {
                return (false, string.Empty, 0, "Error al validar el pago.",new Guid());
            }
        }




        public async Task<(bool IsValid, string DestinationAccountHolderName, string Message)> ValidateThirdPartyTransactionAsync(string sourceAccountNumber, string destinationAccountNumber)
        {
            try
            {
                var sourceAccount = await _accountRepository.GetAccountByNumber(sourceAccountNumber);
                if (sourceAccount == null || sourceAccount.Status != AccountStatus.ACTIVE)
                {
                    return (false, string.Empty, "La cuenta origen no es válida o está inactiva.");
                }

                var destinationAccount = await _accountRepository.GetAccountByNumber(destinationAccountNumber);
                if (destinationAccount == null || destinationAccount.Status != AccountStatus.ACTIVE)
                {
                    return (false, string.Empty, "La cuenta destino no es válida o está inactiva.");
                }

                var user = await _userService.GetUserById(destinationAccount.UserId);
                if (user == null)
                {
                    return (false, string.Empty, "No se encontró el titular de la cuenta destino.");
                }

                var accountHolderName = $"{user.Name} {user.LastName}";
                return (true, accountHolderName, string.Empty);
            }
            catch
            {
                return (false, string.Empty, "Error al validar la transacción.");
            }
        }

        #endregion

        #region Procesamiento de Transacciones

        public async Task<(bool Success, string Message)> ProcessDepositAsync(DepositViewModel model, string tellerId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var account = await _accountRepository.GetAccountByNumber(model.AccountNumber);
                if (account == null || account.Status != AccountStatus.ACTIVE)
                {
                    await _unitOfWork.RollbackAsync();
                    return (false, "Cuenta inválida o inactiva.");
                }

                // Acreditar el monto a la cuenta
                await _accountRepository.CreditBalance(model.AccountNumber, model.Amount);


                var operationId = _transactionRepository.GenerateOperationId();
                // Registrar la transacción
                var transaction = new Transaction
                {
                    Amount = model.Amount,
                    DateTime = DateTime.Now,
                    Type = TransactionType.CREDIT,
                    Origin = "DEPÓSITO",
                    Beneficiary = model.AccountNumber,
                    AccountNumber = model.AccountNumber,
                    AccountId = account.Id,
                    Status = OperationStatus.APPROVED,
                    Description = DescriptionTransaction.DEPOSIT,
                    TellerId = tellerId,
                };

                await _transactionRepository.AddAsync(transaction);
                await _unitOfWork.CommitAsync();

                try
                {
                    var user = await _userService.GetUserById(account.UserId);
                    if (user != null)
                    {
                        var last4Digits = model.AccountNumber.Substring(model.AccountNumber.Length - 4);
                        await _emailService.SendAsync(new EmailRequestDto
                        {
                            To = user.Email,
                            Subject = $"Depósito realizado a su cuenta {last4Digits}",
                            BodyHtml = $"Estimado {user.Name} {user.LastName},<br/><br/>" +
                                   $"Se ha realizado un depósito a su cuenta por un monto de RD${model.Amount:N2}.<br/>" +
                                   $"Fecha y hora: {DateTime.Now:dd/MM/yyyy HH:mm}<br/><br/>" +
                                   $"Gracias por confiar en nosotros."
                        });
                    }
                }
                catch
                {
                    // Si falla el email, no afecta el depósito
                }

                return (true, "Depósito realizado exitosamente.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return (false, $"Error al procesar el depósito: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ProcessWithdrawalAsync(WithdrawalViewModel model, string tellerId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var account = await _accountRepository.GetAccountByNumber(model.AccountNumber);
                if (account == null || account.Status != AccountStatus.ACTIVE)
                {
                    await _unitOfWork.RollbackAsync();
                    return (false, "Cuenta inválida o inactiva.");
                }

                if (account.Balance < model.Amount)
                {
                   
                    await _unitOfWork.RollbackAsync();
                    return (false, "El monto excede el saldo disponible.");
                }


                // Debitar el monto de la cuenta
                await _accountRepository.DebitBalance(model.AccountNumber, model.Amount);
                var operationId = _transactionRepository.GenerateOperationId();

                // Registrar la transacción
                var transaction = new Transaction
                {
                    Amount = model.Amount,
                    DateTime = DateTime.Now,
                    Type = TransactionType.DEBIT,
                    Origin = model.AccountNumber,
                    Beneficiary = "RETIRO",
                    AccountNumber = model.AccountNumber,
                    AccountId = account.Id,
                    Status = OperationStatus.APPROVED,
                    Description = DescriptionTransaction.WITHDRAWAL,

                    TellerId = tellerId
                };

                await _transactionRepository.AddAsync(transaction);
                await _unitOfWork.CommitAsync();

                // Enviar correo al cliente
                var user = await _userService.GetUserById(account.UserId);
                if (user != null)
                {
                    var last4Digits = model.AccountNumber.Substring(model.AccountNumber.Length - 4);
                    await _emailService.SendAsync(new EmailRequestDto
                    {
                        To = user.Email,
                        Subject = $"Retiro realizado a su cuenta {last4Digits}",
                        BodyHtml = $"Estimado {user.Name} {user.LastName},<br/><br/>" +
                               $"Se ha realizado un retiro de su cuenta por un monto de RD${model.Amount:N2}.<br/>" +
                               $"Fecha y hora: {DateTime.Now:dd/MM/yyyy HH:mm}<br/><br/>" +
                               $"Gracias por confiar en nosotros."
                    });
                }

                return (true, "Retiro realizado exitosamente.");
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                return (false, "Error al procesar el retiro.");
            }
        }

        public async Task<(bool Success, string Message)> ProcessCreditCardPaymentAsync(CreditCardPaymentViewModel model, string tellerId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var account = await _accountRepository.GetAccountByNumber(model.AccountNumber);
                var creditCard = await _creditCardRepository.GetByNumberAsync(model.CardNumber);

                if (account == null)
                {
                    await _unitOfWork.RollbackAsync();
                    return (false, $"No se encontró la cuenta con número {model.AccountNumber}.");
                }

                if (account.Status != AccountStatus.ACTIVE)
                {
                    await _unitOfWork.RollbackAsync();
                    return (false, $"La cuenta {model.AccountNumber} está inactiva y no puede realizar operaciones.");
                }

                if (creditCard == null)
                {
                    await _unitOfWork.RollbackAsync();
                    return (false, $"No se encontró la tarjeta con número {model.CardNumber}.");
                }

                if (creditCard.Status != CardStatus.ACTIVE)
                {
                    await _unitOfWork.RollbackAsync();
                    return (false, $"La tarjeta está inactiva o cancelada y no puede recibir pagos.");
                }

                // Calcular el monto real a pagar (no puede exceder la deuda)
                var actualAmountToPay = Math.Min(model.Amount, creditCard.TotalAmountOwed);

                if (actualAmountToPay <= 0)
                {
                    await _unitOfWork.RollbackAsync();
                    return (false, "La tarjeta no tiene deuda pendiente.");
                }

                if (account.Balance < actualAmountToPay)
                {
                    await _unitOfWork.RollbackAsync();
                    return (false, $"Saldo insuficiente. La cuenta tiene RD${account.Balance:N2} y se requieren RD${actualAmountToPay:N2}.");
                }

                // Debitar de la cuenta
                await _accountRepository.DebitBalance(model.AccountNumber, actualAmountToPay);
           

                // Reducir la deuda de la tarjeta
               var creditWithDebit =  await _creditCardRepository.DebitTotalAmountOwedAsync(model.CardNumber, actualAmountToPay);
                var  NuevaDeuda = Math.Max(creditWithDebit!.TotalAmountOwed,0);


                var operationId = _transactionRepository.GenerateOperationId();
                // Registrar la transacción
                var transaction = new Transaction
                {
                    Amount = actualAmountToPay,
                    DateTime = DateTime.Now,
                    Type = TransactionType.DEBIT,
                    Origin = model.AccountNumber,
                    Beneficiary = model.CardNumber,
                    AccountNumber = model.AccountNumber,
                    AccountId = account.Id,
                    Status = OperationStatus.APPROVED,
                    Description = DescriptionTransaction.CREDITCARDPAYMENT,
                    TellerId = tellerId,
                };

       

                await _transactionRepository.AddAsync(transaction);
                await _unitOfWork.CommitAsync();

                // Enviar correo al cliente
                var user = await _userService.GetUserById(creditCard.ClientId);
                if (user != null)
                {
                    var last4Digits = model.CardNumber.Substring(model.CardNumber.Length - 4);
                    var accountLast4 = model.AccountNumber.Substring(model.AccountNumber.Length - 4);
                    await _emailService.SendAsync(new EmailRequestDto
                    {
                        To = user.Email,
                        Subject = $"Pago realizado a la tarjeta {last4Digits}",
                        BodyHtml = $"Estimado {user.Name} {user.LastName},<br/><br/>" +
                               $"Se ha realizado un pago a su tarjeta *{last4Digits} por un monto de RD${actualAmountToPay:N2}.<br/>" +
                               $"Cuenta origen: *{accountLast4}<br/>" +
                               $"Deuda restante: RD${(creditCard.TotalAmountOwed - actualAmountToPay):N2}<br/>" +
                               $"Fecha y hora: {DateTime.Now:dd/MM/yyyy HH:mm}<br/><br/>" +
                               $"Gracias por confiar en nosotros."
                    });
                }

                return (true, $"Pago realizado exitosamente por RD${actualAmountToPay:N2}. Nueva deuda: RD${NuevaDeuda:N2}");
            }
            catch (Exception ex)
            {
                try
                {
                    await _unitOfWork.RollbackAsync();
                }
                catch
                {
                    // Ignorar errores de rollback si la transacción ya finalizó
                }
                return (false, $"Error al procesar el pago: {ex.Message}");
            }
        }




        public async Task<(bool Success, string Message)> ProcessLoanPaymentAsync(LoanPaymentViewModel model, string tellerId)
        {
         
            try
            {
                var account = await _accountRepository.GetAccountByNumber(model.AccountNumber);
                var loan = await _loanRepository.GetByNumberAsync(model.LoanNumber);

                if (account == null)
                {
                   
                    return (false, $"No se encontró la cuenta con número {model.AccountNumber}.");
                }

                if (account.Status != AccountStatus.ACTIVE)
                {
                    return (false, $"La cuenta {model.AccountNumber} está inactiva y no puede realizar operaciones.");
                }

                if (loan == null)
                {
                    
                    return (false, $"No se encontró el préstamo con número {model.LoanNumber}.");
                }

                if (!loan.IsActive)
                {
                   
                    return (false, "El préstamo ya está completado o cancelado.");
                }

                if (account.Balance < model.Amount)
                {
                   
                    return (false, $"Saldo insuficiente. La cuenta tiene RD${account.Balance:N2} y se requieren RD${model.Amount:N2}.");
                }



                var transact = await loanService.PayLoanAsync(model.LoanId, model.Amount);

                // Registrar la transacción
                var transaction = new Transaction
                {
                    Amount = loan.Amount,
                    DateTime = DateTime.Now,
                    Type = TransactionType.DEBIT,
                    Origin = model.AccountNumber,
                    Beneficiary = model.LoanNumber,
                    AccountNumber = model.AccountNumber,
                    AccountId = account.Id,
                    Status = OperationStatus.APPROVED,
                    Description = DescriptionTransaction.LOANPAYMENT,
                    TellerId = tellerId
                };

         
                await _transactionRepository.AddAsync(transaction);
            
                // Enviar correo al cliente
                var user = await _userService.GetUserById(loan.ClientId);
                if (user != null)
                {
                    var accountLast4 = model.AccountNumber.Substring(model.AccountNumber.Length - 4);
                    await _emailService.SendAsync(new EmailRequestDto
                    {
                        To = user.Email,
                        Subject = $"Pago realizado al préstamo {model.LoanNumber}",
                        BodyHtml = $"Estimado {user.Name} {user.LastName},<br/><br/>" +
                               $"Se ha realizado un pago a su préstamo {model.LoanNumber} por un monto de RD${model.Amount:N2}.<br/>" +
                               $"Cuenta origen: *{accountLast4}<br/>" +
                               $"Fecha y hora: {DateTime.Now:dd/MM/yyyy HH:mm}<br/><br/>" +
                               $"Gracias por confiar en nosotros."
                    });
                }

                var message = "Pago realizado exitosamente.";


                return (true, message);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                return (false, "Error al procesar el pago.");
            }
        }



        public async Task<(bool Success, string Message)> ProcessThirdPartyTransactionAsync(ThirdPartyTransactionViewModel model, string tellerId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var sourceAccount = await _accountRepository.GetAccountByNumber(model.SourceAccountNumber);
                var destinationAccount = await _accountRepository.GetAccountByNumber(model.DestinationAccountNumber);

                if (sourceAccount == null || sourceAccount.Status != AccountStatus.ACTIVE)
                {
                    await _unitOfWork.RollbackAsync();
                    return (false, "Cuenta origen inválida o inactiva.");
                }

                if (destinationAccount == null || destinationAccount.Status != AccountStatus.ACTIVE)
                {
                    await _unitOfWork.RollbackAsync();

                    return (false, "Cuenta destino inválida o inactiva.");
                }

                if (sourceAccount.Balance < model.Amount)
                {

                    var operationIdDeclined = _transactionRepository.GenerateOperationId();
                    // Registrar transacción DÉBITO en cuenta origen
                    var debitTransactionDeclined = new Transaction
                    {
                        Amount = model.Amount,
                        DateTime = DateTime.Now,
                        Type = TransactionType.DEBIT,
                        Origin = model.SourceAccountNumber,
                        Beneficiary = model.DestinationAccountNumber,
                        AccountNumber = model.SourceAccountNumber,
                        AccountId = sourceAccount.Id,
                        Status = OperationStatus.DECLINED,
                        Description = DescriptionTransaction.TRANSFER,
                        TellerId = tellerId,
                    };

                    await _transactionRepository.AddAsync(debitTransactionDeclined);

                    await _unitOfWork.RollbackAsync();
                    return (false, "El monto excede el saldo disponible.");
                }

                // Debitar de la cuenta origen (modificación directa, ya está rastreada por EF)
                sourceAccount.Balance -= model.Amount;

                // Acreditar a la cuenta destino (modificación directa, ya está rastreada por EF)
                destinationAccount.Balance += model.Amount;


                var operationId = _transactionRepository.GenerateOperationId();
                // Registrar transacción DÉBITO en cuenta origen
                var debitTransaction = new Transaction
                {
                    Amount = model.Amount,
                    DateTime = DateTime.Now,
                    Type = TransactionType.DEBIT,
                    Origin = model.SourceAccountNumber,
                    Beneficiary = model.DestinationAccountNumber,
                    AccountNumber = model.SourceAccountNumber,
                    AccountId = sourceAccount.Id,
                    Status = OperationStatus.APPROVED,
                    Description = DescriptionTransaction.TRANSFER,
                    TellerId = tellerId,
                };

                await _transactionRepository.AddAsync(debitTransaction);

                // Registrar transacción CRÉDITO en cuenta destino
                var creditTransaction = new Transaction
                {
                    Amount = model.Amount,
                    DateTime = DateTime.Now,
                    Type = TransactionType.CREDIT,
                    Origin = model.SourceAccountNumber,
                    Beneficiary = model.DestinationAccountNumber,
                    AccountNumber = model.DestinationAccountNumber,
                    AccountId = destinationAccount.Id,
                    Status = OperationStatus.APPROVED,
                    Description = DescriptionTransaction.TRANSFER,
                    TellerId = tellerId,

                };

            await _transactionRepository.AddAsync(creditTransaction);
            await _unitOfWork.CommitAsync();

            // Enviar correos
            var sourceUser = await _userService.GetUserById(sourceAccount.UserId);
            var destinationUser = await _userService.GetUserById(destinationAccount.UserId);                if (sourceUser != null)
                {
                    var destLast4 = model.DestinationAccountNumber.Substring(model.DestinationAccountNumber.Length - 4);
                    await _emailService.SendAsync(new EmailRequestDto
                    {
                        To = sourceUser.Email,
                        Subject = $"Transacción realizada a la cuenta {destLast4}",
                        BodyHtml = $"Estimado {sourceUser.Name} {sourceUser.LastName},<br/><br/>" +
                               $"Se ha realizado una transferencia por un monto de RD${model.Amount:N2}.<br/>" +
                               $"Fecha y hora: {DateTime.Now:dd/MM/yyyy HH:mm}<br/><br/>" +
                               $"Gracias por confiar en nosotros."
                    });
                }

                if (destinationUser != null)
                {
                    var sourceLast4 = model.SourceAccountNumber.Substring(model.SourceAccountNumber.Length - 4);
                    await _emailService.SendAsync(new EmailRequestDto
                    {
                        To = destinationUser.Email,
                        Subject = $"Transacción enviada desde la cuenta {sourceLast4}",
                        BodyHtml = $"Estimado {destinationUser.Name} {destinationUser.LastName},<br/><br/>" +
                               $"Ha recibido una transferencia por un monto de RD${model.Amount:N2}.<br/>" +
                               $"Fecha y hora: {DateTime.Now:dd/MM/yyyy HH:mm}<br/><br/>" +
                               $"Gracias por confiar en nosotros."
                    });
                }

                return (true, "Transacción realizada exitosamente.");
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                return (false, "Error al procesar la transacción.");
            }
        }

        #endregion
    }
}
