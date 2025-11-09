using BankingApp.Core.Domain;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BankingApp.Infraestructure.Persistence.Repositories
{
    public class GenericRepository<Entity> : BaseRepository<Entity> ,IGenericRepository<Entity> where Entity : class
    {
        private readonly BankingContext _context;

        public GenericRepository(BankingContext context) :base(context)
        {
            _context = context;
        }

      
        public  async Task AddRangeAsync(List<Entity> entities)
        {
            await _context.Set<Entity>().AddRangeAsync(entities);
            await _context.SaveChangesAsync();

        }
      

        public virtual async Task DeleteRangeAsync(List<Entity> entities)
        {
             _context.Set<Entity>().RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

      

        public virtual async Task<List<Entity>?> GetAllListWithInclude(List<string> properties)
        {
            var query = _context.Set<Entity>().AsQueryable();
            foreach (var property in properties)
            {
                query = query.Include(property);
            }

            return await query.ToListAsync();
        }

        public virtual IQueryable<Entity> GetAllQuery()
        {
            return _context.Set<Entity>().AsQueryable();
        }
        public virtual IQueryable<Entity> GetAllQueryWithInclude(List<string> properties)
        {
            var query = _context.Set<Entity>().AsQueryable();
            foreach (var property in properties)
            {
                query = query.Include(property);
            }

            return query;
        }

      

        public virtual async Task<Entity?> UpdateAsync(int id, Entity entity)
        {
            var entry = await _context.Set<Entity>().FindAsync(id);

            if (entry != null)
            {
                _context.Entry(entry).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();
            }
            return entry;
        }

        public async Task UpdateRangeAsync(List<Entity> entities)
        {
            _context.Set<Entity>().UpdateRange(entities);
            await _context.SaveChangesAsync();
        }
    }
}
