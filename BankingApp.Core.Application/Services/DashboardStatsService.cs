using BankingApp.Core.Application.Dtos.Stats;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net.WebSockets;


namespace BankingApp.Core.Application.Services
{
    public class DashboardStatsService :IDashboardsStatsService
    {
        private readonly IUserService _userService;
        private readonly ILoanRepository _loanRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly ITransacctionRepository _transacctionRepository;

        public DashboardStatsService(
            IUserService userService,
            ILoanRepository loanRepository,
            IAccountRepository accountRepository
            ,

            ICreditCardRepository creditCardRepository,
            ITransacctionRepository transacctionRepository
            )
        {
            _userService = userService;
            _loanRepository = loanRepository;
            _accountRepository = accountRepository;
            _creditCardRepository = creditCardRepository;
            _transacctionRepository = transacctionRepository;
        }


        public async Task<AdminDashboardStatsDto> GetAdminStats()
        {

            var totalAccounts = await _accountRepository.CountSavingAccountsByUserIds(await _userService.GetAllClientIds());
            var totalActiveClients = await _userService.GetActiveClientsCount();

            var Payments = _transacctionRepository.GetAllQuery().Where(r => r.Description == DescriptionTransaction.LOANPAYMENT || r.Description == DescriptionTransaction.CREDITCARDPAYMENT);
            return new AdminDashboardStatsDto
            {

                TotalTransactionsCount = await _transacctionRepository.GetAllQuery().CountAsync(),
                TodayTransactionsCount = await _transacctionRepository.GetAllQuery().Where(r=>r.DateTime== DateTime.Now.Date).CountAsync(),

                DayPaysCount = await Payments.Where(p => p.DateTime.Date == DateTime.Now.Date).CountAsync(),

                TotalPaysCount = await Payments.CountAsync(),
                TotalActiveClientsCount = await _userService.GetActiveClientsCount(),
                TotalInactiveClientsCount = totalActiveClients,
                TotalAsignedProductsCount = totalAccounts + await _loanRepository.GetAllLoansCount() + await _creditCardRepository.GetTotalCreditCardsWithClient(),
                TotalCurrentLoansCount = await _loanRepository.GetActiveLoansCount(),
                TotalActiveCreditCardsCount = await _creditCardRepository.GetTotalActiveCreditCards(),

                TotalIssuedCreditCardsCount = await _creditCardRepository.GetTotalIssuedCreditCards(),
                TotalClientCreditCardsCount = await _creditCardRepository.GetTotalActiveCreditCardsWithClient(),

                TotalSavingAccountsCount = totalAccounts,

                AverageClientsDebt = await GetActiveClientsDebt() / totalActiveClients,

                AverageClientsDebtActiveAndInactive = await GetAverageSystemDebt()

            };




           
        }
        public async Task<decimal> GetAverageSystemDebt()
        {
            var clientIds = await _userService.GetAllClientIds();

            if (clientIds == null || clientIds.Count == 0)
                return 0;

            var totalDebt = await _loanRepository.GetAllQuery()
                .Where(r => clientIds.Contains(r.ClientId))
                .SumAsync(r => r.OutstandingBalance);

            return totalDebt / clientIds.Count;
        }



        private async Task<decimal> GetActiveClientsDebt()
    {
        var activeUserIds = await _userService.GetActiveClientsIds();
        if (activeUserIds == null || !activeUserIds.Any())
            return 0;

        var loanDebts= await _loanRepository.GetActiveClientsLoanDebt(activeUserIds);
        var creditCardDebts = await _creditCardRepository.GetActiveClientsCreditCardDebt(activeUserIds);
        return loanDebts + creditCardDebts;
    }
    }
}
