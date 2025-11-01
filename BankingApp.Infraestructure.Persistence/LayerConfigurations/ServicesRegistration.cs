using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace BankingApp.Infraestructure.Persistence.LayerConfigurations
{
    public static class ServicesRegistration
    {
        public static void AddPersistenceLayer(this IServiceCollection services, IConfiguration config  )
        {
            services.AddScoped(typeof(IGenericRepository<>),typeof(GenericRepository<> ));
        }

    }
}
