

namespace BankingApp.Core.Application.Interfaces
{
    public interface IBaseService<Tentity,TentityDto> where TentityDto : class where Tentity : class
    {


        public Task<TentityDto?> AddAsync(TentityDto entityDto);
        public Task<TentityDto?> GetByIdAsync(int id);
        public Task DeleteAsync(int id);

    }
}
