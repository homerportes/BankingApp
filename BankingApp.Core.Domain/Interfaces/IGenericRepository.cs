

namespace BankingApp.Core.Domain.Interfaces
{
    public interface IGenericRepository <Entity>  where Entity : class
    {




        Task<Entity> AddAsync(Entity entity);
        public Task<Entity?> GetByIdAsync(int id);
        Task DeleteAsync(int id);
        Task<List<Entity>?> GetAllList();
        public Task<List<Entity>?> GetAllListWithInclude(List<string> properties);
        public IQueryable<Entity> GetAllQueryWithInclude(List<string> properties);
        public IQueryable<Entity> GetAllQuery();
        public Task<Entity?> UpdateAsync(int id, Entity entity);
        public Task AddRangeAsync(List<Entity> entities);
        public Task DeleteRangeAsync(List<Entity> entities);
        public Task UpdateRangeAsync(List<Entity> entities);


    }
}
