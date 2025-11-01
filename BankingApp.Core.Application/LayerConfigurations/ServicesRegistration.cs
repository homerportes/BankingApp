using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.Services;
using Microsoft.Extensions.DependencyInjection;


namespace BankingApp.Core.Application.LayerConfigurations
{
    public static class ServicesRegistration
    {
        public static void AddApplicationLayer(this IServiceCollection services  )
        {
           services.AddScoped(typeof(IGenericService<,>), typeof(GenericService<,>));

        }

    }
}
