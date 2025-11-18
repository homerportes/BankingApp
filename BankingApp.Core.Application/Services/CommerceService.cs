using AutoMapper;
using BankingApp.Core.Application.Dtos.Commerce;
using BankingApp.Core.Application.Dtos.Operations;
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
        private readonly ICommerceUserRepository _commerceUserRepository;

        public CommerceService(ICommerceRepository repo, IMapper mapper, ICommerceUserRepository commerceUserRepository, IUnitOfWork unitOfWork) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
            _commerceUserRepository = commerceUserRepository;

        }

        public async Task<bool>  CommerceAlreadyHasUser(int CommerceId)
        {
           return await _commerceUserRepository.GetAllQuery().AnyAsync(r => r.CommerceId == CommerceId);
        }
        public async Task <OperationResultDto>SetUser (int CommerceId, string UserId)
        {
            var response = new OperationResultDto() { IsSuccessful = true };

            var existing = await _commerceUserRepository.GetAllQuery().Where(r => r.UserId == UserId && r.CommerceId == CommerceId).ToListAsync();

            if (existing != null && existing.Count()>=1)
            {
                response.IsSuccessful = false;

                return response;

   

            }
            try
            {
                await _commerceUserRepository.AddAsync(new CommerceUser { CommerceId = CommerceId, UserId = UserId, Id = 0 });
                return response;
            }
            catch
            {
                response.IsInternalError=true;
                return response;

            }
        }



        public async Task<CommercePaginationDto> GetAllActiveFiltered(int? page, int? pageSize)
        {
            var query = _repo.GetAllQuery()
                             .Where(r => r.IsActive)
                             .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();

            if (!page.HasValue && !pageSize.HasValue)
            {
                var allData = await query.ToListAsync();

                return new CommercePaginationDto
                {
                    Data = _mapper.Map<List<CommerceDto>>(allData),
                    TotalCount = totalCount,
                    CurrentPage = 1,
                    PagesCount = 1
                };
            }

            if ((page.HasValue && page.Value == 0) || (pageSize.HasValue && pageSize.Value == 0))
            {
                return new CommercePaginationDto
                {
                    Data = new List<CommerceDto>(),
                    TotalCount = totalCount,
                    CurrentPage = 0,
                    PagesCount = 0
                };
            }

            int currentPage = page ?? 1;
            int currentPageSize = pageSize ?? 20;

            int skip = (currentPage - 1) * currentPageSize;

            var paginatedData = await query
                .Skip(skip)
                .Take(currentPageSize)
                .ToListAsync();

            return new CommercePaginationDto
            {
                Data = _mapper.Map<List<CommerceDto>>(paginatedData),
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PagesCount = (int)Math.Ceiling((double)totalCount / currentPageSize)
            };
        }



        public async Task<List<string>> GetCommerceAssociates(int commerceId)
        {
           return await _commerceUserRepository.GetAllQuery().Where(r => r.CommerceId == commerceId).Select(r => r.UserId).ToListAsync();
        }
    }
}
