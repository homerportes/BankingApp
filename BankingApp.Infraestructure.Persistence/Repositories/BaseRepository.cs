

using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Infraestructure.Persistence.Repositories
{
    public class BaseRepository<Entity> : IBaseRepository<Entity> where Entity : class
    {


        private readonly BankingContext _context;

        public BaseRepository(BankingContext context)
        {
            _context = context;
        }




        public  virtual async Task<Entity> AddAsync(Entity entity)
        {

            await _context.Set<Entity>().AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;


        }

        public  virtual async Task DeleteAsync(int id)
        {
            var entity = await _context.Set<Entity>().FindAsync(id);
            if (entity != null)
            {
                _context.Set<Entity>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public virtual async Task<List<Entity>?> GetAllList()
        {
            return await _context.Set<Entity>().ToListAsync();
        }


        public  virtual async Task<Entity?> GetByIdAsync(int id)
        {
            return await _context.Set<Entity>().FindAsync(id);
        }
    }
}
