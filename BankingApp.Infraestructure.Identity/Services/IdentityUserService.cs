using BankingApp.Core.Application.Dtos.User;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;


namespace BankingApp.Infraestructure.Identity.Services
{
    public class IdentityUserService:  IUserService
    {

        private UserManager<AppUser> _userManager;

        public IdentityUserService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<UserPaginationResultDto>GetAllExceptCommerce  (int page=1, int pageSize=20, string ?rol = null)
        {
            var query = _userManager.Users.Where(r => r.Role != AppRoles.COMMERCE)
                 .Skip(pageSize * (page - 1))
                 .Take(pageSize);

            if (rol != null) {
                query = query.Where(r => r.Role.ToString().ToLower() == rol.ToLower());
            }
            var dtos = await 
            query.Select(r => new UserDto
            {
                Id = r.Id,
                Email = r.Email??"",
                Name = r.Name,
                LastName = r.LastName,
                PhoneNumber = r.PhoneNumber?? "",
                Role = r.Role.ToString(),
                UserName = r.UserName?? "",
                IsVerified = r.EmailConfirmed
            }).ToListAsync();

            var userCount =  dtos.Count();
            return new UserPaginationResultDto
            {
                UserList = dtos,
                PagesCount = (int) ( userCount / pageSize),
                CurrentPage=page,
                TotalCount=userCount
               

            };
        }
    }
}
