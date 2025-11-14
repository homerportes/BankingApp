using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;


namespace BankingApp.Core.Application.LayerConfigurations
{
    public static class ServicesRegistration
    {
        public static void AddApplicationLayer(this IServiceCollection services)
        {
           services.AddScoped(typeof(IGenericService<,>), typeof(GenericService<,>));
            services.AddScoped<IBankAccountService,BankAccountService>();
            services.AddScoped<ICommerceService,CommerceService> ();
            services.AddScoped<IBeneficiaryService, BeneficiaryService>();

            services.AddScoped<ILoanService, LoanService>();
            services.AddScoped(typeof(IGenericService<,>), typeof(GenericService<,>));
            services.AddScoped<IBankAccountService, BankAccountService>();
            services.AddScoped<ICommerceService, CommerceService>();
            services.AddScoped<ICreditCardService, CreditCardService>();
            services.AddScoped<ISavingsAccountServiceForWebApp, SavingsAccountServiceForWebApp>();
            services.AddScoped<ITransactionService, TransactionExpressService>();
            services.AddScoped<ITransactionToCreditCardService, TransactionToCreditCardService>();
            services.AddScoped<ITransactionToLoanService, TransactionToLoanService>();
            services.AddScoped<ITransactionToBeneficiaryService, TransactionExpressService>();


            services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());
            EnumMappings.Initialize();
        }

    }
}

