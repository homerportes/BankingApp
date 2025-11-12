using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Infraestructure.Identity.Seeds
{
    public static class DefaultClientUser
    {


        public static async Task SeedAsync(UserManager<AppUser> userManager)
        {

            //agregando un usuario de tipo cliente
            await userManager.CreateAsync(new AppUser
            {
                DocumentIdNumber = "",
                LastName = "",
                Name = "BeeThree",
                UserName = "Client02",
                Email = "Client.bankingapp@gmail.com",
                EmailConfirmed = true,
                IsActive = true

            });

            var _user = await userManager.FindByNameAsync("Client02");
            await userManager.AddPasswordAsync(_user!, "Cli$Pas1");
            await userManager.AddToRoleAsync(_user!, AppRoles.CLIENT.ToString());

        }





    }
}
