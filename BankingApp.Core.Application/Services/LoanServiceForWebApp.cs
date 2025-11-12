using AutoMapper;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Services
{
    public class LoanServiceForWebApp : BaseLoanService, ILoanServiceForWebApp
    {
        public LoanServiceForWebApp(ILoanRepository repo, IMapper mapper, ILogger<Loan> logger) : base(repo, mapper, logger)
        {
           
        }
    }
}
