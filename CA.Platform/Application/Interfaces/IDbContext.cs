using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CA.Platform.Application.Interfaces
{
    public interface IDbContext: IDisposable
    {
        DbSet<T> GetDbSet<T>() where T : class;

        DbSet<T> Set<T>() where T : class;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}