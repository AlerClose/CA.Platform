using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CA.Platform.Application.Interfaces
{
    public interface IDbSet<T> where T: class
    {
        void Add(T baseObject);
        
        void AddRange(IEnumerable<T> baseObjects);

        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> expression);

        IQueryable<T> Where(Expression<Func<T, bool>> expression);

        IQueryable<T> Include(string navigationPath);
    }
}