using AutoMapper;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Services
{
    public class BaseService<TEntity, TEntityDto>  : IBaseService<TEntity,TEntityDto> where TEntity : class where TEntityDto : class 
    {

        private readonly IMapper _mapper;
        private readonly IBaseRepository<TEntity> repo;


        public BaseService(IMapper mapper, IBaseRepository<TEntity> repo)
        {

            this._mapper = mapper;
            this.repo = repo;
            
        }


        public async Task<TEntityDto?> AddAsync(TEntityDto entityDto)
        {
            try
            {

                var entity = _mapper.Map<TEntity>(entityDto);
                await repo.AddAsync(entity);
                var dto = _mapper.Map<TEntity, TEntityDto>(entity);
                return dto;
            }
            catch
            {
                return default;
            }
        }


        public async Task DeleteAsync(int id)
        {
            try
            {

                await repo.DeleteAsync(id);
            }
            catch
            {
            }
        }



        public async Task<TEntityDto?> GetByIdAsync(int id)
        {
            var entity = await repo.GetByIdAsync(id);

            var dto = _mapper.Map<TEntityDto>(entity);
            return dto;
        }
    }
}
