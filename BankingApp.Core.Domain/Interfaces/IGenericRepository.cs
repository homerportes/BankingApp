

namespace BankingApp.Core.Domain.Interfaces
{
    public interface IGenericRepository <Entity> : IBaseRepository<Entity> where Entity : class
    {
      
      
        public Task<List<Entity>?> GetAllListWithInclude(List<string> properties);
        public IQueryable<Entity> GetAllQueryWithInclude(List<string> properties);
        public IQueryable<Entity> GetAllQuery();
        public Task<Entity?> UpdateAsync(int id, Entity entity);
        public Task AddRangeAsync(List<Entity> entities);
        public Task DeleteRangeAsync(List<Entity> entities);
        public Task UpdateRangeAsync(List<Entity> entities);


    }
}
