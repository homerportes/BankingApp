using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Interfaces
{
    public interface IGenericService< Entity, EntityDto>  where Entity : class where EntityDto : class
    {

        public Task<EntityDto?> AddAsync(EntityDto entityDto);
        public Task<EntityDto?> GetByIdAsync(int id);
        public Task DeleteAsync(int id);
        public Task<List<EntityDto>?> GetAllList();
        public Task<List<EntityDto>?> GetAllListWithInclude(List<string> properties);
        public Task<EntityDto?> UpdateAsync(int id, EntityDto entityDto);
        public Task UpdateRangeAsync(List<EntityDto> entityDtos);
        public Task DeleteRangeAsync(List<EntityDto> entityDtos);
    }
}
