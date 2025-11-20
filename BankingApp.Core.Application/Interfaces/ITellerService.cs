using BankingApp.Core.Application.ViewModels.Teller;

namespace BankingApp.Core.Application.Interfaces
{
    /// <summary>
    /// Interfaz para los servicios del cajero (Teller)
    /// </summary>
    public interface ITellerService
    {
        /// <summary>
        /// Obtiene los indicadores del dashboard del cajero para el día actual
        /// </summary>
        /// <param name="tellerId">ID del cajero autenticado</param>
        /// <returns>ViewModel con los indicadores del día</returns>
        Task<TellerDashboardViewModel> GetTellerDashboardDataAsync(string tellerId);

        /// <summary>
        /// Procesa un depósito a una cuenta de ahorro
        /// </summary>
        /// <param name="model">Datos del depósito</param>
        /// <param name="tellerId">ID del cajero que realiza la operación</param>
        /// <returns>True si el depósito fue exitoso</returns>
        Task<(bool Success, string Message)> ProcessDepositAsync(DepositViewModel model, string tellerId);

        /// <summary>
        /// Procesa un retiro de una cuenta de ahorro
        /// </summary>
        /// <param name="model">Datos del retiro</param>
        /// <param name="tellerId">ID del cajero que realiza la operación</param>
        /// <returns>True si el retiro fue exitoso</returns>
        Task<(bool Success, string Message)> ProcessWithdrawalAsync(WithdrawalViewModel model, string tellerId);

        /// <summary>
        /// Procesa un pago a una tarjeta de crédito
        /// </summary>
        /// <param name="model">Datos del pago</param>
        /// <param name="tellerId">ID del cajero que realiza la operación</param>
        /// <returns>True si el pago fue exitoso</returns>
        Task<(bool Success, string Message)> ProcessCreditCardPaymentAsync(CreditCardPaymentViewModel model, string tellerId);

        /// <summary>
        /// Procesa un pago a un préstamo
        /// </summary>
        /// <param name="model">Datos del pago</param>
        /// <param name="tellerId">ID del cajero que realiza la operación</param>
        /// <returns>True si el pago fue exitoso</returns>
        Task<(bool Success, string Message)> ProcessLoanPaymentAsync(LoanPaymentViewModel model, string tellerId);

        /// <summary>
        /// Procesa una transacción entre cuentas de terceros
        /// </summary>
        /// <param name="model">Datos de la transacción</param>
        /// <param name="tellerId">ID del cajero que realiza la operación</param>
        /// <returns>True si la transacción fue exitosa</returns>
        Task<(bool Success, string Message)> ProcessThirdPartyTransactionAsync(ThirdPartyTransactionViewModel model, string tellerId);

        /// <summary>
        /// Valida y obtiene información de la cuenta para confirmación de depósito
        /// </summary>
        Task<(bool IsValid, string AccountHolderName, string Message)> ValidateAccountForDepositAsync(string accountNumber);

        /// <summary>
        /// Valida y obtiene información de la cuenta para confirmación de retiro
        /// </summary>
        Task<(bool IsValid, string AccountHolderName, decimal Balance, string Message)> ValidateAccountForWithdrawalAsync(string accountNumber, decimal amount, string tellerId);

        /// <summary>
        /// Valida y obtiene información para confirmación de pago a tarjeta de crédito
        /// </summary>
        Task<(bool IsValid, string CardHolderName, decimal CurrentDebt, string Message)> ValidateCreditCardPaymentAsync(string accountNumber, string cardNumber, decimal amount, string tellerId);

        /// <summary>
        /// Valida y obtiene información para confirmación de pago a préstamo
        /// </summary>
        Task<(bool IsValid, string LoanHolderName, decimal RemainingBalance, string Message, Guid IdLoan)> ValidateLoanPaymentAsync(string accountNumber, string loanNumber, decimal amount, string tellerId);

        /// <summary>
        /// Valida cuentas para transacción entre terceros
        /// </summary>
        Task<(bool IsValid, string DestinationAccountHolderName, string Message)> ValidateThirdPartyTransactionAsync(string sourceAccountNumber, string destinationAccountNumber, decimal amount);
    }
}
