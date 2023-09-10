using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CA.Platform.Application.Interfaces;
using CA.Platform.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CA.Platform.Infrastructure.DataBase
{
    class DataContextWrapper<TContext>: IDbContext where TContext : BaseDbContext
    {
        private readonly TContext _dataContext;

        private readonly Dictionary<Type, PropertyInfo> _dbSets = new ();

        public DataContextWrapper(
            TContext dataContext, 
            IEnumerable<IEntitySaveHandler> saveHandlers)
        {
            _dataContext = dataContext;

            foreach (var property in _dataContext.GetDbSetProperties())
            {
                var entityType = property.PropertyType.GenericTypeArguments.First();
                _dbSets.Add(entityType, property);
            }

            _dataContext.ChangeTracker.StateChanged += (sender, args) =>
            {
                foreach (var handler in saveHandlers)
                {
                    handler.Save(args.Entry);
                }
            };
            
            _dataContext.ChangeTracker.Tracked += (sender, args) =>
            {
                foreach (var handler in saveHandlers)
                {
                    handler.Save(args.Entry);
                }
            }; 
        }

        public DbSet<T> GetDbSet<T>() where T : class
        {
            if (!_dbSets.ContainsKey(typeof(T)))
                throw new NotRegistredClassException(typeof(T));
            
            return _dbSets[typeof(T)].GetValue(_dataContext) as DbSet<T>;
        }

        public DbSet<T> Set<T>() where T : class
        {
            return _dataContext.Set<T>();
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await _dataContext.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _dataContext?.Dispose();
        }
    }
}