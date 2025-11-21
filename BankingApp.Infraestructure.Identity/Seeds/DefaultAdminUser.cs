using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;


namespace BankingApp.Infraestructure.Identity.Seeds
{
    public static class DefaultAdminUser
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager)
        {
            await userManager.CreateAsync(new AppUser
            {
                DocumentIdNumber = "10234567356",
                LastName = "ADMIN",
                Name = "ADMIN",
                UserName = "superAdmin",
                Email = "no.repply.bankingappp@gmail.com",
                EmailConfirmed = true,
                IsActive = true

            });

            var user = await  userManager.FindByNameAsync("superAdmin");
            await userManager.AddPasswordAsync(user!, "Pa$Word1");

            await userManager.AddToRoleAsync(user!, AppRoles.ADMIN.ToString());



        }
    }
}
