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
        private readonly IMapper _mapper;

        public CommerceService(ICommerceRepository repo, IMapper mapper) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
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

        public async Task<CommercePaginationDto> GetAllFiltered(int? page, int ?pageSize)
        {
            var query = _repo.GetAllQuery()
                 .OrderByDescending(r => r.CreatedAt)

                .Where(r => r.IsActive);


            var totalCount = await query.CountAsync();

            if (page.HasValue && pageSize.HasValue && page > 0 && pageSize > 0)
            {
                int skip = (page.Value - 1) * pageSize.Value;
                query = query.Skip(skip).Take(pageSize.Value);
            }

            var data = await query.ToListAsync();

            var result = new CommercePaginationDto
            {
                Data = _mapper.Map<List<CommerceDto>>(data),
                TotalCount = totalCount,
                CurrentPage = page??1,
                PagesCount = pageSize.HasValue && pageSize > 0
                    ? (int)Math.Ceiling((double)totalCount / pageSize.Value)
                    : 1
            };

            return result;


        }
    }
}
