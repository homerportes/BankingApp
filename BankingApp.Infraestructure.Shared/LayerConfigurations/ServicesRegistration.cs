using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Settings;
using BankingApp.Infraestructure.Shared.Services.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Infraestructure.Shared.LayerConfigurations
{
    public static class ServicesRegistration
    {
        public static IServiceCollection AddSharedLayer(this IServiceCollection services , IConfiguration config )
        {
            services.AddScoped<IEmailService, EmailService>();
            services.Configure<MailSettings>(config.GetSection("MailSettings"));
            return services;
        }

    }
}
