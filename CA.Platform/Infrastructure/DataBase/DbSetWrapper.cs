using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CA.Platform.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CA.Platform.Infrastructure.DataBase
{
    public class DbSetWrapper<T> : IDbSet<T> where T : class
    {
        private readonly DbSet<T> _dbSet;

        public DbSetWrapper(DbSet<T> dbSet)
        {
            _dbSet = dbSet;
        }

        public void Add(T baseObject)
        {
            _dbSet.Add(baseObject);
        }

        public IQueryable<T> Include(Expression<Func<T, Property>> expression)
        {
            return _dbSet.Include(expression);
        }

        public void AddRange(IEnumerable<T> baseObjects)
        {
            _dbSet.AddRange(baseObjects);
        }

        public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> expression)
        {
            return _dbSet.FirstOrDefaultAsync(expression);
        }

        public IQueryable<T> Where(Expression<Func<T, bool>> expression)
        {
            return _dbSet.Where(expression);
        }

        public IQueryable<T> Include(string navigationPath)
        {
            return _dbSet.Include(navigationPath);
        }
    }
}