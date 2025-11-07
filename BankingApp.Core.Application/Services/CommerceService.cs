using AutoMapper;
using BankingApp.Core.Application.Dtos.Commerce;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;

namespace BankingApp.Core.Application.Services
{
    public class CommerceService : GenericService<Commerce, CommerceDto>, ICommerceService
    {
        private readonly ICommerceRepository _repo;
        public CommerceService(ICommerceRepository repo, IMapper mapper) : base(repo, mapper)
        {
            _repo = repo;
        }


        public async Task SetUser (int CommerceId, string UserId)
        {
            var entity=  await _repo.GetByIdAsync(CommerceId);

            if (entity != null)
            {
                entity.UserId= UserId;
                await _repo.UpdateAsync(entity.Id,entity);
            }
            
        }

    }
}
