using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;


namespace BankingApp.Infraestructure.Identity.Seeds
{
    public static class DefaultClientUser
    {


        public static async Task SeedAsync(UserManager<AppUser> userManager)
        {

            await userManager.CreateAsync(new AppUser
            {
                DocumentIdNumber = "10232567856",
                LastName = "CLIENT",
                Name = "BeeThree",
                UserName = "Client01",
                Email = "Client.bankingapp@gmail.com",
                EmailConfirmed = true,
                IsActive = true

            });

            var _user = await userManager.FindByNameAsync("Client01");
            await userManager.AddPasswordAsync(_user!, "Cli$Pass1");
            await userManager.AddToRoleAsync(_user!, AppRoles.CLIENT.ToString());





        }





    }
}
