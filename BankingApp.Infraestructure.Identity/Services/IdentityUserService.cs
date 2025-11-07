using BankingApp.Core.Application.Dtos.User;
using BankingApp.Core.Application.Helpers;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Infraestructure.Identity.Contexts;
using BankingApp.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data;


namespace BankingApp.Infraestructure.Identity.Services
{
    public class IdentityUserService:  IUserService
    {

        private UserManager<AppUser> _userManager;
        private readonly IdentityContext _identityContext;

        public IdentityUserService(UserManager<AppUser> userManager, IdentityContext identityDbContext)
        {
            _userManager = userManager;
            _identityContext = identityDbContext;
        }
        public async Task<ApiUserPaginationResultDto> GetAllOnlyCommerce(int page = 1, int pageSize = 20, string? rol = null)
        {
            var commerceRoleId = await _identityContext.Roles
                .Where(r => r.Name == AppRoles.COMMERCE.ToString())
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            string? extraRoleId = null;
            if (!string.IsNullOrWhiteSpace(rol))
            {
                var allRoles = await _identityContext.Roles.ToListAsync();
                var matchedRole = allRoles.FirstOrDefault(r =>
                    r.Name.Equals(rol, StringComparison.OrdinalIgnoreCase) ||
                    EnumTranslator.Translate(r.Name).Equals(rol, StringComparison.OrdinalIgnoreCase));

                extraRoleId = matchedRole?.Id;
            }

            var commerceUsersQuery = from ur in _identityContext.UserRoles
                                     where ur.RoleId == commerceRoleId
                                     join u in _identityContext.Users on ur.UserId equals u.Id
                                     select u;

            if (extraRoleId!=null)
            {
                var extraUsers = await _identityContext.UserRoles
                    .Where(ur => ur.RoleId == extraRoleId)
                    .Select(ur => ur.UserId)
                    .ToListAsync();

                commerceUsersQuery = commerceUsersQuery.Where(u => extraUsers.Contains(u.Id));
            }

            var totalCount = await commerceUsersQuery.CountAsync();

            var pagedUsers = await commerceUsersQuery
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userIds = pagedUsers.Select(u => u.Id).ToList();

            var otherRoles = await _identityContext.UserRoles
                .Where(ur => userIds.Contains(ur.UserId) && ur.RoleId != commerceRoleId)
                .Join(_identityContext.Roles,
                      ur => ur.RoleId,
                      r => r.Id,
                      (ur, r) => new { ur.UserId, RoleName = r.Name })
                .GroupBy(x => x.UserId)
                .Select(g => new { UserId = g.Key, RoleName = g.Select(x => x.RoleName).FirstOrDefault() })
                .ToListAsync();

            var roleMap = otherRoles.ToDictionary(x => x.UserId, x => x.RoleName);

            var result = pagedUsers.Select(u => new UserDtoForApi
            {
                Email = u.Email ?? "",
                Name = u.Name,
                LastName = u.LastName,
                Role = EnumTranslator.Translate(roleMap.GetValueOrDefault(u.Id) ?? "") ?? AppRoles.COMMERCE.ToString(),
                DocumentIdNumber = u.DocumentIdNumber,
                UserName = u.UserName ?? "",
                Status = u.EmailConfirmed ? "activo" : "inactivo"
            }).ToList();

            return new ApiUserPaginationResultDto
            {
                Data = result,
                PagesCount = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = page,
                TotalCount = totalCount
            };
        }

        public async Task<ApiUserPaginationResultDto> GetAllExceptCommerce(int page = 1, int pageSize = 20, string? rol = null)
        {
            var commerceRoleId = await _identityContext.Roles
                .Where(r => r.Name == AppRoles.COMMERCE.ToString())
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            string? extraRoleId = null;
            if (!string.IsNullOrWhiteSpace(rol))
            {
                var allRoles = await _identityContext.Roles.ToListAsync();
                var matchedRole = allRoles.FirstOrDefault(r =>
                    r.Name.Equals(rol, StringComparison.OrdinalIgnoreCase) ||
                    EnumTranslator.Translate(r.Name).Equals(rol, StringComparison.OrdinalIgnoreCase));

                extraRoleId = matchedRole?.Id;
            }

            var query = from ur in _identityContext.UserRoles
                        join u in _identityContext.Users on ur.UserId equals u.Id
                        join r in _identityContext.Roles on ur.RoleId equals r.Id
                        where ur.RoleId != commerceRoleId
                        select new { User = u, Role = r };

            if (!string.IsNullOrEmpty(extraRoleId))
            {
                query = query.Where(x => x.Role.Id == extraRoleId);
            }

            var totalCount = await query.CountAsync();

            var pagedUsers = await query
                .OrderByDescending(x => x.User.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(); 

            var result = pagedUsers.Select(x => new UserDtoForApi
            {
                Email = x.User.Email ?? "",
                Name = x.User.Name,
                LastName = x.User.LastName,
                Role = EnumTranslator.Translate(x.Role.Name ?? ""),
                DocumentIdNumber = x.User.DocumentIdNumber,
                UserName = x.User.UserName ?? "",
                Status = x.User.EmailConfirmed ? "activo" : "inactivo"
            }).ToList();

            return new ApiUserPaginationResultDto
            {
                Data = result,
                PagesCount = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = page,
                TotalCount = totalCount
            };
        }

    }
}
