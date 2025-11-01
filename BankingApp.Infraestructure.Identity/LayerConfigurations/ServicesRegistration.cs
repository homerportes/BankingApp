using BankingApp.Core.Application.Interfaces;
using BankingApp.Infraestructure.Identity.Contexts;
using BankingApp.Infraestructure.Identity.Entities;
using BankingApp.Infraestructure.Identity.Seeds;
using BankingApp.Infraestructure.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankingApp.Infraestructure.Identity.LayerConfigurations
{
    public static class ServicesRegistration
    {

        public static void AddIdentityLayerIocForWebApp(this IServiceCollection services, IConfiguration config)
        {

            GeneralConfiguration(services, config);
            #region Identity

            services.Configure<IdentityOptions>(opt =>
            {
                opt.Password.RequiredLength = 8;
                opt.Password.RequireDigit = true;
                opt.Password.RequireNonAlphanumeric = true;

                opt.Password.RequireLowercase = true;
                opt.Password.RequireUppercase = true;
                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
                opt.Lockout.MaxFailedAccessAttempts = 5;
                opt.User.RequireUniqueEmail = true;
                opt.SignIn.RequireConfirmedEmail = true;


            });
            services.AddIdentityCore<AppUser>()
                        .AddRoles<IdentityRole>()
                        .AddSignInManager()
                        .AddEntityFrameworkStores<IdentityContext>()
                        .AddTokenProvider<DataProtectorTokenProvider<AppUser>>(TokenOptions.DefaultProvider);



            services.Configure<DataProtectionTokenProviderOptions>(opt =>
            {
                opt.TokenLifespan = TimeSpan.FromHours(12);

            });

            services.AddAuthentication(opt =>
            {
                opt.DefaultScheme = IdentityConstants.ApplicationScheme;
                opt.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                opt.DefaultScheme = IdentityConstants.ApplicationScheme;


            }).AddCookie(IdentityConstants.ApplicationScheme, opt =>
            {
                opt.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                opt.LoginPath = "/Login";
                opt.AccessDeniedPath = "/Login/AccessDenied";
                opt.SlidingExpiration = true;

            });







            #endregion


            #region Services
            services.AddScoped<IAccountServiceForWebAPP, AccountServiceForWebAPP>();
            #endregion

        }

        private static void GeneralConfiguration(IServiceCollection services, IConfiguration config)
        {

            if (config.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<IdentityContext>(opt =>
                                                        opt.UseInMemoryDatabase("AppDb"));
            }
            else
            {
                var connectionString = config.GetConnectionString("IdentityConnection");
                services.AddDbContext<IdentityContext>(

                    (serviceProvider, opt) =>
                    {
                        opt.EnableSensitiveDataLogging();
                        opt.UseSqlServer(connectionString,
                            m => m.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName));
                    },
                    contextLifetime: ServiceLifetime.Scoped,
                    optionsLifetime: ServiceLifetime.Scoped


                    );
            }
        }

        public static async Task RunIdentitySeedAsync(this IServiceProvider service)
        {
            using var scope = service.CreateScope();
            var servicesProvider = scope.ServiceProvider;

            var userManager = servicesProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = servicesProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await DefaultRoles.SeedAsync(roleManager);

            await DefaultAdminUser.SeedAsync(userManager);

            await DefaultUser.SeedAsync(userManager);
        }
    }
}
